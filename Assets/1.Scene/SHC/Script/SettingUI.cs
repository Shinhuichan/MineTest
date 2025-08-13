using CustomInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingUI : MonoBehaviour
{
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject settingUI;
    [SerializeField, ReadOnly] private bool toggleBool;

    #region UI Active
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
    #endregion

    public void SceneRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GameQuit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}