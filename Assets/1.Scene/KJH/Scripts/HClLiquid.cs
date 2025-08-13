using UnityEngine;
public class HClLiquid : MonoBehaviour
{
    KJHLiquid kJHLiquid;
    void Awake()
    {
        TryGetComponent(out kJHLiquid);
    }
    void OnEnable()
    {
        callCount = 0;
    }
    int callCount;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Ore1")
        {
            callCount++;
            if (callCount % 20 == 0)
            {
                ParticleManager.I.PlayParticle("DustSmall", collision.contacts[0].point, Quaternion.identity, null);
                SoundManager.I.PlaySFX("LiquidDrop", collision.contacts[0].point, null, 0.8f);
            }
            else if (callCount > 100)
            {
                kJHLiquid.UnInit();
                kJHLiquid.Despawn();
            }
        }
    }
}
