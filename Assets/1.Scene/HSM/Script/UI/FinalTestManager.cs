using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class FinalTestManager : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private XRSocketInteractor[] answerSockets;
    
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;

    private void Start()
    {
        // 모든 소켓에 아이템이 채워지는 이벤트를 감지하도록 등록합니다.
        foreach (var socket in answerSockets)
        {
            socket.selectEntered.AddListener(OnSocketFilled);
        }
        if(feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }
    // 스크립트가 비활성화될 때 이벤트 연결을 해제하여 메모리 누수를 방지합니다.
    private void OnDestroy()
    {
        foreach(var socket in answerSockets)
        {
            socket.selectEntered.RemoveListener(OnSocketFilled);
        }
    }

    private void OnSocketFilled(SelectEnterEventArgs args)
    {
        bool allSocketsFull = true;
        foreach (var socket in answerSockets)
        {
            if(!socket.hasSelection)
            {
                allSocketsFull = false;
                break;  // 하나라도 비어있으면 검사 중단
            }
        }
        if(allSocketsFull)
        {
            Debug.Log("모든 소켓이 채워졌습니다. 자동채점을 시작합니다.");
            checkAnswer();
        }

    }

    public void checkAnswer()
    {
        OreData[] submittedData = new OreData[answerSockets.Length];
        for(int i =0; i < answerSockets.Length; i++)
        {
            IXRSelectInteractable socketedObject = answerSockets[i].GetOldestInteractableSelected();
            if(socketedObject != null)
            {
                answerSheet sheet = socketedObject.transform.GetComponent<answerSheet>();
                if(sheet != null)
                {
                    submittedData[i] = sheet.associatedOreData;
                }
            }
        }
        GameManager.I.CheckFinalAnswer(submittedData);
    }
    public void ShowFeedback(int correctCount, int totalQuestions)
    {
        if (feedbackPanel == null || feedbackText == null) return;

        // 정답일 경우
        if(correctCount == totalQuestions)
        {
            feedbackText.text = "정답입니다!";
        }
        // 오답일 경우
        else
        {
            feedbackText.text = $"다시 한번 생각해보세요.\n정답 ({correctCount} / {totalQuestions}";
        }
        StartCoroutine(FeedbackRoutine());
    }
    private IEnumerator FeedbackRoutine()
    {
        feedbackPanel.SetActive(true);
        
        yield return new WaitForSeconds(2f);
        
        feedbackPanel.SetActive(false);
    }

}
