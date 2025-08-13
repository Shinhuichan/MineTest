
using TMPro;
using UnityEngine;


public class DisplayPanel : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI isReactingText;
    public TextMeshProUGUI HardnessText;


    public GameObject panelRoot;

    public GameObject[] checks;


    private ObjectInfo currentInfo; // 현재 보고 있는 오브젝트 정보 저장
    private void OnEnable()
    {
        UIManager.Instance.OnObjectHovered += OnHover;
        UIManager.Instance.OnObjectHoverExited += ClearDisplay;
        UIManager.Instance.OnExperimentUpdated += OnExperimentUpdate;
    }
    private void OnDisable()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnObjectHovered -= OnHover;
            UIManager.Instance.OnObjectHoverExited -= ClearDisplay;
            UIManager.Instance.OnExperimentUpdated -= OnExperimentUpdate;

        }
    }

    private void OnHover(ObjectInfo info)
    {
        currentInfo = info;
        panelRoot.SetActive(true);
        RefreshDisplay();
    }
    private void OnExperimentUpdate(ObjectInfo updatedInfo)
    {

        // 지금 보고 있는 화면이 맞는지 확인
        if (currentInfo != null && currentInfo == updatedInfo)
        {
            RefreshDisplay(); // 맞다면 새로고침
        }
    }

    private void RefreshDisplay()
    {
        if (currentInfo == null || currentInfo.oreData == null) return;

        var progress = currentInfo.GetComponent<ExperimentProgress_H>();

        if(progress == null)
        {
            Debug.Log($"{currentInfo.name}에 ExperimentProgress 컴포넌트가 없습니다");
            return;
        }

        nameText.text = currentInfo.oreData.type.ToString();

        checks[0].SetActive(progress.isMicroScopeCheckd);

        checks[1].SetActive(progress.isHardnessChecked);
        HardnessText.text = progress.isHardnessChecked ? currentInfo.oreData.hardness.ToString() : "??";

        checks[2].SetActive(progress.isReactingChecked);
        isReactingText.text = progress.isReactingChecked ? currentInfo.oreData.isReactingToChem.ToString() : "??";
        }

    private void ClearDisplay()
    {
        currentInfo = null;
        panelRoot.SetActive(false);
    }
}