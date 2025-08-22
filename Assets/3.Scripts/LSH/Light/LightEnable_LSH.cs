using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightEnable_LSH : MonoBehaviour
{
    [Header("Fan Target")]
    public GameObject fanObject;

    [Header("Speed (deg/sec)")]
    [Tooltip("처음 천천히 도는 속도")]
    public float idleSpeed = 90f;
    [Tooltip("최대 회전 속도 (rotateSpeed 역할)")]
    public float maxSpeed = 720f;

    [Header("Accel/Decel (deg/sec^2)")]
    [Tooltip("가속도 (높을수록 빨리 올라감)")]
    public float accel = 600f;
    [Tooltip("감속도 (높을수록 빨리 멈춤)")]
    public float decel = 500f;

    [Header("Warm-up")]
    [Tooltip("전류가 통해도 이 시간 동안은 느리게(idleSpeed) 돌다가 이후 최대속도로 가속")]
    public float warmupSeconds = 1.0f;

    [Header("Optional Pivot (회전 기준)")]
    public Transform pivot;

    private OreData Data;
    private XRSocketInteractor socket;

    private bool isPowered = false;      // 소켓에 ‘전도성’ 아이템이 꽂혀 전원이 들어왔는가
    private float poweredElapsed = 0f;   // 전원 인가 후 경과 시간
    private float currentSpeed = 0f;     // 현재 실제 회전 속도 (deg/sec)
    private float targetSpeed = 0f;      // 목표 속도 (deg/sec)

    void Awake()
    {
        socket = GetComponentInChildren<XRSocketInteractor>();

        if (pivot == null && fanObject != null)
            pivot = fanObject.transform;
    }

    void OnEnable()
    {
        if (socket != null)
        {
            socket.selectEntered.AddListener(OnSelectEntered);
            socket.selectExited.AddListener(OnSelectExited);
        }
    }

    void OnDisable()
    {
        if (socket != null)
        {
            socket.selectEntered.RemoveListener(OnSelectEntered);
            socket.selectExited.RemoveListener(OnSelectExited);
        }
    }
    SFX sfx;
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        sfx = SoundManager.I.PlaySFX("fan", transform.position, null, 0.8f, 0.8f);
        var go = (args.interactableObject as Component)?.gameObject;
        if (go == null) return;

        var obj = go.GetComponent<ItemObject>();
        Data = obj != null ? obj.data : null;

        // 전도성 아이템이면 전원 On
        if (Data != null && Data.electroConduct)
        {
            isPowered = true;
            poweredElapsed = 0f;              // 워밍업 타이머 리셋
        }
        else
        {
            isPowered = false;
        }
    }


    private void OnSelectExited(SelectExitEventArgs args)
    {
        // 소켓에서 빠지면 전원 Off (감속)
        isPowered = false;
        Data = null;
        sfx?.Despawn();
    }

    void Update()
    {
        // 목표 속도 결정
        if (isPowered)
        {
            poweredElapsed += Time.deltaTime;
            // 워밍업 시간 전에는 idleSpeed 근처 유지, 이후에는 maxSpeed로
            float desired = (poweredElapsed >= warmupSeconds) ? maxSpeed : Mathf.Max(idleSpeed, currentSpeed);
            targetSpeed = desired;
        }
        else
        {
            targetSpeed = 0f; // 감속
        }

        // 현재 속도를 목표 속도로 부드럽게 이동 (가속/감속 분리)
        float rate = (targetSpeed > currentSpeed) ? accel : decel;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        // 회전 적용 (누적 회전)
        if (pivot != null && currentSpeed > 0f)
        {
            // 원하는 축으로 회전 (예: X축)
            pivot.Rotate(currentSpeed * Time.deltaTime, 0f, 0f, Space.Self);
        }

        if (!isPowered && currentSpeed <= 0.01f)
            currentSpeed = 0f;
    }
}