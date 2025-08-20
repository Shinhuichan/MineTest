using UnityEngine;
public class MohsTestTrigger : MonoBehaviour
{
    public int number;
    Vector3 prevDisplacement = Vector3.zero;
    Vector3 displacement = Vector3.zero;
    Vector3 velocity;
    MeshCollider mc;
    LineRenderer lr;
    LineRenderer targetLr;
    int count = 0;
    MohsTest mt;
    void Awake()
    {
        TryGetComponent(out mc);
        lr = GetComponentInParent<LineRenderer>();
        mt = GetComponentInParent<MohsTest>();
    }
    void OnEnable()
    {
        MeshFilter meshFilter = transform.parent.GetComponent<MeshFilter>();
        mc.sharedMesh = null;
        mc.sharedMesh = meshFilter.sharedMesh;
        Vector3[] temp = new Vector3[100];
        System.Array.Fill(temp, Vector3.zero);
        lr.SetPositions(temp);
    }
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ObjectInfo info))
        {
            prevDisplacement = displacement;
            displacement = transform.position - other.transform.position;
            velocity = displacement - prevDisplacement;
            if (velocity.magnitude > 0.006f)
            {
                if (number < info.oreData.hardness)
                {
                    Collider myColl = transform.parent.GetComponent<Collider>();
                    Vector3 closet = myColl.ClosestPoint(other.transform.position);
                    DebugExtension.DebugWireSphere(closet, Color.blue, 0.01f, 0.02f, true);
                    count++;
                    if (count % 24 == 0)
                    {
                        AddScratch(lr, transform.parent);/*, transform.InverseTransformPoint(closet)*/
                        Collider collider = transform.parent.GetComponent<Collider>();
                        Vector3 pos = collider.ClosestPoint(other.transform.position);
                        if (Random.value < 0.5f)
                            ParticleManager.I.PlayParticle("DustSmall", pos, Quaternion.identity, null);
                        SoundManager.I.PlaySFX("Scratch", pos, null, 0.8f);
                    }
                    if (count > 20000) count = 0;
                }
                else if (number > info.oreData.hardness)
                {
                    Collider targetColl = other.transform.GetComponent<Collider>();
                    targetLr = other.transform.GetComponent<LineRenderer>();
                    Vector3 closet = targetColl.ClosestPoint(transform.position);
                    count++;
                    if (count % 24 == 0)
                    {
                        AddScratch(targetLr, other.transform);/*, .transform.InverseTransformPoint(closet)*/
                        Vector3 pos = other.ClosestPoint(transform.position);
                        if (Random.value < 0.5f)
                            ParticleManager.I.PlayParticle("DustSmall", pos, Quaternion.identity, null);
                        SoundManager.I.PlaySFX("Scratch", pos, null, 0.8f);
                    }
                    if (count > 20000) count = 0;
                }
                else if (number == info.oreData.hardness)
                {


                }
            }
        }
    }
    public void AddScratch(LineRenderer lr, Transform other)
    {
        if (!mt.Has(lr))
        {
            mt.AddScratch(lr);
        }
        else
        {
            mt.ReCoroutine(lr);
        }
        Collider collider = other.GetComponent<Collider>();
        Vector3 pos = collider.ClosestPoint(lr.transform.position);
        DebugExtension.DebugWireSphere(lr.transform.position + pos, Color.white, 0.005f, 1f, true);


        // LayerMask layerMask = 1 << lr.gameObject.layer;
        // Collider collider = lr.GetComponent<Collider>();
        // Vector3 origin = lr.transform.position + 30f * (lr.transform.position + pos - collider.bounds.center).normalized;
        // Vector3 direction = -(lr.transform.position + pos - collider.bounds.center).normalized;
        // RaycastHit hit;
        // if (Physics.Raycast(origin, direction, out hit, 50f, layerMask, QueryTriggerInteraction.Ignore))
        // {
        //     DebugExtension.DebugWireSphere(hit.point, Color.blue, 0.005f, 1f, true);
        //     Debug.DrawLine(origin, hit.point, Color.blue, 1f);
        // }


    }




}
