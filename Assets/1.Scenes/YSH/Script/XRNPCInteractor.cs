using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRUIController : MonoBehaviour
{
    [Header("Ray Interactor")]
    public XRRayInteractor rayInteractor;

    [Header("UI Objects")]
    public GameObject uiPanel1;
    public GameObject uiPanel2;

    [Header("Dialogue Manager")]
    public DialogueManager dialogueManager;

    private bool uiActive = false;

    void OnEnable()
    {
        rayInteractor.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDisable()
    {
        rayInteractor.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (uiActive) return;

        uiActive = true;

        // UI 켜기
        uiPanel1.SetActive(true);
        uiPanel2.SetActive(true);

        // DialogueManager 시작
        if (dialogueManager != null)
        {
            dialogueManager.ActivateDialogue();
        }
    }

    // UI 내 버튼에서 호출하는 함수
    public void CloseUI()
    {
        uiActive = false;

        uiPanel1.SetActive(false);
        uiPanel2.SetActive(false);

        if (dialogueManager != null)
        {
            dialogueManager.HideDialogue(); // Optional: DialogueManager UI 숨기기
        }
    }
}
