using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingController : MonoBehaviour
{
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

    void OnEnable()
    {
        // 초기값(우선순위: SoundManager → PlayerPrefs)
        float bgm = PlayerPrefs.GetFloat("volumeBGM");
        float sfx = PlayerPrefs.GetFloat("volumeSFX");

        // 슬라이더 이벤트 연결 전에 값 세팅(연쇄 호출 방지)
        bgmSlider.SetValueWithoutNotify(Mathf.Clamp01(bgm));
        sfxSlider.SetValueWithoutNotify(Mathf.Clamp01(sfx));

        bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    void OnDisable()
    {
        bgmSlider.onValueChanged.RemoveListener(OnBgmChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
    }

    public void OnBgmChanged(float v) => SoundManager.I?.SetVolumeBGM(v);
    public void OnSfxChanged(float v) => SoundManager.I?.SetVolumeSFX(v);
}