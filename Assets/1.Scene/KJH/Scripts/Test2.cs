using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class VertexCollisionVisualizer : MonoBehaviour
{
    [Header("Physics Settings")]
    public float rayLength = 0.1f;
    public LayerMask collisionMask;

    [Header("Forces")]
    public float gravityStrength = 0.001f;       // 낙하 강도
    public float volumeStiffness = 5f;           // 볼륨 유지 강도
    public float edgeStiffness = 50f;            // 삼각형 엣지 스프링 강도
    public float maxVelocity = 0.05f;            // 반발력 최대 속도 제한

    Mesh mesh;
    Vector3[] vertices;
    Vector3[] velocities;
    Vector3[] worldVertices;
    bool[] isFrozen;
    int[] triangles;
    Vector3[] normals;
    Vector3[] initialVertices;

    float initialVolume;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        initialVertices = (Vector3[])vertices.Clone();
        velocities = new Vector3[vertices.Length];
        worldVertices = new Vector3[vertices.Length];
        isFrozen = new bool[vertices.Length];
        triangles = mesh.triangles;
        normals = mesh.normals;

        initialVolume = CalculateMeshVolume(mesh);
        Debug.Log($"Initial Volume: {initialVolume}");
    }

    void Update()
    {
        for (int i = 0; i < vertices.Length; i++)
            worldVertices[i] = transform.TransformPoint(vertices[i]);

        float currentVolume = CalculateMeshVolume(mesh);
        float ratio = initialVolume / currentVolume;

        // 부피 유지 + 중력
        for (int i = 0; i < vertices.Length; i++)
        {
            if (isFrozen[i]) continue;

            Vector3 vel = velocities[i];

            // 중력
            vel += Vector3.down * gravityStrength;

            // 부피 유지 속도 (법선 방향)
            Vector3 normal = normals[i];
            Vector3 velVol = normal * (ratio - 1f) * volumeStiffness * Time.deltaTime;

            // 최대 속도 제한
            velVol = Vector3.ClampMagnitude(velVol, maxVelocity);

            vel += velVol;

            velocities[i] = vel;
        }

        // 엣지 스프링 적용 (초기 길이 기준)
        ApplyEdgeConstraints();

        // 충돌 처리
        for (int i = 0; i < vertices.Length; i++)
        {
            if (isFrozen[i]) continue;

            Vector3 vel = velocities[i];
            Vector3 dir = vel.normalized;
            if (dir != Vector3.zero &&
                Physics.Raycast(worldVertices[i], dir, out RaycastHit hit, rayLength, collisionMask))
            {
                velocities[i] = Vector3.zero;
                isFrozen[i] = true;
            }
            else
            {
                vertices[i] += vel * Time.deltaTime;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void ApplyEdgeConstraints()
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            ApplyEdgeSpring(i0, i1);
            ApplyEdgeSpring(i1, i2);
            ApplyEdgeSpring(i2, i0);
        }
    }

    void ApplyEdgeSpring(int iA, int iB)
    {
        Vector3 delta = vertices[iB] - vertices[iA];
        float currentLength = delta.magnitude;
        float restLength = Vector3.Distance(initialVertices[iA], initialVertices[iB]);
        float diff = currentLength - restLength;

        Vector3 force = delta.normalized * diff * edgeStiffness * Time.deltaTime;

        // 속도 상한 적용
        force = Vector3.ClampMagnitude(force, maxVelocity);

        if (!isFrozen[iA]) velocities[iA] += force * 0.5f;
        if (!isFrozen[iB]) velocities[iB] -= force * 0.5f;
    }

    float CalculateMeshVolume(Mesh m)
    {
        float volume = 0f;
        Vector3[] verts = m.vertices;
        int[] tris = m.triangles;

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v1 = verts[tris[i]];
            Vector3 v2 = verts[tris[i + 1]];
            Vector3 v3 = verts[tris[i + 2]];
            volume += SignedVolumeOfTriangle(v1, v2, v3);
        }
        return Mathf.Abs(volume);
    }

    float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Vector3.Dot(p1, Vector3.Cross(p2, p3)) / 6f;
    }
}
