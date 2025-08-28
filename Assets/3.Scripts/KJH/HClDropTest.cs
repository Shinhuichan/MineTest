using System.Collections;
using UnityEngine;
public class HClDropTest : MonoBehaviour
{
    public float amount = 1f;
    int count = 0;
    KJHLiquidDrop kJHLiquidDrop;
    void Awake()
    {
        transform.parent.TryGetComponent(out kJHLiquidDrop);
    }
    void OnEnable()
    {
        count = 0;
        StartCoroutine(nameof(AutoRemove));
    }
    bool isReact;
    int reactType = 0;
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ObjectInfo info))
        {
            count++;
            if (count > 5 && !isReact && amount >= 0)
            {
                amount = transform.lossyScale.x;
                //Debug.Log($"{info.chemistryCount} + {amount} --> {info.chemistryCount + amount}");
                isReact = true;
                info.chemistryCount += amount;
            }
            //
            if (info.oreData.isReactingToChem == ChemicalType.None)
            {
                if (count % 30 == 0)
                {
                    reactType = 0;
                }
            }
            else if (info.oreData.isReactingToChem.HasFlag(ChemicalType.Acid) && !info.oreData.isReactingToChem.HasFlag(ChemicalType.Water))
            {
                if (count % 30 == 0)
                {
                    Vector3 pos = other.ClosestPoint(kJHLiquidDrop.worldCenter);
                    ParticleManager.I.PlayParticle("Bubble", pos, Quaternion.identity, null);
                    SoundManager.I.PlaySFX("Bubble", pos, null, 0.8f, 0.8f);
                    reactType = 1;
                }
            }
            else if (info.oreData.isReactingToChem.HasFlag(ChemicalType.Acid) && info.oreData.isReactingToChem.HasFlag(ChemicalType.Water))
            {
                if (count % 30 == 0)
                {
                    if (amount < 0.7f)
                    {
                        Vector3 pos = other.ClosestPoint(kJHLiquidDrop.worldCenter);
                        var pb = ParticleManager.I.PlayParticle("Smoke", pos, Quaternion.identity, null);
                        pb.transform.localScale = 0.3f * Vector3.one;
                        SoundManager.I.PlaySFX("Smoke", pos, null, 0.8f, 0.7f);
                        GlobalUI.I.StartCoroutine("SmallFire", pos);
                    }
                    else if (amount >= 0.7f)
                    {
                        Vector3 pos = other.ClosestPoint(kJHLiquidDrop.worldCenter);
                        var pb = ParticleManager.I.PlayParticle("Smoke", pos, Quaternion.identity, null);
                        pb.transform.localScale = 1.0f * Vector3.one;
                        SoundManager.I.PlaySFX("Smoke", pos, null, 0.8f, 1.3f);
                        GlobalUI.I.StartCoroutine("BigFire", pos);
                        int find = GameManager.I.accidents.FindIndex(x => x.accidentName == "Explosion");
                        if (find == -1)
                        {
                            LaboratoryAccident la = new LaboratoryAccident();
                            la.accidentName = "Explosion";
                            la.accidentWeight = 10;
                            GameManager.I.accidents.Add(la);
                            GlobalUI.I.StartCoroutine("Explosion", pos);
                        }
                    }
                    reactType = 2;
                }
            }
            if (count > 250 - (amount * 100) && isReact)
            {
                //Debug.Log($"{info.chemistryCount} - {amount} --> {info.chemistryCount - amount}");
                isReact = false;
                info.chemistryCount -= amount;
                string str = GameManager.I.GetBoardText(info.oreData, 0);
                if (!str.Contains("염산 :"))
                {
                    if (reactType == 0)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 반응 없음");
                        else if (str.Contains("물 :"))
                            GameManager.I.Clear(info.oreData, 0, "염산 : 반응 없음" + "\n" + str);
                        if (!GlobalUI.I.isShowReactHClNoReact)
                        {
                            GlobalUI.I.isShowReactHClNoReact = true;
                            GlobalUI.I.Narration("이 광물은 염산과 반응하지 않는 것 같다.", 3.7f);
                        }
                    }
                    else if (reactType == 1)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 기포 반응");
                        else if (str.Contains("물 :"))
                            GameManager.I.Clear(info.oreData, 0, "염산 : 기포 반응" + "\n" + str);
                    }
                    else if (reactType == 2)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 격렬한 반응");
                        else if (str.Contains("물 :"))
                            GameManager.I.Clear(info.oreData, 0, "염산 : 격렬한 반응" + "\n" + str);
                    }
                }

                kJHLiquidDrop.UnInit();
                kJHLiquidDrop.Despawn();
            }
        }
    }
    bool isTrigger;
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ObjectInfo info))
        {
            isTrigger = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ObjectInfo info))
        {
            isTrigger = false;
        }
    }
    IEnumerator AutoRemove()
    {
        float time = Time.time;
        YieldInstruction yi = new WaitForSeconds(1f);
        while (true)
        {
            yield return yi;
            if (Time.time - time < 12f) continue;
            if (isTrigger) continue;
            kJHLiquidDrop.UnInit();
            kJHLiquidDrop.Despawn();
        }
    }

}
