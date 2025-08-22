using System.Collections;
using Language.Lua;
using UnityEngine;
public class H2ODropTest : MonoBehaviour
{
    public float amount = 0f;
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
        amount = 0f;
    }
    int react = 0;
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ObjectInfo info))
        {
            count++;
            if (amount == 0)
            {
                amount = transform.localScale.x;
            }
            if (info.oreData.isReactingToChem == ChemicalType.None)
            {
                react = 0;
            }
            else if (info.oreData.isReactingToChem == ChemicalType.Acid)
            {
                react = 1;
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
            if (count > 250 - (amount * 100))
            {
                kJHLiquidDrop.UnInit();
                kJHLiquidDrop.Despawn();
                string str = GameManager.I.GetBoardText(info.oreData, 0);
                if (!str.Contains("물 :"))
                {
                    if (react == 0)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "물 : 반응 없음");
                        else if (str.Contains("염산 :"))
                            GameManager.I.Clear(info.oreData, 0, str + "\n" + "물 : 반응 없음");
                    }
                    else if (react == 1)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "물 : 반응 없음");
                        else if (str.Contains("염산 :"))
                            GameManager.I.Clear(info.oreData, 0, str + "\n" + "물 : 반응 없음");
                    }
                    else if (react == 2)
                    {
                        if (str == "")
                            GameManager.I.EditBoardText(info.oreData, 0, "물 : 격렬한 반응");
                        else if (str.Contains("염산 :"))
                            GameManager.I.Clear(info.oreData, 0, str + "\n" + "물 : 격렬한 반응");
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
