using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Random = UnityEngine.Random;
// --- MonoBehaviour ---
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class KJHLiquid : PoolBehaviour
{
    public LayerMask collisionMask;
    public float gravityFixVelo = 0.001f;
    public float volumeSpring = 5f;
    public float edgeSpring = 50f;
    public float maxSpringVelo = 0.05f;
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;
    Mesh original;
    [HideInInspector] public Mesh copy;
    #region Entity Setting
    EntityManager entityManager;
    Entity entity;
    void InitEntity()
    {
        if (entityManager.Exists(entity)) entityManager.DestroyEntity(entity);
        entity = entityManager.CreateEntity(typeof(KJHLiquidTag));
        entityManager.AddComponentObject(entity, this);
    }
    #endregion
    uint seed;
    [HideInInspector] public NativeArray<float3> vertices;
    [HideInInspector] public NativeArray<float3> velocites;
    [HideInInspector] public NativeArray<float3> normals;
    [HideInInspector] public NativeArray<int> triangles;
    [HideInInspector] public NativeArray<bool> isAttaches;
    [HideInInspector] public NativeArray<RaycastCommand> rayComms;
    [HideInInspector] public NativeArray<RaycastHit> hits;
    [HideInInspector] public NativeArray<float3> hitClosePoints;
    [HideInInspector] public NativeArray<float3> hitCloseNormals;
    [HideInInspector] public Vector3[] verticesToArray;
    [HideInInspector] public Vector3[] normalsToArray;
    [HideInInspector] public Transform attachTarget;
    [HideInInspector] public Vector3 initTargetPos;
    [HideInInspector] public Vector3 initTrPos;
    [HideInInspector] public float3 initScale;
    [ReadOnlyInspector] public float initVolume;
    // 엣지 정보 추가
    [HideInInspector] public NativeArray<float3> initVertices;
    [HideInInspector] public NativeArray<int> edgeIndices;
    [HideInInspector] public NativeArray<int> edgeOffsets;
    void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        TryGetComponent(out mf);
        TryGetComponent(out mr);
        TryGetComponent(out mc);
    }
    void OnEnable()
    {
        Init();
    }
    void OnDisable() => UnInit();
    void OnDestroy() => Dispose();
    public void UnInit()
    {
        try
        {
            if (entityManager != null && entityManager.Exists(entity))
                entityManager.DestroyEntity(entity);
        }
        catch
        {

        }
    }
    public void Dispose()
    {
        if (vertices.IsCreated) vertices.Dispose();
        if (velocites.IsCreated) velocites.Dispose();
        if (normals.IsCreated) normals.Dispose();
        if (triangles.IsCreated) triangles.Dispose();
        if (isAttaches.IsCreated) isAttaches.Dispose();
        if (rayComms.IsCreated) rayComms.Dispose();
        if (hits.IsCreated) hits.Dispose();
        if (hitClosePoints.IsCreated) hitClosePoints.Dispose();
        if (hitCloseNormals.IsCreated) hitCloseNormals.Dispose();
    }
    public void Draw()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            verticesToArray[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z);
        }
        copy.vertices = verticesToArray;
        copy.RecalculateNormals();
        copy.RecalculateBounds();
        mc.sharedMesh = null;
        mc.sharedMesh = copy;
    }
    float CalculateSignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return Vector3.Dot(p1, Vector3.Cross(p2, p3)) / 6f;
    }
    public float CalculateMeshVolume(Mesh m)
    {
        float volume = 0f;
        Vector3[] verts = m.vertices;
        int[] tris = m.triangles;
        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v1 = verts[tris[i]];
            Vector3 v2 = verts[tris[i + 1]];
            Vector3 v3 = verts[tris[i + 2]];
            volume += CalculateSignedVolumeOfTriangle(v1, v2, v3);
        }
        return Mathf.Abs(volume);
    }

    void CreateEdges(out NativeArray<int> offsets, out NativeArray<int> indices)
    {
        // edgeCount 배열을 사용하여 각 정점에 연결된 엣지(방향성)의 총 개수를 계산
        var edgeCount = new NativeArray<int>(vertices.Length, Allocator.Temp);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            // 한 삼각형은 6개의 방향성 엣지(i0->i1, i1->i0, i1->i2, i2->i1, i2->i0, i0->i2)를 가짐
            edgeCount[i0] += 2; // i0에서 출발하는 엣지 두 개
            edgeCount[i1] += 2; // i1에서 출발하는 엣지 두 개
            edgeCount[i2] += 2; // i2에서 출발하는 엣지 두 개
        }

        // offsets 배열 생성
        offsets = new NativeArray<int>(vertices.Length + 1, Allocator.Persistent);
        offsets[0] = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            offsets[i + 1] = offsets[i] + edgeCount[i];
        }

        // indices 배열 생성 (총 엣지 개수)
        indices = new NativeArray<int>(offsets[vertices.Length], Allocator.Persistent);
        var edgePointers = new NativeArray<int>(vertices.Length, Allocator.Temp);

        // indices 배열 채우기
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            int p0 = offsets[i0] + edgePointers[i0]++;
            int p1 = offsets[i1] + edgePointers[i1]++;
            int p2 = offsets[i2] + edgePointers[i2]++;

            indices[p0] = i1;
            indices[p1] = i2;
            indices[p2] = i0;

            int p3 = offsets[i0] + edgePointers[i0]++;
            int p4 = offsets[i1] + edgePointers[i1]++;
            int p5 = offsets[i2] + edgePointers[i2]++;

            indices[p3] = i2;
            indices[p4] = i0;
            indices[p5] = i1;
        }

        edgeCount.Dispose();
        edgePointers.Dispose();
    }
    public void Init()
    {
        seed = (uint)Random.Range(0, 10000);
        if (original == null)
        {
            original = mf.mesh;
            copy = Instantiate(original);
            mf.mesh = copy;
            vertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            velocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            normals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            triangles = new NativeArray<int>(copy.triangles.Length, Allocator.Persistent);
            isAttaches = new NativeArray<bool>(copy.vertices.Length, Allocator.Persistent);
            rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
            hitClosePoints = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            hitCloseNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            initVertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            for (int i = 0; i < triangles.Length; i++)
                triangles[i] = copy.triangles[i];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
                initVertices[i] = vertices[i];
                velocites[i] = float3.zero;
                normals[i] = new float3(copy.normals[i].x, copy.normals[i].y, copy.normals[i].z);
                isAttaches[i] = false;
                hits[i] = new RaycastHit();
                hitClosePoints[i] = float3.zero;
                hitCloseNormals[i] = float3.zero;
            }
        }
        else
        {
            copy = Instantiate(original);
            for (int i = 0; i < triangles.Length; i++)
                triangles[i] = copy.triangles[i];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
                velocites[i] = float3.zero;
                normals[i] = new float3(copy.normals[i].x, copy.normals[i].y, copy.normals[i].z);
                isAttaches[i] = false;
                hits[i] = new RaycastHit();
                hitClosePoints[i] = float3.zero;
                hitCloseNormals[i] = float3.zero;
            }
        }
        initScale = transform.lossyScale;
        verticesToArray = new Vector3[vertices.Length];
        normalsToArray = new Vector3[vertices.Length];
        initVolume = CalculateMeshVolume(copy);
        CreateEdges(out edgeOffsets, out edgeIndices);
        InitEntity();
    }
}
// --- IComponentData ---
public struct KJHLiquidTag : IComponentData { }
// --- ISystem or SystemBase ---
[RequireMatchingQueriesForUpdate]
public partial class KJHLiquidSystem : SystemBase
{
    EntityCommandBufferSystem ecbSystem;
    float elapsed;
    JobHandle jobHandle1;
    JobHandle jobHandle2;
    JobHandle jobHandle3;
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnDestroy()
    {
        if (!jobHandle1.IsCompleted) jobHandle1.Complete();
        if (!jobHandle2.IsCompleted) jobHandle2.Complete();
        if (!jobHandle3.IsCompleted) jobHandle3.Complete();
    }
    protected override void OnUpdate()
    {
        Entities.ForEach((KJHLiquid mono) =>
        {
            elapsed = (float)SystemAPI.Time.ElapsedTime;
            Transform tr = mono.transform;
            if (mono.initTrPos == Vector3.zero) mono.initTrPos = mono.transform.position;
            Vector3 displacement = Vector3.zero;
            if (mono.attachTarget != null)
                displacement = mono.attachTarget.position - mono.initTargetPos;
            Vector3 pivot = tr.position + displacement;

            // --- Raycast Command Start Job ---
            var job1 = new KJHLiquidRayCommJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
                vertices = mono.vertices,
                velocites = mono.velocites,
                normals = mono.normals,
                rayComms = mono.rayComms,
                layerMask = mono.collisionMask,
            };
            jobHandle1 = job1.Schedule(mono.rayComms.Length, 64, this.Dependency);
            this.Dependency = jobHandle1;
            jobHandle1.Complete();

            // --- Raycast Command Result Job ---
            jobHandle2 = RaycastCommand.ScheduleBatch(mono.rayComms, mono.hits, 1, this.Dependency);
            this.Dependency = jobHandle2;
            jobHandle2.Complete();
            for (int i = 0; i < mono.hits.Length; i++)
            {
                if (mono.hits[i].collider != null && mono.hits[i].distance <= 1f && mono.hits[i].distance > 0f)
                {
                    mono.hitCloseNormals[i] = new float3(mono.hits[i].normal.x, mono.hits[i].normal.y, mono.hits[i].normal.z);
                    mono.hitClosePoints[i] = new float3(mono.hits[i].point.x, mono.hits[i].point.y, mono.hits[i].point.z);
                }
                else
                {
                    mono.hitCloseNormals[i] = float3.zero;
                    mono.hitClosePoints[i] = float3.zero;
                }
            }

            // --- 버섯구름 방지: 바닥 접촉 비율 계산 ---
            int contactCount = 0;
            for (int i = 0; i < mono.vertices.Length; i++)
            {
                if (math.length(mono.hitClosePoints[i]) > 0.01f)
                    contactCount++;
            }
            float contactRatio = (float)contactCount / mono.vertices.Length;
            float volumeSpringFactor = 1f;
            if (contactRatio > 0.4f) // 40% 이상 접촉 시 감쇠
            {
                float minFactor = 0.3f; // 최소 복원력 비율
                volumeSpringFactor = math.max(minFactor, math.pow(1f - contactRatio, 2f));
            }

            // --- Move Job ---
            float _currVolume = mono.CalculateMeshVolume(mono.copy);
            float deltaTime = SystemAPI.Time.DeltaTime; // 여기서 deltaTime 가져옴
            var job = new KJHLiquidMoveJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.lossyScale.x, tr.lossyScale.y, tr.lossyScale.z),
                deltaTime = deltaTime, // 수정
                vertices = mono.vertices,
                velocites = mono.velocites,
                normals = mono.normals,
                triangles = mono.triangles,
                isAttaches = mono.isAttaches,
                hitClosePoints = mono.hitClosePoints,
                hitCloseNormals = mono.hitCloseNormals,
                gravityFixVelo = mono.gravityFixVelo,
                volumeSpring = mono.volumeSpring * volumeSpringFactor, // 감쇠 적용
                edgeSpring = mono.edgeSpring,
                maxSpringVelo = mono.maxSpringVelo,
                initVolume = mono.initVolume,
                currVolume = _currVolume,
                initVertices = mono.initVertices,
                edgeIndices = mono.edgeIndices,
                edgeOffsets = mono.edgeOffsets,
            };
            jobHandle3 = job.Schedule(mono.vertices.Length, 64, this.Dependency);
            this.Dependency = jobHandle3;
            jobHandle3.Complete();

