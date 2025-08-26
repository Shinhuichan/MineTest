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
    bool alreadyTalk = false;

    [ReadOnly][SerializeField] XRGrabInteractable[] grabs;
    void Awake()
    {
        grabs = FindObjectsOfType<XRGrabInteractable>();
        foreach (var grab in grabs) { grab.enabled = false; }
        SoundManager.I.PlayBGM("실험실 속 작은 세계");
    }
    void LateUpdate()
    {
        if (!alreadyTalk)
            CallPlayer(testCount);
    }

    public void CallPlayer(int experimentNumber)
    {
        if (!GameManager.I.IsCurrentTestClear(experimentNumber)) return;
        if (trigger != null)
        {
            trigger.OnUse();
            alreadyTalk = true;
            return;
        }
    }
    public void OnConversationEnd(Transform actor)  // 대화 종료 브로드캐스트 직접 수신
    {
        Debug.Log($"[Dialogue] OnConversationEnd by {actor?.name}");
        TestEndCore();
    }

    #region Talk
    public void TalkEnd(Transform _)
    { TalkEndCore(); }
    private void TalkEndCore()
    {
        foreach (var grab in grabs) { grab.enabled = true; }
    }
    #endregion Talk

    #region Test
    public void TestEnd(Transform _)               // UnityEvent(Dynamic Transform)용
    { TestEndCore(); }

    public void TestEnd()                           // 매뉴얼 호출용(인자 없는 버튼 등)
    { TestEndCore(); }

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