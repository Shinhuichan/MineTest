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
                    ParticleManager.I.PlayParticle("DustSmall", pos, Quaternion.identity, null);
                    SoundManager.I.PlaySFX("LiquidDrop", pos, null, 0.8f);
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

}
