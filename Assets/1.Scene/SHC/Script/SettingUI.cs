using CustomInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject settingUI;
    [SerializeField, ReadOnly] private bool toggleBool;

    [Header("Slider")]
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

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
        SoundManager.I.SetVolumeBGM(bgmSlider.value);
        SoundManager.I.SetVolumeSFX(sfxSlider.value);
        SoundManager.I.SetVolumeEnd();
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