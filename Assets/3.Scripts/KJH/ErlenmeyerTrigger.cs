using System.Collections;
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
            pipette.erl = this;
            if (type == Type.HCl)
            {
                pipette.liquidPrefab = allLiquids[0];
            }
            else
            {
                pipette.liquidPrefab = allLiquids[1];
            }
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
            pipette.erl = null;
            //Debug.Log("bbbb");
        }
    }
    public void Refresh()
    {
        liquid.fillAmount = (1 - fill) * fillRange.x + fill * fillRange.y;
    }



}
