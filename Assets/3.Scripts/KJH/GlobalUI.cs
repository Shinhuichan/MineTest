using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEditor;
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
        redScreen = pivot.Find("RedScreen").gameObject;
        redScreenRdr = redScreen.GetComponent<MeshRenderer>();
        redScreenMr = redScreenRdr.sharedMaterial;
        narration = pivot.Find("Narration");
        narrationText = pivot.Find("Narration/Text").GetComponent<Text>();
        gameOverPop = transform.Find("GameOver").gameObject;
        gameClearPop = transform.Find("GameClear").gameObject;
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
    [HideInInspector] public bool isShowReactWaterNoReact;
    [HideInInspector] public bool isShowReactHClNoReact;
    [HideInInspector] public bool isShowMohsLower;
    [HideInInspector] public bool isShowMohsUpper;
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
    [HideInInspector] public SFX narrationSFX;
    public void Narration(string str, float duration)
    {
        narrationTween?.Kill();
        narrationText.text = str;
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
    GameObject redScreen;
    GameObject gameOverPop;
    MeshRenderer redScreenRdr;
    Material redScreenMr;
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
        yield return new WaitForSeconds(1f);
        GameManager.I.StopPlayer();
        GameManager.I.LookTarget(pos);
        yield return new WaitForSeconds(4f);
        ParticleManager.I.PlayParticle("Explosion", pos, Quaternion.identity, null);
        var pb = ParticleManager.I.PlayParticle("Explosion", pos, Quaternion.identity, null);
        pb.transform.localScale = 0.2f * Vector3.one;
        SoundManager.I.PlaySFX("Explosion", pos, null, 0.4f, 1.2f);
        yield return new WaitForSeconds(0.5f);
        ShowRedScreen();
        yield return new WaitForSeconds(0.5f);
        SoundManager.I.PlaySFX("GameOver", redScreen.transform.position, null, 0.6f);
        yield return new WaitForSeconds(0.5f);
        GameManager.I.LookTarget(gameOverPop.transform.position);
        SoundManager.I.PlaySFX("GameOverTTS", gameOverPop.transform.position, null);
        gameOverPop.SetActive(true);
        Transform child = gameOverPop.transform.GetChild(0);
        Vector3 scale = child.localScale;
        child.localScale = 0.5f * scale.x * Vector3.one;
        child.DOScale(1f * scale.x, 1.2f).SetEase(Ease.OutBounce);
        GameManager.I.ResumeHand();
    }
    public void ShowRedScreen()
    {
        redScreen.gameObject.SetActive(true);
        redScreenMr.SetColor("_Color", new Color(1f, 0f, 0f, 0f));
        redScreenMr.DOColor(new Color(1f, 0f, 0f, 0.2f), "_Color", 1.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            redScreenMr.SetColor("_Color", new Color(1f, 0f, 0f, 0f));
            redScreen.gameObject.SetActive(false);
        });
    }
    public void Restart()
    {
        GameManager.I.ResumePlayer();
        GameManager.I.ChangeScene(0);
    }
    public void GameQuit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
    #endregion
    #region Game Clear
    GameObject gameClearPop;
    public void GameClear()
    {
        StartCoroutine(nameof(GameCler_co));
    }
    IEnumerator GameCler_co()
    {
        GameManager.I.StopPlayer();
        GameManager.I.LookTarget(gameClearPop.transform.position);
        yield return new WaitForSeconds(1.8f);
        SoundManager.I.PlaySFX("GameClear", gameClearPop.transform.position, null, 0.6f);
        StartCoroutine(nameof(ConfetiEffect));
        yield return new WaitForSeconds(0.5f);
        SoundManager.I.PlaySFX("GameClearTTS", gameClearPop.transform.position, null);
        gameClearPop.SetActive(true);
        Transform child = gameClearPop.transform.GetChild(0);
        Vector3 scale = child.localScale;
        child.localScale = 0.5f * scale.x * Vector3.one;
        child.DOScale(1f * scale.x, 1.2f).SetEase(Ease.OutBounce);
        GameManager.I.ResumeHand();
    }
    IEnumerator ConfetiEffect()
    {
        Vector3 randomPos;
        for (int i = 0; i < 12; i++)
        {
            int count = Random.Range(1, 5);
            for (int j = 0; j < count; j++)
            {
                randomPos = transform.position + new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(0f, 1f), Random.Range(-1.5f, 1.5f));
                SoundManager.I.PlaySFX("Confetti", randomPos);
                string str = "";
                if (Random.value > 0.5f)
                    str = "Confetti1";
                else
                    str = "Confetti2";
                var pa = ParticleManager.I.PlayParticle(str, randomPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), null);
                yield return new WaitForSeconds(Random.Range(0.01f, 0.1f));
            }
            yield return new WaitForSeconds(Random.Range(0.5f, 1.2f));
        }
    }
    #endregion


}
