using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class VertexDropperElastic : MonoBehaviour
{
    [Header("Physics")]
    public Vector3 gravity = new Vector3(0f, -9.81f, 0f);
    public float planeY = 0f;
    public float terminalVelocity = 50f;

    [Header("Elastic Settings")]
    [Tooltip("원래 위치로 복원하려는 힘의 세기")]
    public float springStrength = 20f;
    [Tooltip("복원 시 감속(댐핑). 0=즉시감속, 1=감속 없음")]
    [Range(0f, 1f)]
    public float damping = 0.9f;

    Mesh workingMesh;
    Vector3[] originalVertices;
    Vector3[] currentVertices;
    Vector3[] velocities;
    bool[] grounded;

    void Start()
    {
        var mf = GetComponent<MeshFilter>();
        workingMesh = Instantiate(mf.sharedMesh);
        workingMesh.MarkDynamic();
        mf.mesh = workingMesh;

        originalVertices = workingMesh.vertices;
        currentVertices = new Vector3[originalVertices.Length];
        velocities = new Vector3[originalVertices.Length];
        grounded = new bool[originalVertices.Length];

        System.Array.Copy(originalVertices, currentVertices, originalVertices.Length);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < currentVertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(currentVertices[i]);

            // 스프링 힘 (원래 위치로 되돌아가려는 힘)
            Vector3 targetWorldPos = transform.TransformPoint(originalVertices[i]);
            Vector3 springForce = (targetWorldPos - worldPos) * springStrength;

            // 중력 + 스프링
            Vector3 acceleration = gravity + springForce;

            velocities[i] += acceleration * dt;
            velocities[i] = Vector3.ClampMagnitude(velocities[i], terminalVelocity);

            // 위치 업데이트
            worldPos += velocities[i] * dt;

            // 충돌 체크
            if (worldPos.y <= planeY)
            {
                worldPos.y = planeY;
                velocities[i].y = -velocities[i].y * 0.3f; // 살짝 튀기기 (반발계수 0.3)
                if (Mathf.Abs(velocities[i].y) < 0.01f)
                    velocities[i].y = 0;
            }

            // 속도 댐핑
            velocities[i] *= damping;

            // 로컬로 변환
            currentVertices[i] = transform.InverseTransformPoint(worldPos);
        }

        workingMesh.vertices = currentVertices;
        workingMesh.RecalculateNormals();
        workingMesh.RecalculateBounds();
    }
}
