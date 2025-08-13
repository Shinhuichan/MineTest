
using TMPro;
using UnityEngine;


public class DisplayPanel : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI isReactingText;
    public TextMeshProUGUI HardnessText;


    public GameObject panelRoot;

    public GameObject[] checks;


    private ObjectInfo currentInfo; // ���� ���� �ִ� ������Ʈ ���� ����
    private void Start()
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

        // ���� ���� �ִ� ȭ���� �´��� Ȯ��
        if (currentInfo != null && currentInfo == updatedInfo)
        {
            RefreshDisplay(); // �´ٸ� ���ΰ�ħ
        }
    }

    private void RefreshDisplay()
    {
        if (currentInfo == null || currentInfo.oreData == null) return;

        var progress = currentInfo.GetComponent<ExperimentProgress_H>();

        if(progress == null)
        {
            Debug.Log($"{currentInfo.name}�� ExperimentProgress ������Ʈ�� �����ϴ�");
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