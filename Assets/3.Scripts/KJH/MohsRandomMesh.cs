using UnityEngine;
public class MohsRandomMesh : MonoBehaviour
{
    [SerializeField] Mesh[] meshes1;
    [SerializeField] Mesh[] meshes2;
    MeshFilter mf1;
    MeshFilter mf2;
    MeshRenderer mr1;
    MeshRenderer mr2;
    MeshCollider mc1;
    void Awake()
    {
        transform.Find("Stone").TryGetComponent(out mf1);
        transform.Find("Stone").TryGetComponent(out mr1);
        transform.Find("Stone").TryGetComponent(out mc1);
        transform.Find("NameBend").TryGetComponent(out mf2);
        transform.Find("NameBend").TryGetComponent(out mr2);
    }
    void OnEnable()
    {
        int rnd = Random.Range(0, 3);
        mf1.mesh = Instantiate(meshes1[rnd]);
        mf2.mesh = Instantiate(meshes2[rnd]);
        mf1.mesh.RecalculateNormals();
        mf2.mesh.RecalculateNormals();
        mf1.mesh.RecalculateBounds();
        mf2.mesh.RecalculateBounds();
        mc1.sharedMesh = null;
        mc1.sharedMesh = mf1.mesh;
    }

}
