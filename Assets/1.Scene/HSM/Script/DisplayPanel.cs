
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DisplayPanel : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image objectImage;
    public GameObject panelRoot;

    private void OnEnable()
    {
        UIManager.Instance.OnObjectHovered += UpdateDisplay;
        UIManager.Instance.OnObjectHoverExited += ClearDisplay;
    }
    private void OnDisable()
    {
        UIManager.Instance.OnObjectHovered -= UpdateDisplay;
        UIManager.Instance.OnObjectHoverExited -= ClearDisplay;
    }

    private void UpdateDisplay(objectInfo info)
    {
        panelRoot.SetActive(true);
        nameText.text = info.objectName;
        descriptionText.text = info.description;

        if (info.objImage != null)
        {
            objectImage.sprite = info.objImage;
            objectImage.color = Color.white;
        }
        else
        {
            objectImage.color = new Color(1, 1, 1, 0);
        }
    }
    private void ClearDisplay()
    {
        panelRoot.SetActive(false);
    }
}
