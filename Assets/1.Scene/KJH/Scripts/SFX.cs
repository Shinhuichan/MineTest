using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class SFX : PoolBehaviour
{
    #region UniTask Setting
    CancellationTokenSource cts;
    void OnEnable()
    {
        cts = new CancellationTokenSource();
        Application.quitting += UniTaskCancel;
    }
    void OnDisable() { UniTaskCancel(); }
    void OnDestroy() { UniTaskCancel(); }
    void UniTaskCancel()
    {
        try
        {
            cts?.Cancel();
            cts?.Dispose();
        }
        catch (System.Exception e)
        {

            Debug.Log(e);
        }
        cts = null;
    }
    #endregion
    public AudioSource aus;
    public void Play(AudioClip clip, float vol, float time, float is3d)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        PlayUT(clip, vol, time, is3d, cts.Token).Forget();
    }
    async UniTask PlayUT(AudioClip clip, float vol, float time, float is3d, CancellationToken token)
    {
        await UniTask.Delay(2, ignoreTimeScale: true, cancellationToken:token);
        aus.loop = false;
        aus.clip = clip;
        aus.spatialBlend = is3d;
        aus.volume = vol;
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        if (!aus.enabled) aus.enabled = true;
        await UniTask.Delay(2, ignoreTimeScale: true, cancellationToken:token);
        aus.pitch = Random.Range(0.97f,1.03f);
        aus.Play();
        await UniTask.Delay((int)(1000f * (time + 0.2f)),ignoreTimeScale: true , cancellationToken:token);
        base.Despawn();
    }
}
