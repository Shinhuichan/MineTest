using System.Collections.Generic;
using System.Collections;
using UnityEngine;
public class SoundManager : SingletonBehaviour<SoundManager>
{
    protected override bool IsDontDestroy() => false;
    [SerializeField] List<AudioClip> bgmList = new List<AudioClip>();
    [SerializeField] List<AudioClip> sfxList = new List<AudioClip>();
    [SerializeField] List<AudioClip> ambienceList = new List<AudioClip>();
    [SerializeField] SFX sfxPrefab;
    AudioSource ausBGM0;
    AudioSource ausBGM1;
    AudioClip lastBGMclip;
    [ReadOnlyInspector][SerializeField] float volumeBGM = 1f;
    [ReadOnlyInspector][SerializeField] float volumeSFX = 1f;
    protected override void Awake()
    {
        base.Awake();
        transform.GetChild(0).TryGetComponent(out ausBGM0);
        transform.GetChild(1).TryGetComponent(out ausBGM1);
    }
    public void PlayBGM(string Name, float crossFadeTime = 0f)
    {
        AudioClip clip;
        if(Name != "")
        {
            int find = -1;
            for (int i = 0; i < bgmList.Count; i++)
            {
                if (bgmList[i].name == Name)
                {
                    //Debug.Log(bgmList[i].name);
                    find = i;
                    break;
                }
            }
            if (find == -1) return;
            clip = bgmList[find];
        }
        else clip = null;
        if (crossFadeTime == 0f)
        {
            ausBGM0.clip = clip;
            ausBGM0.Play();
        }
        else
        {
            StopCoroutine("PlayBGM_co");
            lastBGMclip = ausBGM0.clip;
            float lastTime = ausBGM0.time;
            ausBGM0.clip = clip;
            ausBGM1.clip = lastBGMclip;
            ausBGM0.Play();
            ausBGM1.Play();
            ausBGM1.time = lastTime;
            StartCoroutine("PlayBGM_co", crossFadeTime);
        }
    }
    IEnumerator PlayBGM_co(float crossFadeTime)
    {
        float startTime = Time.time;
        float startVolume = ausBGM0.volume * 0.8f;
        while (Time.time - startTime < crossFadeTime)
        {
            yield return null;
            float t = (Time.time - startTime) / crossFadeTime;
            ausBGM0.volume = Mathf.Lerp(0f, volumeBGM, t);
            ausBGM1.volume = Mathf.Lerp(startVolume, 0f, t * 2.5f);
        }
        ausBGM0.volume = volumeBGM;
        ausBGM1.volume = 0f;
        ausBGM1.Stop();
    }
    public SFX PlaySFX(string Name, Vector3 pos, Transform parent = null, float spatialBlend = 0.8f, float volume = 1f)
    {
        int find = -1;
        for (int i = 0; i < sfxList.Count; i++)
        {
            if (sfxList[i].name == Name)
            {
                //Debug.Log(sfxList[i].name);
                find = i;
                break;
            }
        }
        if (find == -1) return null;
        if (parent == null) parent = transform;
        PoolBehaviour pb = PoolManager.I?.Spawn(sfxPrefab, pos, Quaternion.identity, parent);
        SFX _pb = pb as SFX;
        float vol = volumeSFX * volume;
        _pb.Play(sfxList[find], vol, sfxList[find].length, spatialBlend);
        return _pb;
    }
    public SFX PlaySFX(string Name, Transform parent = null)
    {
       return PlaySFX(Name, Vector3.zero,null);
    }
    void Start()
    {
        volumeBGM = PlayerPrefs.GetFloat("volumeBGM", 1f);
        volumeSFX = PlayerPrefs.GetFloat("volumeSFX", 1f);
        ausBGM0.loop = true;
        ausBGM1.loop = true;
    }
    // 슬라이더 드래그시 아래 실행
    public void SetVolumeBGM(float ratio)
    {
        volumeBGM = ratio;
        ausBGM0.volume = volumeBGM;
        ausBGM1.volume = volumeBGM;
    }
    // 슬라이더 드래그시 아래 실행
    public void SetVolumeSFX(float ratio)
    {
        volumeSFX = ratio;
    }
    // 슬라이더 포인터업 엔드시 아래 실행
    public void SetVolumeEnd()
    {
        PlayerPrefs.SetFloat("volumeBGM", volumeBGM);
        PlayerPrefs.SetFloat("volumeSFX", volumeSFX);
        PlayerPrefs.Save();
    }

}
