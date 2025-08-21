using UnityEngine;
public class ErlenmeyerTrigger : MonoBehaviour
{
    public enum Type
    {
        HCl,
        H2O,
    }
    [SerializeField] KJHLiquidDrop[] allLiquids;
    public Type type;
    [SerializeField] private Vector2 fillRange;
    public float fill = 1f;
    Liquid liquid;
    void Awake()
    {
        liquid = transform.parent.GetComponentInChildren<Liquid>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null) return;
        if (other.transform.parent.parent == null) return;
        if (other.transform.parent.parent.TryGetComponent(out Pipette pipette))
        {
            pipette.isInErlenmeyer = true;
            //Debug.Log("aaaa");
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.transform.parent == null) return;
        if (other.transform.parent.parent == null) return;
        if (other.transform.parent.parent.TryGetComponent(out Pipette pipette))
        {
            pipette.isInErlenmeyer = false;
            //Debug.Log("bbbb");
        }
    }

}
