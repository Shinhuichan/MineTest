using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class GlobalUI : SingletonBehaviour<GlobalUI>
{
    protected override bool IsDontDestroy() => false;
    Transform pivot;
    Camera camera;
    protected override void Awake()
    {
        pivot = transform.GetChild(0);
        fade = pivot.Find("Fade").gameObject;
        fadeRdr = fade.GetComponent<MeshRenderer>();
        fadeMr = fadeRdr.sharedMaterial;
        narration = pivot.Find("Narration");
        narrationText = pivot.Find("Narration/Text").GetComponent<Text>();
    }
    void OnEnable()
    {
        camera = Camera.main;
        pivot.parent = camera.transform;
        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;
        pivot.localScale = Vector3.one;
    }
    void OnDestroy()
    {
        fadeMr.color = new Color(0f, 0f, 0f, 0.3f);
    }
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        Narration("먼저 선생님과 대화부터 해보자..", 3.2f);
    }
    [HideInInspector] public bool isShowMohsSameHardness;
    [HideInInspector] public bool isShowExplosionText;
    #region Fade
    GameObject fade;
    MeshRenderer fadeRdr;
    Material fadeMr;
    Sequence sequenceFade;
    public void FadeOut(float time)
    {
        if (time == 0)
        {
            fade.gameObject.SetActive(true);
            fadeMr.SetColor("_Color", new Color(0f, 0f, 0f, 1f));
            return;
        }
        //시작
        sequenceFade.Kill();
        fade.gameObject.SetActive(true);
        fadeMr.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
        //진행
        Tween tween;
        tween = fadeMr.DOColor(new Color(0f, 0f, 0f, 1f), "_Color", time).SetEase(Ease.OutQuad);
        sequenceFade?.Append(tween);
    }
    public void FadeIn(float time)
    {
        if (time == 0)
        {
            fade.gameObject.SetActive(false);
            fadeMr.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
            return;
        }
        //시작
        sequenceFade.Kill();
        DOTween.Kill(fade.gameObject);
        DOTween.Kill(fadeMr);
        fade.gameObject.SetActive(true);
        fadeMr.SetColor("_Color", new Color(0f, 0f, 0f, 1f));
        //진행
        Tween tween;
        tween = fadeMr.DOFade(0f, 3.45f).SetEase(Ease.InSine).OnComplete(() => fade.gameObject.SetActive(false));
        sequenceFade?.Append(tween);
    }
    #endregion
    #region Narration
    Transform narration;
    Text narrationText;
    Tween narrationTween;
    public void Narration(string str, float duration)
    {
        narrationText.text = str;
        narrationTween?.Kill();
        narrationText.color = new Color(narrationText.color.r, narrationText.color.g, narrationText.color.b, 0f);
        narrationTween = narrationText.DOFade(1f, 1f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            DOVirtual.DelayedCall(duration, () =>
            {

            }).OnComplete(() =>
            {
                narrationText.DOFade(0f, 1f).SetEase(Ease.OutSine);
            });
        });
    }
    #endregion
    #region Game Over
    public IEnumerator SmallFire(Vector3 pos)
    {
        yield return new WaitForSeconds(3f);
        var pb = ParticleManager.I.PlayParticle("Fire", pos, Quaternion.identity, null);
        pb.transform.localScale = 0.1f * Vector3.one;
        SoundManager.I.PlaySFX("BurnSmall", pos, null, 0.4f, 1.2f);
    }
    public IEnumerator BigFire(Vector3 pos)
    {
        yield return new WaitForSeconds(3f);
        var pb = ParticleManager.I.PlayParticle("Fire", pos, Quaternion.identity, null);
        pb.transform.localScale = 0.6f * Vector3.one;
        SoundManager.I.PlaySFX("Burn", pos, null, 0.4f, 1.2f);
    }
    public IEnumerator Explosion(Vector3 pos)
    {
        yield return new WaitForSeconds(1.5f);
        if (!isShowExplosionText)
        {
            Narration("어..? 어? 너무 많이 부었나..", 4f);
            isShowExplosionText = true;
        }
        yield return new WaitForSeconds(1.5f);
        GameManager.I.StopPlayer();
        yield return new WaitForSeconds(3f);
        ParticleManager.I.PlayParticle("Explosion", pos, Quaternion.identity, null);
        SoundManager.I.PlaySFX("Explosion", pos, null, 0.4f, 1.2f);
        yield return new WaitForSeconds(1.5f);

    }
    #endregion



}
