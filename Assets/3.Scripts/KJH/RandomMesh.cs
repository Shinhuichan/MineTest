using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RandomMesh : MonoBehaviour
{
    [SerializeField] GameObject[] meshes;
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;
    Mesh original;
    [HideInInspector] public Mesh copy;
    void Awake()
    {
        TryGetComponent(out mf);
        TryGetComponent(out mr);
        TryGetComponent(out mc);
    }
    void OnEnable()
    {
        if (original == null) original = mf.mesh;
        int rnd = Random.Range(0, meshes.Length);
        MeshFilter meshFilter = meshes[rnd].GetComponent<MeshFilter>();
        copy = Instantiate(meshFilter.sharedMesh);
        copy.RecalculateNormals();
        copy.RecalculateBounds();
        mf.mesh = copy;
        mc.sharedMesh = null;
        mc.sharedMesh = copy;
    }
    
}
