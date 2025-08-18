using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class Pipette : MonoBehaviour
{
    public bool isGrab = false;
    public bool isLeftButton = false;
    public bool isRightButton = false;
    [ReadOnlyInspector][SerializeField] string xRControllerParentName;
    [SerializeField] private InputActionAsset inputAsset;
    [SerializeField] private KJHLiquid liquidPrefab;
    private Transform spawnPos;
    private Transform handle;
    Transform model;
    XRGrabInteractable xRGrab;
    Transform xRController;
    void Awake()
    {
        TryGetComponent(out xRGrab);
        model = transform.GetChild(0);
        spawnPos = model.Find("Pipette/SpawnPos");
        handle = model.Find("PipetteHandle");
    }
    void OnEnable()
    {
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").performed += LeftButton;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").canceled += LeftButtonUp;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").performed += RightButton;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").canceled += RightButtonUp;
        airAmount = 1f;
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
    int coolTime;
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
                isHold = true;
                float speed = 1.15f;
                airAmount -= speed * timeStep;
                airAmount = Mathf.Clamp01(airAmount);
                handle.localScale = Vector3.Lerp(handle.localScale, new Vector3(0.64f, 0.76f, 0.61f), 2.6f * timeStep);
                coolTime++;
                if (coolTime > 100) coolTime = 0;
                if (coolTime == 5)
                {
                    PoolManager.I.Spawn(liquidPrefab, spawnPos.position, Quaternion.identity, null, 40);
                }
                
            }
            else
            {
                isHold = false;
            }
            #endregion
            yield return yi;
        }
    }
    void Update()
    {
        if (airAmount >= 1f) return;
        if (isHold) return;
        airAmount += 0.88f * Time.deltaTime;
        airAmount = Mathf.Clamp01(airAmount);
        handle.localScale = Vector3.Lerp(handle.localScale, Vector3.one, Time.deltaTime);

    }
    ///////////////////////
    [ReadOnlyInspector][SerializeField] float airAmount = 1f;

























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
