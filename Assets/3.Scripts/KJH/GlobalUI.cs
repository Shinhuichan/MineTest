using UnityEngine;
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
    }
    void OnEnable()
    {
        camera = Camera.main;
    }
    void OnDestroy()
    {
        fadeMr.color = new Color(0f, 0f, 0f, 0.3f);   
    }
    void Update()
    {
        pivot.position = camera.transform.position;
    }
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

    #endregion
    #region Accident

    #endregion
    #region NextToDo

    #endregion



}
