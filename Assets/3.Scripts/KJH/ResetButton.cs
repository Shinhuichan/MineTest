using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class ResetButton : MonoBehaviour
{
    [SerializeField] Transform objectGroup;
    Transform[] initTrs;
    Vector3[] initPoses;
    Quaternion[] initRotations;
    Vector3[] initScales;
    //bool[] initIsActive;
    bool isPlaying = false;
    void OnEnable()
    {
        StartCoroutine(nameof(RecoredInitTransforms));
    }
    IEnumerator RecoredInitTransforms()
    {
        yield return null;
        yield return null;
        yield return null;
        initTrs = objectGroup.GetComponentsInChildren<Transform>(true);
        initPoses = new Vector3[initTrs.Length];
        initRotations = new Quaternion[initTrs.Length];
        initScales = new Vector3[initTrs.Length];
        //initIsActive = new bool[initTrs.Length];
        for (int i = 0; i < initTrs.Length; i++)
        {
            initPoses[i] = initTrs[i].position;
            initRotations[i] = initTrs[i].rotation;
            initScales[i] = initTrs[i].localScale;
            //initIsActive[i] = initTrs[i].gameObject.activeSelf;
            //Debug.Log(initTrs[i].name);
            yield return null;
        }
    }
    public void ButtonEnter()
    {
        if (!isRunning)
        {
            StopCoroutine(nameof(ButtonHolding));
            StartCoroutine(nameof(ButtonHolding));
        }
    }
    public void ButtonExit()
    {
        
    }
    public void QuickReset()
    {
        for (int i = 0; i < initTrs.Length; i++)
        {
            initTrs[i].position = initPoses[i];
            initTrs[i].rotation = initRotations[i];
            initTrs[i].localScale = initScales[i];
            if (initTrs[i].TryGetComponent(out ErlenmeyerTrigger erlenmeyer))
            {
                erlenmeyer.fill = 1f;
                erlenmeyer.Refresh();
            }
        }
    }
    bool isRunning;
    IEnumerator ButtonHolding()
    {
        isRunning = true;
        yield return new WaitForSeconds(1.4f);
        GlobalUI.I.FadeOut(0.6f);
        for (int i = 0; i < initTrs.Length; i++)
        {
            Rigidbody rb = initTrs[i].GetComponent<Rigidbody>();
            XRGrabInteractable xRGrab = initTrs[i].GetComponent<XRGrabInteractable>();
            CollisionDetectionMode mode1 = CollisionDetectionMode.Discrete;
            RigidbodyInterpolation mode2 = RigidbodyInterpolation.None;
            bool grab = false;
            if (xRGrab != null)
            {
                grab = xRGrab.enabled;
                xRGrab.enabled = false;
                yield return null;
            }
            if (rb != null)
            {
                mode1 = rb.collisionDetectionMode;
                mode2 = rb.interpolation;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.interpolation = RigidbodyInterpolation.None;
                yield return null;
            }
            initTrs[i].position = initPoses[i];
            initTrs[i].rotation = initRotations[i];
            initTrs[i].localScale = initScales[i];
            if (initTrs[i].TryGetComponent(out ErlenmeyerTrigger erlenmeyer))
            {
                erlenmeyer.fill = 1f;
                erlenmeyer.Refresh();
            }
            if (rb != null)
            {
                yield return null;
                rb.collisionDetectionMode = mode1;
                rb.interpolation = mode2;
            }
            if (xRGrab != null)
            {
                yield return null;
                xRGrab.enabled = grab;
            }
        }
        yield return new WaitForSeconds(0.2f);
        GlobalUI.I.FadeIn(0.3f);
        isRunning = false;
    }
}
