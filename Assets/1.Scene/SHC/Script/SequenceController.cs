using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UIElements;

public class SequenceController : MonoBehaviour
{
    [SerializeField] DialogueSystemTrigger trigger;       // ← Inspector에서 지정
    [SerializeField] Transform actor;                // 교수님 등 화자
    [SerializeField] Transform conversant;           // 플레이어(카메라 등)
    [SerializeField] GameObject[] testObjects;
    int testCount = 0;
    bool alreadyTalk = false;

    bool IsReady => GameManager.I != null && GameManager.I.progreses != null && GameManager.I.progreses.Count > 0;
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
            Debug.Log("대화");
            alreadyTalk = true;
            return;
        }
    }
    public void TestEnd()
    {
        testObjects[testCount].SetActive(false);
        if (testCount < 4) { testCount++; }
        else return;
        testObjects[testCount].SetActive(true);
        alreadyTalk = false;
    }
}