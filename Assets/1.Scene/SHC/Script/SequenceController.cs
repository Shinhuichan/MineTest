using PixelCrushers.DialogueSystem;
using UnityEngine;

public class SequenceController : MonoBehaviour
{
    [SerializeField] DialogueSystemTrigger dialogueTrigger;
    public bool IsCurrentTestClear(int experimentNumber)
    {
        int clearCount = 0;
        foreach (var pr in GameManager.I.progreses)
        {
            if (pr.isClear[experimentNumber]) clearCount++;
        }
        return clearCount >= GameManager.I.progreses.Count;
    }

    public void CallPlayer(int experimentNumber)
    {
        if (IsCurrentTestClear(experimentNumber))
            dialogueTrigger.OnUse();
    }
}