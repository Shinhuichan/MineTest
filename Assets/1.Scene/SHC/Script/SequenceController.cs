using System.Collections;
using System.Linq;
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
    [SerializeField] GameObject ores;
    [SerializeField][ReadOnly] int testCount = 0;

    [Header("Update Test")]
    [SerializeField] float testUpdateCount = 2f;
    bool alreadyTalk = false;
    private enum EndAction { None, Talk, Test }
    [SerializeField] EndAction pendingEnd = EndAction.None;
    [ReadOnly][SerializeField] XRGrabInteractable[] grabs;
    ResetButton rb;
    void Awake()
    {
        grabs = FindObjectsOfType<XRGrabInteractable>();

        if (ores == null)
        {
            ores = FindObjectOfType<ObjectInfo>().transform.parent.gameObject;
        }
        foreach (var grab in grabs) { grab.enabled = false; }
        SoundManager.I.PlayBGM("실험실 속 작은 세계_Fix", 0.6f);
        rb = FindObjectOfType<ResetButton>(true);
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
            if (GameManager.I.progreses.Count() > 0 && !alreadyTalk)
                { CallPlayer(testCount); }
                
        }
    }
    #region 호출 조건
    public void EndTalk()
    {
        pendingEnd = EndAction.Talk;
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
        Debug.Log($"[Seq] CallPlayer({experimentNumber}), alreadyTalk={alreadyTalk}");
    }
    #endregion 호출 조건
    public void OnConversationEnd(Transform actor)
    {
        switch (pendingEnd)
        {
            case EndAction.Talk: TalkEndCore(); Debug.Log("이야기 끝"); break;
            case EndAction.Test: TestEndCore(); Debug.Log("실험 끝"); break;
        }
        pendingEnd = EndAction.None;
        Debug.Log($"[Seq] OnConversationEnd pending={pendingEnd} → TestEndCore/TalkEndCore");
        rb.QuickReset();
    }

    #region Talk
    private void TalkEndCore()
    {
        foreach (var grab in grabs) { grab.enabled = true; }
    }
    #endregion Talk

    #region Test

    //GameManager가 호출할 전용함수
    public void TriggerTestEndDialogue(int experimentNumber)
    {
        //  이미 대화가 진행중이면 중복 호출 방지
        if (alreadyTalk) return;
        
        // GameManager로 부터 호출을 받았으므로
        if(GameManager.I.IsCurrentTestClear(experimentNumber))
        {
            if(trigger != null)
            {
                Debug.Log($"GameManager의 요청으로 {experimentNumber}번 실험 완료 대화를 시작합니다.");
                alreadyTalk = true;
                trigger.OnUse();
                pendingEnd = EndAction.Test;
            }
        }
 Debug.Log($"[Seq] TriggerTestEndDialogue({experimentNumber}) pending=Test");

    }

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

            GameManager.I.PrepareNextExperimentUI();

            if (testCount == testObjects.Length - 1) ores.SetActive(false);
            alreadyTalk = false; // 다음 대화 트리거 허용
        }
        else { alreadyTalk = true; /* 더 이상 대화 시작 금지*/ }
    }
    #endregion Test
}