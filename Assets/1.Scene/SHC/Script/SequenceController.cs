using System.Collections;
using System.Collections.Generic;
using CustomInspector;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SequenceController : MonoBehaviour
{
    [SerializeField] DialogueSystemTrigger trigger;       // ← Inspector에서 지정
    [SerializeField] Transform actor;                // 교수님 등 화자
    [SerializeField] Transform conversant;           // 플레이어(카메라 등)
    [SerializeField] GameObject[] testObjects;
    [SerializeField][ReadOnly] int testCount = 0;

    [Header("Update Test")]
    [SerializeField] float testUpdateCount = 2f;
    bool alreadyTalk = false;

    private enum EndAction { None, Talk, Test }
    [SerializeField] EndAction pendingEnd = EndAction.None;

    [ReadOnly][SerializeField] XRGrabInteractable[] grabs;
    void Awake()
    {
        grabs = FindObjectsOfType<XRGrabInteractable>();
        foreach (var grab in grabs) { grab.enabled = false; }
        SoundManager.I.PlayBGM("실험실 속 작은 세계_Fix");
    }
    void Start()
    {
        StartCoroutine(CheckTestCount());
    }

    IEnumerator CheckTestCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(testUpdateCount);
            if (!alreadyTalk) { CallPlayer(testCount); }
        }
    }
    #region 호출 조건
    public void EndTalk()
    {
        pendingEnd = EndAction.Talk;
        trigger.OnUse();
    }

    public void CallPlayer(int experimentNumber)
    {
        if (!GameManager.I.IsCurrentTestClear(experimentNumber)) return;
        if (trigger != null)
        {
            trigger.OnUse();
            pendingEnd = EndAction.Test;
            alreadyTalk = true;
            return;
        }
    }
    #endregion 호출 조건
    
    public void OnConversationEnd(Transform actor)
    {
        // Debug.Log($"[Dialogue] end: {actor?.name}, pending={pendingEnd}");
        switch (pendingEnd)
        {
            case EndAction.Talk: TalkEndCore(); break;
            case EndAction.Test: TestEndCore(); break;
        }
        pendingEnd = EndAction.None;
    }

    #region Talk
    private void TalkEndCore()
    {
        foreach (var grab in grabs) { grab.enabled = true; }
    }
    #endregion Talk

    #region Test

    private void TestEndCore()
    {
        if (testObjects != null && testObjects.Length > 0 && testCount >= 0 && testCount < testObjects.Length)
            testObjects[testCount].SetActive(false);


        // 다음 실험 인덱스 안전 처리
        int last = testObjects?.Length > 0 ? testObjects.Length - 1 : -1;
        if (testCount < last)
        {
            testCount++;
            if (testObjects[testCount] != null) testObjects[testCount].SetActive(true);
            alreadyTalk = false; // 다음 대화 트리거 허용
        }
        else { alreadyTalk = true; /* 더 이상 대화 시작 금지*/ }
    }
    #endregion Test
}