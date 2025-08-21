using System.Collections;
using Language.Lua;
using UnityEngine;
public class HClDropTest : MonoBehaviour
{
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
    int react = 0;
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ObjectInfo info))
        {
            count++;
            if (info.oreData.isReactingToChem == ChemicalType.None)
            {
                react = 0;
            }
            if (info.oreData.isReactingToChem == ChemicalType.Acid)
            {
                if (count % 30 == 0)
                {
                    Vector3 pos = other.ClosestPoint(kJHLiquidDrop.worldCenter);
                    ParticleManager.I.PlayParticle("Bubble", pos, Quaternion.identity, null);
                    SoundManager.I.PlaySFX("Bubble", pos, null, 0.8f, 0.8f);
                    react = 1;
                }
            }
            else if (info.oreData.isReactingToChem == ChemicalType.Water)
            {
                if (count % 30 == 0)
                {
                    Vector3 pos = other.ClosestPoint(kJHLiquidDrop.worldCenter);
                    ParticleManager.I.PlayParticle("Bubble", pos, Quaternion.identity, null);
                    SoundManager.I.PlaySFX("Bubble", pos, null, 0.8f, 0.8f);
                    react = 2;
                }
            }
            if (count > 250)
            {
                kJHLiquidDrop.UnInit();
                kJHLiquidDrop.Despawn();
                string str = GameManager.I.GetBoardText(info.oreData, 0);
                if (!str.Contains("염산 :"))
                {
                    if (react == 0)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 반응 없음");
                        else if (str.Contains("물 :"))
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 반응 없음" + "\n" + str);
                    }
                    else if (react == 1)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 기포 반응");
                        else if (str.Contains("물 :"))
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 기포 반응" + "\n" + str);
                    }
                    else if (react == 2)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 격렬한 반응");
                        else if (str.Contains("물 :"))
                            GameManager.I.EditBoardText(info.oreData, 0, "염산 : 격렬한 반응" + "\n" + str);
                    }
                }

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
            if (Time.time - time < 20f) continue;
            if (isTrigger) continue;
            kJHLiquidDrop.UnInit();
            kJHLiquidDrop.Despawn();
        }
    }

}
