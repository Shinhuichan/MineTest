using CustomInspector;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject settingUI;
    [SerializeField, ReadOnly] private bool toggleBool;
    public void SetToggleUI()
    {
        menuUI.SetActive(!toggleBool);
        toggleBool = !toggleBool;
    }

    public void EnterSettingUI()
    {
        settingUI.SetActive(true);
    }
    public void ExitSettingUI()
    {
        settingUI.SetActive(false);
    }
}