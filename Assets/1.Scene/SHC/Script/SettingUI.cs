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
        SoundManager.I.PlaySFX("KeyboardNoise");
        menuUI.SetActive(!toggleBool);
        toggleBool = !toggleBool;
    }
    public void EnterSettingUI()
    {
        SoundManager.I.PlaySFX("KeyboardNoise");
        settingUI.SetActive(true);
    }
    public void ExitSettingUI()
    {
        SoundManager.I.PlaySFX("KeyboardNoise");
        SoundManager.I.SetVolumeBGM(bgmSlider.value);
        SoundManager.I.SetVolumeSFX(sfxSlider.value);
        SoundManager.I.SetVolumeEnd();
        settingUI.SetActive(false);
    }
    #endregion

    public void SceneRestart()
    {
        SoundManager.I.PlaySFX("KeyboardNoise");
        GameManager.I.ChangeScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GameQuit()
    {
        SoundManager.I.PlaySFX("KeyboardNoise");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }
}