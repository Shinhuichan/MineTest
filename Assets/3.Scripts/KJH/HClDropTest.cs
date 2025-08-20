using System.Collections;
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
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ObjectInfo info))
        {
            count++;
            if (info.oreData.isReactingToChem == ChemicalType.Acid)
            {
                if (count % 30 == 0)
                {
                    Vector3 pos = other.ClosestPoint(kJHLiquidDrop.worldCenter);
                    ParticleManager.I.PlayParticle("Bubble", pos, Quaternion.identity, null);
                    SoundManager.I.PlaySFX("Bubble", pos, null, 0.8f);
                }
            }
            if (count > 250)
            {
                kJHLiquidDrop.UnInit();
                kJHLiquidDrop.Despawn();
                GameManager.I.ClearExperiment(info.oreData, 0);
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
