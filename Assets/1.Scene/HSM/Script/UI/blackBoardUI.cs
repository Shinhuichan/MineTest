using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class blackBoardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mainExperimentTiltle;
    [SerializeField] private List<TMP_Text> objectStatusTexts;
    
    [SerializeField] private List<string> experimentNames;

    public GameObject experimentStatusPanel;
    public GameObject testPanel;

    private int currentlyDisplayedIndex = -1;

    public void Start()
    {
        ShowExperimentView();
    }
    public void ShowExperimentView()
    {
        experimentStatusPanel.SetActive(true);
        testPanel.SetActive(false);
    }
    public void ShowTestView()
    {
        experimentStatusPanel.SetActive(false);
        testPanel.SetActive(true);
    }
    public void ShowExperimentStatus(int experimentIndex)
    {
        //보여중 실험이 유효한 범위 내에 있는지 확인
        if(experimentIndex < 0 || experimentIndex >= experimentNames.Count)
        {
            Debug.Log(experimentNames.Count);
            Debug.LogError("잘못된 실험 인덱스 입니다");
            return;
        }
        currentlyDisplayedIndex = experimentIndex;


        // 1. 제목 업데이트 
        mainExperimentTiltle.text = experimentNames[currentlyDisplayedIndex];

        // 2. O/X 상태 업데이트
        UpdateStatusDisplay();
    }

    public void UpdateStatusDisplay()
    {
        if (GameManager.I == null || GameManager.I.progreses == null) return;

        int totalProgressCount = GameManager.I.progreses.Count;

        int loopCount = Mathf.Min(objectStatusTexts.Count, totalProgressCount);

        for(int i = 0; i < loopCount; i++)
        {
            // i번째 광물의 현재 표시 중인 실험의 완료 여부 확인
            bool isDone = GameManager.I.progreses[i].isClear[currentlyDisplayedIndex];

            if(isDone)
            {
                objectStatusTexts[i].text = "O";
                objectStatusTexts[i].color = Color.green;
            }
            else
            {
                objectStatusTexts[i].text = "X";
                objectStatusTexts[i].color = Color.red;
            }
        }

    }

}
