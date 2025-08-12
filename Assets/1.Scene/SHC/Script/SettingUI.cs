using CustomInspector;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [SerializeField] GameObject settingUI;
    [SerializeField, ReadOnly] private bool toggleBool;
    public void SetToggleUI()
    {
        settingUI.SetActive(!toggleBool);
        toggleBool = !toggleBool;
    }
}