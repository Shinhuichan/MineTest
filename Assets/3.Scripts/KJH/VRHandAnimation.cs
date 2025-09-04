using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
public class VRHandAnimation : MonoBehaviour
{
    public enum Type
    {
        Left,
        Right,
    }
    public Type type;
    Animator anim;
    [SerializeField] private InputActionAsset inputAsset;
    void Awake()
    {
        TryGetComponent(out anim);
    }
    void OnEnable()
    {
        // 왼손 검지 --> 프라이머리 트리거 (검지 트리거)
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").performed += LPT_Down;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").canceled += LPT_Up;
        // 오른손 검지 --> 프라이머리 트리거 (검지 트리거)
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").performed += RPT_Down;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").canceled += RPT_Up;
        // 왼손 중지+약지+소지 ---> 세컨더리 트리거 (중지 트리거)
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Select").performed += LST_Down;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Select").canceled += LST_Up;
        // 오른손 중지+약지+소지 ---> 세컨더리 트리거 (중지 트리거)
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Select").performed += RST_Down;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Select").canceled += RST_Up;
        // 왼손 엄지 ---> A,B,X,Y 버튼 전부 애니매이션 엄지로 통일
        // 미구현
        // 본 프로젝트에서는 A,B,X,Y 버튼 아무것도 사용하지 않고. 검지,중지 트리거 2개만 사용하므로 미구현함
    }
    void OnDisable()
    {
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").performed -= LPT_Down;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").canceled -= LPT_Up;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").performed -= RPT_Down;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").canceled -= RPT_Up;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Select").performed -= LST_Down;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Select").canceled -= LST_Up;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Select").performed -= RST_Down;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Select").canceled -= RST_Up;
        // 왼손 엄지 ---> A,B,X,Y 버튼 전부 애니매이션 엄지로 통일
        // 미구현
        // 본 프로젝트에서는 A,B,X,Y 버튼 아무것도 사용하지 않고. 검지,중지 트리거 2개만 사용하므로 미구현함
    }
    Tween tweenLPT;
    void LPT_Down(InputAction.CallbackContext context)
    {
        if (type != Type.Left) return;
        tweenLPT?.Kill();
        tweenLPT = DOTween.To(() => anim.GetFloat("Index"), x => anim.SetFloat("Index", x), 1f, 0.5f);
    }
    void LPT_Up(InputAction.CallbackContext context)
    {
        if (type != Type.Left) return;
        tweenLPT?.Kill();
        tweenLPT = DOTween.To(() => anim.GetFloat("Index"), x => anim.SetFloat("Index", x), 0f, 0.5f);
    }
    Tween tweenRPT;
    void RPT_Down(InputAction.CallbackContext context)
    {
        if (type != Type.Right) return;
        tweenRPT?.Kill();
        tweenRPT = DOTween.To(() => anim.GetFloat("Index"), x => anim.SetFloat("Index", x), 1f, 0.5f);
    }
    void RPT_Up(InputAction.CallbackContext context)
    {
        if (type != Type.Right) return;
        tweenRPT?.Kill();
        tweenRPT = DOTween.To(() => anim.GetFloat("Index"), x => anim.SetFloat("Index", x), 0f, 0.5f);
    }
    Tween tweenLST;
    void LST_Down(InputAction.CallbackContext context)
    {
        if (type != Type.Left) return;
        tweenLST?.Kill();
        tweenLST = DOTween.To(() => anim.GetFloat("ThreeFingers"), x => anim.SetFloat("ThreeFingers", x), 1f, 0.5f);
    }
    void LST_Up(InputAction.CallbackContext context)
    {
        if (type != Type.Left) return;
        tweenLST?.Kill();
        tweenLST = DOTween.To(() => anim.GetFloat("ThreeFingers"), x => anim.SetFloat("ThreeFingers", x), 0f, 0.5f);
    }
    Tween tweenRST;
    void RST_Down(InputAction.CallbackContext context)
    {
        if (type != Type.Right) return;
        tweenRST?.Kill();
        tweenRST = DOTween.To(() => anim.GetFloat("ThreeFingers"), x => anim.SetFloat("ThreeFingers", x), 1f, 0.5f);
    }
    void RST_Up(InputAction.CallbackContext context)
    {
        if (type != Type.Right) return;
        tweenRST?.Kill();
        tweenRST = DOTween.To(() => anim.GetFloat("ThreeFingers"), x => anim.SetFloat("ThreeFingers", x), 0f, 0.5f);
    }



    







}
