using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightEnable_LSH : MonoBehaviour
{
    [Header("Fan Target")]
    public GameObject fanObject;

    [Header("Speed (deg/sec)")]
    [Tooltip("ó�� õõ�� ���� �ӵ�")]
    public float idleSpeed = 90f;
    [Tooltip("�ִ� ȸ�� �ӵ� (rotateSpeed ����)")]
    public float maxSpeed = 720f;

    [Header("Accel/Decel (deg/sec^2)")]
    [Tooltip("���ӵ� (�������� ���� �ö�)")]
    public float accel = 600f;
    [Tooltip("���ӵ� (�������� ���� ����)")]
    public float decel = 500f;

    [Header("Warm-up")]
    [Tooltip("������ ���ص� �� �ð� ������ ������(idleSpeed) ���ٰ� ���� �ִ�ӵ��� ����")]
    public float warmupSeconds = 1.0f;

    [Header("Optional Pivot (ȸ�� ����)")]
    public Transform pivot;

    private OreData Data;
    private XRSocketInteractor socket;

    private bool isPowered = false;      // ���Ͽ� ���������� �������� ���� ������ ���Դ°�
    private float poweredElapsed = 0f;   // ���� �ΰ� �� ��� �ð�
    private float currentSpeed = 0f;     // ���� ���� ȸ�� �ӵ� (deg/sec)
    private float targetSpeed = 0f;      // ��ǥ �ӵ� (deg/sec)

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

        // ������ �������̸� ���� On
        if (Data != null && Data.electroConduct)
        {
            isPowered = true;
            poweredElapsed = 0f;              // ���־� Ÿ�̸� ����
        }
        else
        {
            isPowered = false;
        }
    }


    private void OnSelectExited(SelectExitEventArgs args)
    {
        // ���Ͽ��� ������ ���� Off (����)
        isPowered = false;
        Data = null;
        sfx?.Despawn();
    }

    void Update()
    {
        // ��ǥ �ӵ� ����
        if (isPowered)
        {
            poweredElapsed += Time.deltaTime;
            // ���־� �ð� ������ idleSpeed ��ó ����, ���Ŀ��� maxSpeed��
            float desired = (poweredElapsed >= warmupSeconds) ? maxSpeed : Mathf.Max(idleSpeed, currentSpeed);
            targetSpeed = desired;
        }
        else
        {
            targetSpeed = 0f; // ����
        }

        // ���� �ӵ��� ��ǥ �ӵ��� �ε巴�� �̵� (����/���� �и�)
        float rate = (targetSpeed > currentSpeed) ? accel : decel;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        // ȸ�� ���� (���� ȸ��)
        if (pivot != null && currentSpeed > 0f)
        {
            // ���ϴ� ������ ȸ�� (��: X��)
            pivot.Rotate(currentSpeed * Time.deltaTime, 0f, 0f, Space.Self);
        }

        if (!isPowered && currentSpeed <= 0.01f)
            currentSpeed = 0f;
    }
}