            mono.Draw();
        }).WithoutBurst().Run();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
// Job
[BurstCompile]
public partial struct KJHLiquidRayCommJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public float3 scale;
    [ReadOnly] public NativeArray<float3> vertices;
    [ReadOnly] public NativeArray<float3> velocites;
    [ReadOnly] public NativeArray<float3> normals;
    [WriteOnly] public NativeArray<RaycastCommand> rayComms;
    [ReadOnly] public LayerMask layerMask;
    public void Execute(int index)
    {
        float3 direction = velocites[index];
        if (math.length(direction) < 0.0001f)
        {
            direction = normals[index];
        }
        if (math.length(direction) < 0.0001f)
        {
            direction = math.down();
        }
        direction = math.normalize(direction);

        // 4. RaycastCommand에 전달하기 전에 최종적
        QueryParameters queryParameters = new QueryParameters();
        queryParameters.layerMask = layerMask;
        queryParameters.hitTriggers = QueryTriggerInteraction.Ignore;
        queryParameters.hitBackfaces = false;
        queryParameters.hitMultipleFaces = false;
        rayComms[index] = new RaycastCommand(pivot + scale * vertices[index], direction, queryParameters, 10f);
    }
}
[BurstCompile]
public partial struct KJHLiquidMoveJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public float3 scale;
    [ReadOnly] public float deltaTime; // elapsed 대신

    [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeArray<float3> velocites;
    [NativeDisableParallelForRestriction] public NativeArray<float3> normals;
    [ReadOnly] public NativeArray<int> triangles;
    [NativeDisableParallelForRestriction] public NativeArray<bool> isAttaches;
    [ReadOnly] public NativeArray<float3> hitClosePoints;
    [ReadOnly] public NativeArray<float3> hitCloseNormals;

    [ReadOnly] public float gravityFixVelo;
    [ReadOnly] public float volumeSpring;
    [ReadOnly] public float edgeSpring;
    [ReadOnly] public float maxSpringVelo;
    [ReadOnly] public float initVolume;
    [ReadOnly] public float currVolume;

    [ReadOnly] public NativeArray<float3> initVertices;
    [ReadOnly] public NativeArray<int> edgeIndices;
    [NativeDisableParallelForRestriction] public NativeArray<int> edgeOffsets;

    public void Execute(int index)
    {
        if (isAttaches[index]) return;

        float3 vert = vertices[index];
        float3 velo = velocites[index];

        
        velo = gravityFixVelo * math.down() / scale;

        // 부피 유지 스프링 힘
        float ratio = initVolume / currVolume;
        float3 velo_volume = (ratio - 1f) * volumeSpring * deltaTime * normals[index] / scale;
        velo_volume = math.clamp(velo_volume, -maxSpringVelo, maxSpringVelo);
        velo += velo_volume;

        // 삼각형 유지 스프링 힘
        float3 velo_edge = float3.zero;
        int startIndex = edgeOffsets[index];
        int endIndex = edgeOffsets[index + 1];
        for (int i = startIndex; i < endIndex; i++)
        {
            int adjacentIndex = edgeIndices[i];
            float3 delta = vertices[adjacentIndex] - vertices[index];
            float len = math.length(delta);
            if (len > 1e-6f)
            {
                float restLength = math.length(initVertices[index] - initVertices[adjacentIndex]);
                float diff = len - restLength;
                // diff 제한 (초기 길이의 ±50%)
                float maxDiff = restLength * 0.5f;
                diff = math.clamp(diff, -maxDiff, maxDiff);

                float3 force = (delta / len) * diff * edgeSpring * deltaTime;
                force = math.clamp(force, -maxSpringVelo, maxSpringVelo);
                velo_edge += force * 0.5f;
            }
        }
        velo += velo_edge;

        // 충돌 처리
        if (math.length(hitClosePoints[index]) > 0.01f)
        {
            float3 pos = pivot + scale * vertices[index];
            float distance = math.length(hitClosePoints[index] - pos);
            if (distance < 0.005f)
            {
                velo = float3.zero;
                isAttaches[index] = true;
            }
            else if (distance < 0.05f)
            {
                velo = math.lerp(velo, float3.zero, 30f * deltaTime);
                float veloLeng = math.length(velo);
                vert = math.lerp(vert, ((hitClosePoints[index] - pivot) / scale) + 0.1f * hitCloseNormals[index], (3f + veloLeng) * deltaTime);
            }
        }

        // 최종 속도 적용
        vert += velo * deltaTime;

        vertices[index] = vert;
        velocites[index] = velo;
    }
}