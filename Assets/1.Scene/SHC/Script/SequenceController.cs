using PixelCrushers.DialogueSystem;
using UnityEngine;

public class SequenceController : MonoBehaviour
{
    [SerializeField] DialogueSystemTrigger dialogueTrigger;
    public bool IsCurrentTestClear(int experimentNumber)
    {
        int total = GameManager.I?.progreses?.Count ?? 0;
        if (total <= 0) return false;

        // 범위 체크(0~3만 허용)
        if (experimentNumber < 0 || experimentNumber > 3) return false;

        int clearCount = 0;
        foreach (var pr in GameManager.I.progreses)
            if (pr.isClear[experimentNumber]) clearCount++;

        return clearCount == total;
    }

    public void CallPlayer(int experimentNumber)
    {
        if (dialogueTrigger == null) return;
        if (IsCurrentTestClear(experimentNumber))
            dialogueTrigger.OnUse();
    }
}