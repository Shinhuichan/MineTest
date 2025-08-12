using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Unity.Entities;
public class Pipette : MonoBehaviour
{
    public bool isGrab = false;
    [ReadOnlyInspector][SerializeField] string xRControllerParentName;
    public bool isLeftButton = false;
    public bool isRightButton = false;
    [SerializeField] private InputActionAsset inputAsset;
    [SerializeField] private KJHLiquid liquidPrefab;
    [SerializeField] private Transform spawnPos;
    Transform model;
    XRGrabInteractable xRGrab;
    Transform xRController;
    void Awake()
    {
        TryGetComponent(out xRGrab);
        model = transform.GetChild(0);
    }
    void OnEnable()
    {
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").performed += LeftButton;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").canceled += LeftButtonUp;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").performed += RightButton;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").canceled += RightButtonUp;
    }
    void OnDisable()
    {
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").performed -= LeftButton;
        inputAsset.FindActionMap("XRI LeftHand Interaction").FindAction("Activate").canceled -= LeftButtonUp;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").performed -= RightButton;
        inputAsset.FindActionMap("XRI RightHand Interaction").FindAction("Activate").canceled -= RightButtonUp;

    }
    public void GrabStart()
    {
        isGrab = true;
        xRController = xRGrab.firstInteractorSelecting.transform;
        xRControllerParentName = xRController.parent.name;
        StopCoroutine(nameof(GrabTransform));
        StartCoroutine(nameof(GrabTransform));
    }
    public void GrabEnd()
    {
        isGrab = false;
        xRControllerParentName = "";
        StopCoroutine(nameof(GrabTransform));
    }
    IEnumerator GrabTransform()
    {
        yield return null;
        float maxTime = 200f;
        float time = Time.time;
        YieldInstruction yi = new WaitForSeconds(0.02f);
        while (Time.time - time < maxTime)
        {
            if (xRControllerParentName == "Left Controller" && isLeftButton)
            {
                PoolManager.I.Spawn(liquidPrefab, spawnPos.position, Quaternion.identity, null, 40);
                yield return new WaitForSeconds(1f);
            }
            if (xRControllerParentName == "Right Controller" && isRightButton)
            {
                PoolManager.I.Spawn(liquidPrefab, spawnPos.position, Quaternion.identity, null, 40);
                yield return new WaitForSeconds(1f);
            }

            yield return yi;
        }
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






}
