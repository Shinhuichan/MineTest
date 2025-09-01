using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class MohsOre : MonoBehaviour
{
    public int number;
    [SerializeField] LayerMask myLayerMask;
    MohsTest mt;
    [ReadOnlyInspector][SerializeField] int count = 1;
    XRGrabInteractable xRGrab;
    MeshCollider mc;
    LineRenderer myLr;
    Vector3 contactPoint;
    Transform targetFix = null;
    LineRenderer targetLr;
    LayerMask targetlayerMask;
    void Awake()
    {
        TryGetComponent(out mc);
        TryGetComponent(out xRGrab);
        myLr = GetComponentInParent<LineRenderer>();
        mt = GetComponentInParent<MohsTest>();
    }
    void OnEnable()
    {
        Material mat = myLr.material;
        myLr.material = Instantiate(mat);
        myLr.material.color = myLr.startColor;
        myLr.material.SetColor("_Color", myLr.startColor);
        startTime = Time.time;
    }
    Vector3 prevDisplacement;
    Vector3 currDisplacement;
    Vector3 velocity;
    Vector3 firstLocalPos;
    bool isGrab;
    public void GrabStart()
    {
        isGrab = true;
        SoundManager.I.PlaySFX("DropGlass1", transform.position, null, 0.8f, 0.3f);
    }
    public void GrabEnd()
    {
        isGrab = false;
    }
    bool isCoolTime;
    float startTime;
    IEnumerator CoolTime()
    {
        yield return new WaitForSeconds(1.4f);
        isCoolTime = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        if (Time.time - startTime > 1.4f)
            if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
                if (!isCoolTime && !isGrab)
                {
                    isCoolTime = true;
                    SoundManager.I.PlaySFX("DropGlass2", transform.position, null, 0.8f, 0.6f);
                    StartCoroutine(nameof(CoolTime));
                }
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.TryGetComponent(out ObjectInfo info))
        {
            if (targetFix == null)
            {
                targetFix = collision.collider.transform;
                targetLr = targetFix.GetComponent<LineRenderer>();
                targetlayerMask = 1 << collision.collider.gameObject.layer;
                StartCoroutine(nameof(ResetTarget));
            }
            else if (targetFix == collision.collider.transform)
            {
                contactPoint = collision.contacts[0].point;
                t = 0;
                prevDisplacement = currDisplacement;
                currDisplacement = transform.position - targetFix.position;
                if (prevDisplacement != Vector3.zero)
                {
                    velocity = currDisplacement - prevDisplacement;
                    if (velocity.magnitude > 0.005f && velocity.magnitude < 0.1f)
                    {
                        count++;
                        if (count % 30 == 0)
                        {
                            if (number != info.oreData.hardness)
                            {
                                SoundManager.I.PlaySFX("Scratch", contactPoint, null, 0.7f, 0.5f);
                                ParticleManager.I.PlayParticle("DustSmall", contactPoint, Quaternion.identity, null);
                            }
                            else
                            {
                                SoundManager.I.PlaySFX("Scratch", contactPoint, null, 0.7f, 0.2f);
                            }
                        }
                        if (count % 6 == 0)
                        {
                            //DebugExtension.DebugWireSphere(contactPoint, 0.012f, 0.5f, true);
                            if (number < info.oreData.hardness)
                            {
                                if (firstLocalPos == Vector3.zero)
                                {
                                    firstLocalPos = transform.InverseTransformPoint(contactPoint);
                                    myLr.positionCount = 1;
                                    myLr.SetPosition(0, firstLocalPos);
                                    if (!mt.Has(myLr))
                                        mt.AddScratch(myLr);
                                    else
                                        mt.ReCoroutine(myLr);
                                }
                                else
                                {
                                    if (coroutine == null)
                                        coroutine = StartCoroutine(AddScratch(myLr, targetFix, myLayerMask));

                                    if (xRGrab.firstInteractorSelecting != null)
                                    {
                                        //Debug.Log(xRGrab.firstInteractorSelecting.transform.parent);
                                        ActionBasedController abc = xRGrab.firstInteractorSelecting.transform.parent.GetComponent<ActionBasedController>();
                                        abc.SendHapticImpulse(0.5f, 0.2f);
                                    }

                                }
                                if (count > 35)
                                {
                                    string str = GameManager.I.GetBoardText(info.oreData, 1);
                                    if (str == info.oreData.hardness.ToString())
                                    {
                                        //Debug.Log("이미 완료한 실험");
                                        return;
                                    }
                                    string str2 = "";
                                    if (str == "")
                                        str = "?";
                                    else
                                    {
                                        str2 = str.Split("~")[0];
                                        str = str.Split("~")[1];
                                    }
                                    float temp = -999;
                                    float.TryParse(str2, out temp);
                                    temp = Mathf.Max(temp, number);
                                    GameManager.I.EditBoardText(info.oreData, 1, $"{temp}~{str}");
                                    float temp2 = 999;
                                    if (float.TryParse(str, out temp2))
                                    {
                                        if (info.oreData.hardness % 1 == 0)
                                        {
                                            if (Mathf.Abs(temp - temp2) == 2)
                                            {
                                                GameManager.I.Clear(info.oreData, 1, info.oreData.hardness.ToString());
                                                GlobalUI.I.Narration($"경도가 {temp}보다 크고 {str}보다 작으니까.\n이 광물의 경도는 {info.oreData.hardness}쯤 될것같다.", 3.7f);
                                            }
                                            else
                                            {
                                                if (!GlobalUI.I.isShowMohsUpper)
                                                {
                                                    GlobalUI.I.isShowMohsUpper = true;
                                                    GlobalUI.I.Narration($"경도가 {number}인 광물이 긁히는 걸 보면, 이 광물은\n경도가 {number}보다 클 것이다. 범위를 더 좁혀보자..", 4.2f);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Mathf.Abs(temp - temp2) == 1)
                                            {
                                                GameManager.I.Clear(info.oreData, 1, info.oreData.hardness.ToString());
                                                GlobalUI.I.Narration($"경도가 {temp}보다 크고 {str}보다 작으니까.\n이 광물의 경도는 {info.oreData.hardness}쯤 될것같다.", 3.7f);
                                            }
                                            else
                                            {
                                                if (!GlobalUI.I.isShowMohsUpper)
                                                {
                                                    GlobalUI.I.isShowMohsUpper = true;
                                                    GlobalUI.I.Narration($"경도가 {number}인 광물이 긁히는 걸 보면, 이 광물은\n경도가 {number}보다 클 것이다. 범위를 더 좁혀보자..", 4.2f);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!GlobalUI.I.isShowMohsUpper)
                                        {
                                            GlobalUI.I.isShowMohsUpper = true;
                                            GlobalUI.I.Narration($"경도가 {number}인 광물이 긁히는 걸 보면, 이 광물은\n경도가 {number}보다 클 것이다. 범위를 더 좁혀보자..", 4.2f);
                                        }
                                    }
                                }
                            }
                            else if (number > info.oreData.hardness)
                            {
                                if (firstLocalPos == Vector3.zero)
                                {
                                    firstLocalPos = info.transform.InverseTransformPoint(contactPoint);
                                    targetLr.positionCount = 1;
                                    targetLr.SetPosition(0, firstLocalPos);
                                    if (!mt.Has(targetLr))
                                        mt.AddScratch(targetLr);
                                    else
                                        mt.ReCoroutine(targetLr);
                                }
                                else
                                {
                                    if (coroutine == null)
                                        coroutine = StartCoroutine(AddScratch(targetLr, transform, targetlayerMask));
                                    XRGrabInteractable _xRGrab = info.GetComponent<XRGrabInteractable>();
                                    if (_xRGrab != null)
                                    {
                                        if (_xRGrab.firstInteractorSelecting != null)
                                        {
                                            ActionBasedController abc = _xRGrab.firstInteractorSelecting.transform.parent.GetComponent<ActionBasedController>();
                                            abc.SendHapticImpulse(0.5f, 0.2f);
                                        }
                                    }
                                }
                                if (count > 35)
                                {
                                    string str = GameManager.I.GetBoardText(info.oreData, 1);
                                    if (str == info.oreData.hardness.ToString())
                                    {
                                        //Debug.Log("이미 완료한 실험");
                                        return;
                                    }
                                    string str2 = "";
                                    if (str == "")
                                        str = "?";
                                    else
                                    {
                                        str2 = str.Split("~")[1];
                                        str = str.Split("~")[0];
                                    }
                                    float temp = 999;
                                    float.TryParse(str2, out temp);
                                    if (temp == 0) temp = 999;
                                    temp = Mathf.Min(temp, number);
                                    GameManager.I.EditBoardText(info.oreData, 1, $"{str}~{temp}");
                                    float temp2 = 999;
                                    if (float.TryParse(str, out temp2))
                                    {
                                        if (info.oreData.hardness % 1 == 0)
                                        {
                                            if (Mathf.Abs(temp - temp2) == 2)
                                            {
                                                GlobalUI.I.Narration($"경도가 {str}보다 크고 {temp}보다 작으니까.\n이 광물의 경도는 {info.oreData.hardness}쯤 될것같다.", 3.7f);
                                                GameManager.I.Clear(info.oreData, 1, info.oreData.hardness.ToString());
                                            }
                                            else
                                            {
                                                if (!GlobalUI.I.isShowMohsLower)
                                                {
                                                    GlobalUI.I.isShowMohsLower = true;
                                                    GlobalUI.I.Narration($"경도가 {number}인 광물에 긁히는 걸 보면, 이 광물은\n경도가 {number}보다 작을 것이다. 범위를 더 좁혀보자..", 4.2f);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Mathf.Abs(temp - temp2) == 1)
                                            {
                                                GlobalUI.I.Narration($"경도가 {str}보다 크고 {temp}보다 작으니까.\n이 광물의 경도는 {info.oreData.hardness}쯤 될것같다.", 3.7f);

                                                GameManager.I.Clear(info.oreData, 1, info.oreData.hardness.ToString());
                                            }
                                            else
                                            {
                                                if (!GlobalUI.I.isShowMohsLower)
                                                {
                                                    GlobalUI.I.isShowMohsLower = true;
                                                    GlobalUI.I.Narration($"경도가 {number}인 광물에 긁히는 걸 보면, 이 광물은\n경도가 {number}보다 작을 것이다. 범위를 더 좁혀보자..", 4.2f);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!GlobalUI.I.isShowMohsLower)
                                        {
                                            GlobalUI.I.isShowMohsLower = true;
                                            GlobalUI.I.Narration($"경도가 {number}인 광물에 긁히는 걸 보면, 이 광물은\n경도가 {number}보다 작을 것이다. 범위를 더 좁혀보자..", 4.2f);
                                        }
                                    }
                                }
                            }
                            else if (number == info.oreData.hardness)
                            {
                                if (count > 35)
                                {
                                    string str = GameManager.I.GetBoardText(info.oreData, 1);
                                    if (str == info.oreData.hardness.ToString())
                                    {
                                        //Debug.Log("이미 완료한 실험");
                                        return;
                                    }
                                    else
                                    {
                                        if (!GlobalUI.I.isShowMohsSameHardness)
                                        {
                                            GlobalUI.I.Narration("경도가 거의 같아서 서로 잘 안긁히는 것 같다.\n다른 광물로 해보자.", 6f);
                                            GlobalUI.I.isShowMohsSameHardness = true;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
    }
    float t = 0;
    IEnumerator ResetTarget()
    {
        t = 0;
        prevDisplacement = Vector3.zero;
        currDisplacement = Vector3.zero;
        velocity = Vector3.zero;
        firstLocalPos = Vector3.zero;
        count = 1;
        while (true)
        {
            t += Time.deltaTime;
            //Debug.Log($"{targetFix} , {t}");
            yield return null;
            if (t > 1.8f)
            {
                targetFix = null;
                t = 0;
                prevDisplacement = Vector3.zero;
                currDisplacement = Vector3.zero;
                velocity = Vector3.zero;
                firstLocalPos = Vector3.zero;
                count = 1;
                break;
            }
        }
    }
    Coroutine coroutine;
    IEnumerator AddScratch(LineRenderer lr, Transform secondTr, LayerMask layerMask)
    {
        if (!mt.Has(lr))
            mt.AddScratch(lr);
        else
            mt.ReCoroutine(lr);
        int n = lr.positionCount;
        lr.positionCount = n + 1;
        Collider collider = lr.GetComponentInChildren<Collider>();
        contactPoint = collider.ClosestPoint(contactPoint);
        lr.SetPosition(n, lr.transform.InverseTransformPoint(contactPoint));
        yield return null;
        coroutine = null;


        // if (controllers != null)
        // {
        //     foreach (var ctrl in controllers)
        //     {
        //         ctrl.SendHapticImpulse(0.5f, 0.2f);
        //     }
        // } 
    }








}
