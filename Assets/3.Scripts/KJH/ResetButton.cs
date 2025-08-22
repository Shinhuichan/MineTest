using System.Collections;
using UnityEngine;
public class ResetButton : MonoBehaviour
{
    [SerializeField] Transform objectGroup;
    Transform[] initTrs;
    Vector3[] initPoses;
    Quaternion[] initRotations;
    Vector3[] initScales;
    bool[] initIsActive;
    bool isPlaying = false;
    void OnEnable()
    {
        initTrs = objectGroup.GetComponentsInChildren<Transform>();
        StartCoroutine(nameof(RecoredInitTransforms));
    }
    IEnumerator RecoredInitTransforms()
    {
        initPoses = new Vector3[initTrs.Length];
        initRotations = new Quaternion[initTrs.Length];
        initScales = new Vector3[initTrs.Length];
        initIsActive = new bool[initTrs.Length];
        for (int i = 0; i < initTrs.Length; i++)
        {
            initPoses[i] = initTrs[i].position;
            initRotations[i] = initTrs[i].rotation;
            initScales[i] = initTrs[i].localScale;
            initIsActive[i] = initTrs[i].gameObject.activeSelf;
            yield return null;
        }
    }
    public void ButtonEnter()
    {
        StopCoroutine(nameof(ButtonHolding));
        StartCoroutine(nameof(ButtonHolding));
    }
    public void ButtonExit()
    {
        StopCoroutine(nameof(ButtonHolding));
    }
    IEnumerator ButtonHolding()
    {
        yield return new WaitForSeconds(1.9f);
        GlobalUI.I.FadeOut(0.6f);
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < initTrs.Length; i++)
        {
            initTrs[i].position = initPoses[i];
            initTrs[i].rotation = initRotations[i];
            initTrs[i].localScale = initScales[i];
            initTrs[i].gameObject.SetActive(initIsActive[i]);
        }
        yield return new WaitForSeconds(0.3f);
        GlobalUI.I.FadeIn(0.6f);
    }
}
