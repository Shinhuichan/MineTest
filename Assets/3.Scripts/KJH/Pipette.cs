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
    [ReadOnlyInspector][SerializeField] string xRControllerParentName;
    [SerializeField] private InputActionAsset inputAsset;
    [SerializeField] private KJHLiquidDrop liquidPrefab;
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
        isHold = false;
    }
    bool isHold;
    bool isSpawn = false;
    IEnumerator GrabHolding()
    {
        yield return null;
        float timeStep = 0.03f;
        YieldInstruction yi = new WaitForSeconds(timeStep);
        isSpawn = false;
        while (true)
        {
            #region Button Holding
            if ((xRControllerParentName == "Left Controller" && isLeftButton)
            || (xRControllerParentName == "Right Controller" && isRightButton))
            {
                isHold = true;
                float speed = 1.15f;
                capacity -= speed * timeStep;
                capacity = Mathf.Clamp01(capacity);
                handle.localScale = Vector3.Lerp(handle.localScale, new Vector3(0.64f, 0.76f, 0.61f), 2.6f * timeStep);
                if (!isSpawn && fill >= 0.5f)
                {
                    isSpawn = true;
                    if (fill > 0.48f / fillRange.y)
                        fill = (0.471f / fillRange.y);
                    else
                        fill = 0f;
                    fill = Mathf.Clamp01(fill);
                    PoolManager.I.Spawn(liquidPrefab, spawnPos.position, Quaternion.identity, null, 40);
                    RefreshFill();
                }
            }
            else
            {
                isHold = false;
                isSpawn = false;
            }
            #endregion
            yield return yi;
        }
    }
    void Update()
    {
        if (capacity >= 1f) return;
        if (isHold) return;
        capacity += 0.88f * Time.deltaTime;
        capacity = Mathf.Clamp01(capacity);
        handle.localScale = Vector3.Lerp(handle.localScale, Vector3.one, Time.deltaTime);
        if (capacity < 1f && isInErlenmeyer)
        {
            fill += 0.8f * Time.deltaTime;
            fill = Mathf.Clamp01(fill);
            RefreshFill();
        }
    }
    ///////////////////////
    [ReadOnlyInspector][SerializeField] private float capacity = 1f;
    [ReadOnlyInspector] public float fill;
    void RefreshFill()
    {
        liquid.fillAmount = (1-fill) * fillRange.x  + fill * fillRange.y;
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
