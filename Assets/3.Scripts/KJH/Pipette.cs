using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class Pipette : MonoBehaviour
{
    public bool isGrab = false;
    public bool isLeftButton = false;
    public bool isRightButton = false;
    public bool isInErlenmeyer = false;
    [HideInInspector] public ErlenmeyerTrigger erl;
    [ReadOnlyInspector][SerializeField] string xRControllerParentName;
    [SerializeField] private InputActionAsset inputAsset;
    public KJHLiquidDrop liquidPrefab;
    [SerializeField] private Vector2 fillRange;
    private Transform spawnPos;
    private Transform handle;
    Transform model;
    XRGrabInteractable xRGrab;
    Transform xRController;
    Liquid liquid;
    void Awake()
    {
        TryGetComponent(out xRGrab);
        model = transform.GetChild(0);
        spawnPos = model.Find("Pipette/SpawnPos");
        handle = model.Find("PipetteHandle");
        liquid = model.Find("PipetteFill").GetComponent<Liquid>();
    }
    void OnEnable()
    {
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").performed += LeftButton;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").canceled += LeftButtonUp;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").performed += RightButton;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").canceled += RightButtonUp;
        capacity = 1f;
    }
    void OnDisable()
    {
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").performed -= LeftButton;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").canceled -= LeftButtonUp;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").performed -= RightButton;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").canceled -= RightButtonUp;
    }
    private void LeftButton(InputAction.CallbackContext context)
    {
        isLeftButton = true;
    }
    private void LeftButtonUp(InputAction.CallbackContext context)
    {
        isLeftButton = false;
    }
    private void RightButton(InputAction.CallbackContext context)
    {
        isRightButton = true;
    }
    private void RightButtonUp(InputAction.CallbackContext context)
    {
        isRightButton = false;
    }
    public void GrabStart()
    {
        isGrab = true;
        xRController = xRGrab.firstInteractorSelecting.transform;
        xRControllerParentName = xRController.parent.name;
        StopCoroutine(nameof(GrabHolding));
        StartCoroutine(nameof(GrabHolding));
    }
    public void GrabEnd()
    {
        isGrab = false;
        xRControllerParentName = "";
        StopCoroutine(nameof(GrabHolding));
        isClick = false;
        isSpawn = false;
    }
    bool isClick;
    bool isSpawn = false;
    IEnumerator GrabHolding()
    {
        yield return null;
        float timeStep = 0.03f;
        YieldInstruction yi = new WaitForSeconds(timeStep);
        while (true)
        {
            #region Button Holding
            if ((xRControllerParentName == "Left Controller" && isLeftButton)
            || (xRControllerParentName == "Right Controller" && isRightButton))
            {
                if (!isSpawn)
                {
                    StartCoroutine("SpawnDrop");
                    isSpawn = true;
                }
                isClick = true;
                float speed = 1.15f;
                capacity -= speed * timeStep;
                capacity = Mathf.Clamp01(capacity);
                handle.localScale = Vector3.Lerp(handle.localScale, new Vector3(0.64f, 0.76f, 0.61f), 2.6f * timeStep);
            }
            else
            {
                isClick = false;
                isSpawn = false;
            }
            #endregion
            yield return yi;
        }
    }
    void Update()
    {
        if (capacity >= 1f) return;
        if (isClick) return;
        capacity += 0.88f * Time.deltaTime;
        capacity = Mathf.Clamp01(capacity);
        handle.localScale = Vector3.Lerp(handle.localScale, Vector3.one, Time.deltaTime);
        if (capacity < 1f && isInErlenmeyer)
        {
            fill += 0.8f * Time.deltaTime;
            fill = Mathf.Clamp01(fill);
            erl.fill -= 0.096f * Time.deltaTime;
            erl.Refresh();
            RefreshFill();
        }
    }
    ///////////////////////
    [ReadOnlyInspector][SerializeField] private float capacity = 1f;
    [ReadOnlyInspector] public float fill;
    void RefreshFill()
    {
        liquid.fillAmount = (1 - fill) * fillRange.x + fill * fillRange.y;
    }
    IEnumerator SpawnDrop()
    {
        while (true)
        {
            yield return null;
            if (!isClick) break;
            if (capacity < 0.5f) break;
        }
        float temp = (1f - capacity);
        temp = Mathf.Clamp(temp, 0.05f, 0.5f);
        if (temp > 0.15f && temp < 0.3f) temp = 0.3f;
        if (temp > 0.05f && temp < 0.15f) temp = 0.05f;
        float amount = 0.4f * (0.5f - temp) / 0.45f + 1.5f * (temp - 0.05f) / 0.45f;
        if (fill > 0)
        {
            if (fill < 0.1f)
            {
                fill = 0f;
                fill = Mathf.Clamp01(fill);
                amount = 0.25f;
                var pb = PoolManager.I.Spawn(liquidPrefab, spawnPos.position, Quaternion.identity, null, 40);
                pb.transform.localScale = amount * Vector3.one;
                RefreshFill();
            }
            else if (fill < 0.5f)
            {
                amount = Mathf.Min(amount, 0.8f);
                fill -= amount * 0.1f;
                fill = Mathf.Clamp01(fill);
                var pb = PoolManager.I.Spawn(liquidPrefab, spawnPos.position, Quaternion.identity, null, 40);
                pb.transform.localScale = amount * Vector3.one;
                if (fill < 0.69f) fill = 0;
                RefreshFill();
            }
            else
            {
                fill -= amount * 0.1f;
                fill = Mathf.Clamp01(fill);
                var pb = PoolManager.I.Spawn(liquidPrefab, spawnPos.position, Quaternion.identity, null, 40);
                pb.transform.localScale = amount * Vector3.one;
                if (fill < 0.69f) fill = 0;
                RefreshFill();
            }
        }
    }























    // bool isPress;
    // IEnumerator Pressing()
    // {
    //     yield return null;
    //     float timeStep = 0.02f;
    //     YieldInstruction yi = new WaitForSeconds(timeStep);
    //     while (true)
    //     {
    //         press -= 0.3f * timeStep;
    //         press = Mathf.Clamp01(press);
    //         if (press <= 0f) break;
    //         yield return yi;
    //     }
    //     isPress = false;
    // }









}
