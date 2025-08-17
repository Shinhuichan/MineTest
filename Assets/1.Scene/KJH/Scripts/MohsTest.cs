using UnityEngine;
public class MohsTest : MonoBehaviour
{
    public int number;
    public Vector3 prevDisplacement = Vector3.zero;
    public Vector3 displacement = Vector3.zero;
    public Vector3 velocity;
    void OnTriggerStay(Collider other)
    {
        if (other.transform.name != "Ore1" && other.transform.name != "Ore2") return;
        prevDisplacement = displacement;
        displacement = transform.position - other.transform.position;
        velocity = displacement - prevDisplacement;
        if(velocity.magnitude > 0.001f)
        Debug.Log($"OnTriggerStay ---> Mohs Ore {number} VS Entered Ore : {other.transform.name} (velocity : {velocity.magnitude})");
    }





}
