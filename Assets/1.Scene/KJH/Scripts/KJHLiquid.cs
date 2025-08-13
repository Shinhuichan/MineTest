// using UnityEngine;
// using Unity.Mathematics;
// using Unity.Burst;
// using Unity.Jobs;
// using Unity.Entities;
// using Unity.Collections;
// using Unity.Transforms;
// using Random = UnityEngine.Random;
// // --- MonoBehaviour ---
// [RequireComponent(typeof(MeshFilter))]
// [RequireComponent(typeof(MeshRenderer))]
// [RequireComponent(typeof(MeshCollider))]
// public class KJHLiquid : PoolBehaviour
// {
//     public LayerMask collisionMask;
//     public float gravityFixVelo = 0.001f;
//     public float volumeSpring = 5f;
//     public float edgeSpring = 50f;
//     public float maxSpringVelo = 0.05f;
//     MeshFilter mf;
//     MeshRenderer mr;
//     MeshCollider mc;
//     Mesh original;
//     [HideInInspector] public Mesh copy;
//     #region Entity Setting
//     EntityManager entityManager;
//     Entity entity;
//     void InitEntity()
//     {
//         if (entityManager.Exists(entity)) entityManager.DestroyEntity(entity);
//         entity = entityManager.CreateEntity(typeof(KJHLiquidTag));
//         entityManager.AddComponentObject(entity, this);
//     }
//     #endregion
//     uint seed;
//     [HideInInspector] public NativeArray<float3> vertices;
//     [HideInInspector] public NativeArray<float3> velocites;
//     [HideInInspector] public NativeArray<float3> normals;
//     [HideInInspector] public NativeArray<int> triangles;
//     [HideInInspector] public NativeArray<bool> isAttaches;
//     [HideInInspector] public NativeArray<RaycastCommand> rayComms;
//     [HideInInspector] public NativeArray<RaycastHit> hits;
//     [HideInInspector] public NativeArray<float3> hitClosePoints;
//     [HideInInspector] public NativeArray<float3> hitCloseNormals;
//     [HideInInspector] public Vector3[] verticesToArray;
//     [HideInInspector] public Vector3[] normalsToArray;
//     [HideInInspector] public Transform attachTarget;
//     [HideInInspector] public Vector3 initTargetPos;
//     [HideInInspector] public Vector3 initTrPos;
//     [HideInInspector] public float3 initScale;
//     [ReadOnlyInspector] public float initVolume = -999f;
//     void Awake()
//     {
//         entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
//         TryGetComponent(out mf);
//         TryGetComponent(out mr);
//         TryGetComponent(out mc);
//     }
//     void OnEnable()
//     {
//         Init();
//     }
//     void OnDisable() => UnInit();
//     void OnDestroy() => Dispose();
//     void UnInit()
//     {
//         if (entityManager.Exists(entity)) entityManager.DestroyEntity(entity);
//     }
//     void Dispose()
//     {
//         if (vertices.IsCreated) vertices.Dispose();
//         if (velocites.IsCreated) velocites.Dispose();
//         if (normals.IsCreated) normals.Dispose();
//         if (triangles.IsCreated) triangles.Dispose();
//         if (isAttaches.IsCreated) isAttaches.Dispose();
//         if (rayComms.IsCreated) rayComms.Dispose();
//         if (hits.IsCreated) hits.Dispose();
//         if (hitClosePoints.IsCreated) hitClosePoints.Dispose();
//         if (hitCloseNormals.IsCreated) hitCloseNormals.Dispose();
//     }
//     public void Draw()
//     {
//         for (int i = 0; i < vertices.Length; i++)
//         {
//             verticesToArray[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z);
//         }
//         copy.vertices = verticesToArray;
//         copy.RecalculateNormals();
//         copy.RecalculateBounds();
//         mc.sharedMesh = null;
//         mc.sharedMesh = copy;
//     }
//     public void Init()
//     {
//         seed = (uint)Random.Range(0, 10000);
//         if (original == null)
//         {
//             original = mf.mesh;
//             copy = Instantiate(original);
//             mf.mesh = copy;
//             vertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             velocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             normals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             triangles = new NativeArray<int>(copy.triangles.Length, Allocator.Persistent);
//             isAttaches = new NativeArray<bool>(copy.vertices.Length, Allocator.Persistent);
//             rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
//             hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
//             hitClosePoints = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             hitCloseNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             for (int i = 0; i < triangles.Length; i++)
//                 triangles[i] = copy.triangles[i];
//             for (int i = 0; i < vertices.Length; i++)
//             {
//                 vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
//                 velocites[i] = float3.zero;
//                 normals[i] = new float3(copy.normals[i].x, copy.normals[i].y, copy.normals[i].z);
//                 isAttaches[i] = false;
//                 hits[i] = new RaycastHit();
//                 hitClosePoints[i] = float3.zero;
//                 hitCloseNormals[i] = float3.zero;
//             }
//         }
//         else
//         {
//             copy = Instantiate(original);
//             for (int i = 0; i < triangles.Length; i++)
//                 triangles[i] = copy.triangles[i];
//             for (int i = 0; i < vertices.Length; i++)
//             {
//                 vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
//                 velocites[i] = float3.zero;
//                 normals[i] = new float3(copy.normals[i].x, copy.normals[i].y, copy.normals[i].z);
//                 isAttaches[i] = false;
//                 hits[i] = new RaycastHit();
//                 hitClosePoints[i] = float3.zero;
//                 hitCloseNormals[i] = float3.zero;
//             }
//         }
//         initScale = transform.lossyScale;
//         verticesToArray = new Vector3[vertices.Length];
//         normalsToArray = new Vector3[vertices.Length];
//         InitEntity();
//     }
// }
// // --- IComponentData ---
// public struct KJHLiquidTag : IComponentData { }
// // --- ISystem or SystemBase ---
// [RequireMatchingQueriesForUpdate]
// public partial class KJHLiquidSystem : SystemBase
// {
//     EntityCommandBufferSystem ecbSystem;
//     float elapsed;
//     JobHandle jobHandle1;
//     JobHandle jobHandle2;
//     JobHandle jobHandle3;
//     protected override void OnCreate()
//     {
//         ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
//     }
//     protected override void OnDestroy()
//     {
//         if (!jobHandle1.IsCompleted) jobHandle1.Complete();
//         if (!jobHandle2.IsCompleted) jobHandle2.Complete();
//         if (!jobHandle3.IsCompleted) jobHandle3.Complete();
//     }
//     protected override void OnUpdate()
//     {
//         Entities.ForEach((KJHLiquid mono) =>
//         {
//             elapsed = (float)SystemAPI.Time.ElapsedTime;
//             Transform tr = mono.transform;
//             if (mono.initTrPos == Vector3.zero) mono.initTrPos = mono.transform.position;
//             Vector3 displacement = Vector3.zero;
//             if (mono.attachTarget != null)
//                 displacement = mono.attachTarget.position - mono.initTargetPos;
//             Vector3 pivot = tr.position + displacement;

//             #region Raycast Command Start Job
//             var job1 = new KJHLiquidRayCommJob
//             {
//                 pivot = new float3(pivot.x, pivot.y, pivot.z),
//                 scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
//                 vertices = mono.vertices,
//                 velocites = mono.velocites,
//                 normals = mono.normals,
//                 rayComms = mono.rayComms,
//                 layerMask = mono.collisionMask,
//             };
//             jobHandle1 = job1.Schedule(mono.rayComms.Length, 64, this.Dependency);
//             this.Dependency = jobHandle1;
//             jobHandle1.Complete();
//             #endregion

//             #region Raycast Command Result Job
//             jobHandle2 = RaycastCommand.ScheduleBatch(mono.rayComms, mono.hits, 1, this.Dependency);
//             this.Dependency = jobHandle2;
//             jobHandle2.Complete();
//             for (int i = 0; i < mono.hits.Length; i++)
//             {
//                 if (mono.hits[i].collider != null && mono.hits[i].distance <= 1f && mono.hits[i].distance > 0f)
//                 {
//                     mono.hitCloseNormals[i] = new float3(mono.hits[i].normal.x, mono.hits[i].normal.y, mono.hits[i].normal.z);
//                     mono.hitClosePoints[i] = new float3(mono.hits[i].point.x, mono.hits[i].point.y, mono.hits[i].point.z);
//                 }
//                 else
//                 {
//                     mono.hitCloseNormals[i] = float3.zero;
//                     mono.hitClosePoints[i] = float3.zero;
//                 }
//             }
// #if UNITY_EDITOR
//             if (math.length(mono.hitCloseNormals[0]) > 0.01f)
//             {
//                 Vector3 pos = pivot + new Vector3(tr.localScale.x * mono.vertices[0].x,
//                 tr.localScale.y * mono.vertices[0].y, tr.localScale.z * mono.vertices[0].z);
//                 float distance = math.length(mono.hitClosePoints[0] - new float3(pos.x, pos.y, pos.z));
//                 if (distance < 0.13f)
//                 {
//                     Debug.DrawLine(pos, mono.hitClosePoints[0], Color.red, 0.1f, true);
//                 }
//                 else if (distance < 0.25f)
//                 {
//                     Debug.DrawLine(pos, mono.hitClosePoints[0], Color.yellow, 0.1f, true);
//                 }
//                 else
//                 {
//                     Debug.DrawLine(pos, mono.hitClosePoints[0], Color.gray, 0.1f, true);
//                 }
//             }
// #endif
//             #endregion

//             #region Move Job
//             elapsed = (float)SystemAPI.Time.ElapsedTime - elapsed;
//             elapsed += SystemAPI.Time.DeltaTime;
//             var job = new KJHLiquidMoveJob
//             {
//                 pivot = new float3(pivot.x, pivot.y, pivot.z),
//                 scale = new float3(tr.lossyScale.x, tr.lossyScale.y, tr.lossyScale.z),
//                 elapsed = elapsed,
//                 vertices = mono.vertices,
//                 velocites = mono.velocites,
//                 normals = mono.normals,
//                 triangles = mono.triangles,
//                 isAttaches = mono.isAttaches,
//                 hitClosePoints = mono.hitClosePoints,
//                 hitCloseNormals = mono.hitCloseNormals,
//                 gravityFixVelo = mono.gravityFixVelo,
//                 volumeSpring = mono.volumeSpring,
//                 edgeSpring = mono.edgeSpring,
//                 maxSpringVelo = mono.maxSpringVelo,
//             };
//             jobHandle3 = job.Schedule(mono.vertices.Length, 64, this.Dependency);
//             this.Dependency = jobHandle3;
//             jobHandle3.Complete();
//             #endregion

//             mono.Draw();
//         }).WithoutBurst().Run();
//         ecbSystem.AddJobHandleForProducer(Dependency);

//     }

// }
// // Job
// [BurstCompile]
// public partial struct KJHLiquidRayCommJob : IJobParallelFor
// {
//     [ReadOnly] public float3 pivot;
//     [ReadOnly] public float3 scale;
//     [ReadOnly] public NativeArray<float3> vertices;
//     [ReadOnly] public NativeArray<float3> velocites;
//     [ReadOnly] public NativeArray<float3> normals;
//     [WriteOnly] public NativeArray<RaycastCommand> rayComms;
//     [ReadOnly] public LayerMask layerMask;
//     public void Execute(int index)
//     {
//         float3 direction;
//         direction = normals[index];
//         if (math.length(vertices[index]) >= 0.01f)
//             direction = math.normalize(vertices[index]);
//         QueryParameters queryParameters = new QueryParameters();
//         queryParameters.layerMask = layerMask;
//         queryParameters.hitTriggers = QueryTriggerInteraction.Ignore;
//         queryParameters.hitBackfaces = false;
//         queryParameters.hitMultipleFaces = false;
//         rayComms[index] = new RaycastCommand(pivot + scale * vertices[index], direction, queryParameters, 10f);
//     }
// }
// [BurstCompile]
// public partial struct KJHLiquidMoveJob : IJobParallelFor
// {
//     [ReadOnly] public float3 pivot;
//     [ReadOnly] public float3 scale;
//     [ReadOnly] public float elapsed;
//     //
//     [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
//     [NativeDisableParallelForRestriction] public NativeArray<float3> velocites;
//     [NativeDisableParallelForRestriction] public NativeArray<float3> normals;
//     [ReadOnly] public NativeArray<int> triangles;
//     [NativeDisableParallelForRestriction] public NativeArray<bool> isAttaches;
//     [ReadOnly] public NativeArray<float3> hitClosePoints;
//     [ReadOnly] public NativeArray<float3> hitCloseNormals;
//     //
//     [ReadOnly] public float gravityFixVelo;
//     [ReadOnly] public float volumeSpring;
//     [ReadOnly] public float edgeSpring;
//     [ReadOnly] public float maxSpringVelo;
//     float3 ApplyEdgeSpring()
//     {
//         float3 resultMyVertexSpeed;

//         resultMyVertexSpeed = float3.zero;

//         return resultMyVertexSpeed;
//     }
//     public void Execute(int index)
//     {
//         if (isAttaches[index]) return;

//         // 불러오기
//         float3 vert = vertices[index];
//         float3 velo = velocites[index];

//         // 중력 (등속도 고정 중력속도)
//         velo = gravityFixVelo * math.down() / scale;

//         // 부피 유지 스프링 힘 (법선 방향)
//         float ratio = 1;
//         float3 velo_volume = (ratio - 1f) * volumeSpring * elapsed * normals[index];
//         velo_volume = math.clamp(velo_volume, -maxSpringVelo, maxSpringVelo) / scale;
//         velo += velo_volume;

//         // 삼각형 유지 스프링 힘 (초기 삼각형 세변 길이 기준)
//         float3 velo_edge = ApplyEdgeSpring();
//         velo_edge = math.clamp(velo_edge, -maxSpringVelo, maxSpringVelo);
//         velo += velo_edge;

//         // 진행하다가 전방에 충돌된 경우 (완전 비탄성 --> 벽에 달라붙어서 속도 0 --> 충돌 포인트에서 더이상 이동 하지않음)
//         if (math.length(hitClosePoints[index]) > 0.01f)
//         {
//             float3 pos = pivot + scale * vertices[index];
//             float distance = math.length(hitClosePoints[index] - pos);
//             if (distance < 0.005f)
//             {
//                 velo = float3.zero;
//                 isAttaches[index] = true;
//             }
//             else if (distance < 0.05f)
//             {
//                 velo = math.lerp(velo, float3.zero, 30f * elapsed);
//                 // 위치 서서히 히트 포인트에 붇는 시각적 효과
//                 float veloLeng = math.length(velo);
//                 vert = math.lerp(vert, ((hitClosePoints[index] - pivot) / scale) + 0.1f * hitCloseNormals[index], (3f + veloLeng) * elapsed);
//             }
//         }

//         // 최종 속도 적용
//         vert += velo * elapsed;

//         // 덮어쓰기
//         vertices[index] = vert;
//         velocites[index] = velo;
//     }
// }







// //     int callCount;
// //     void OnCollisionEnter(Collision collision)
// //     {
// //         if (collision.collider.name == "Ore1")
// //         {
// //             callCount++;
// //             if (callCount % 20 == 0)
// //             {
// //                 ParticleManager.I.PlayParticle("DustSmall", collision.contacts[0].point, Quaternion.identity, null);
// //                 SoundManager.I.PlaySFX("LiquidDrop", collision.contacts[0].point, null, 0.8f);
// //             }
// //             else if (callCount > 100)
// //             {
// //                 UnInit();
// //                 Despawn();
// //             }
// //         }
// //     }



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
    [HideInInspector] public NativeArray<float3> initialVertices;
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
    void UnInit()
    {
        if (entityManager.Exists(entity)) entityManager.DestroyEntity(entity);
    }
    void Dispose()
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
        if (initialVertices.IsCreated) initialVertices.Dispose();
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
    public void Init()
    {
        seed = (uint)Random.Range(0, 10000);
        if (original == null)
        {
            original = mf.mesh;
            copy = Instantiate(original);
            mf.mesh = copy;
            vertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            initialVertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            velocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            normals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            triangles = new NativeArray<int>(copy.triangles.Length, Allocator.Persistent);
            isAttaches = new NativeArray<bool>(copy.vertices.Length, Allocator.Persistent);
            rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
            hitClosePoints = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            hitCloseNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            for (int i = 0; i < triangles.Length; i++)
                triangles[i] = copy.triangles[i];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
                initialVertices[i] = vertices[i];
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
                initialVertices[i] = vertices[i];
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
        InitEntity();
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
            #region Raycast Command Start Job
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
            #endregion
            #region Raycast Command Result Job
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
#if UNITY_EDITOR
            if (math.length(mono.hitCloseNormals[0]) > 0.01f)
            {
                Vector3 pos = pivot + new Vector3(tr.localScale.x * mono.vertices[0].x,
                tr.localScale.y * mono.vertices[0].y, tr.localScale.z * mono.vertices[0].z);
                float distance = math.length(mono.hitClosePoints[0] - new float3(pos.x, pos.y, pos.z));
                if (distance < 0.13f)
                {
                    Debug.DrawLine(pos, mono.hitClosePoints[0], Color.red, 0.1f, true);
                }
                else if (distance < 0.25f)
                {
                    Debug.DrawLine(pos, mono.hitClosePoints[0], Color.yellow, 0.1f, true);
                }
                else
                {
                    Debug.DrawLine(pos, mono.hitClosePoints[0], Color.gray, 0.1f, true);
                }
            }
#endif
            #endregion
            #region Move Job
            elapsed = (float)SystemAPI.Time.ElapsedTime - elapsed;
            elapsed += SystemAPI.Time.DeltaTime;
            float currentVolume = mono.CalculateMeshVolume(mono.copy);
            float ratio = mono.initVolume / currentVolume;
            var job = new KJHLiquidMoveJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.lossyScale.x, tr.lossyScale.y, tr.lossyScale.z),
                elapsed = elapsed,
                vertices = mono.vertices,
                initialVertices = mono.initialVertices,
                velocites = mono.velocites,
                normals = mono.normals,
                triangles = mono.triangles,
                isAttaches = mono.isAttaches,
                hitClosePoints = mono.hitClosePoints,
                hitCloseNormals = mono.hitCloseNormals,
                gravityFixVelo = mono.gravityFixVelo,
                volumeSpring = mono.volumeSpring,
                edgeSpring = mono.edgeSpring,
                maxSpringVelo = mono.maxSpringVelo,
                ratio = ratio,
                initVolume = mono.initVolume,
            };
            jobHandle3 = job.Schedule(mono.vertices.Length, 64, this.Dependency);
            this.Dependency = jobHandle3;
            jobHandle3.Complete();
            #endregion
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
        float3 direction;
        direction = normals[index];
        if (math.length(vertices[index]) >= 0.01f)
            direction = math.normalize(vertices[index]);
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
    [ReadOnly] public float elapsed;
    //
    [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeArray<float3> initialVertices;
    [NativeDisableParallelForRestriction] public NativeArray<float3> velocites;
    [NativeDisableParallelForRestriction] public NativeArray<float3> normals;
    [ReadOnly] public NativeArray<int> triangles;
    [NativeDisableParallelForRestriction] public NativeArray<bool> isAttaches;
    [ReadOnly] public NativeArray<float3> hitClosePoints;
    [ReadOnly] public NativeArray<float3> hitCloseNormals;
    //
    [ReadOnly] public float gravityFixVelo;
    [ReadOnly] public float volumeSpring;
    [ReadOnly] public float edgeSpring;
    [ReadOnly] public float maxSpringVelo;
    [ReadOnly] public float ratio;
    //
    [ReadOnly] public float initVolume;
    float3 ApplyEdgeSpring(int iA, int iB)
    {
        float3 delta = vertices[iB] - vertices[iA];
        float currentLength = math.length(delta);
        float restLength = math.length(initialVertices[iA] - initialVertices[iB]);
        float diff = currentLength - restLength;
        float3 force = math.normalize(delta) * diff * edgeSpring * elapsed;
        return force;
    }
    public void Execute(int index)
    {
        if (isAttaches[index]) return;
        // 불러오기
        float3 vert = vertices[index];
        float3 velo = velocites[index];
        // 중력
        velo = gravityFixVelo * math.down();
        // 부피 유지 스프링 힘 (법선 방향)
        float ratio = initVolume / CalculateMeshVolume(vertices, triangles);

        float3 velo_volume = normals[index] * (ratio - 1f) * volumeSpring * elapsed;
        velo_volume = math.clamp(velo_volume, -maxSpringVelo, maxSpringVelo);
        velo += velo_volume;
        // 삼각형 유지 스프링 힘 (초기 삼각형 세변 길이 기준)
        float3 velo_edge = float3.zero;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (triangles[i] == index)
            {
                velo_edge += ApplyEdgeSpring(triangles[i], triangles[i + 1]) * 0.5f;
                velo_edge += ApplyEdgeSpring(triangles[i], triangles[i + 2]) * 0.5f;
            }
            else if (triangles[i + 1] == index)
            {
                velo_edge += ApplyEdgeSpring(triangles[i + 1], triangles[i]) * 0.5f;
                velo_edge += ApplyEdgeSpring(triangles[i + 1], triangles[i + 2]) * 0.5f;
            }
            else if (triangles[i + 2] == index)
            {
                velo_edge += ApplyEdgeSpring(triangles[i + 2], triangles[i]) * 0.5f;
                velo_edge += ApplyEdgeSpring(triangles[i + 2], triangles[i + 1]) * 0.5f;
            }
        }
        velo_edge = math.clamp(velo_edge, -maxSpringVelo, maxSpringVelo);
        velo += velo_edge;
        // 진행하다가 전방에 충돌된 경우 (완전 비탄성 --> 벽에 달라붙어서 속도 0 --> 충돌 포인트에서 더이상 이동 하지않음)
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
                velo = math.lerp(velo, float3.zero, 30f * elapsed);
                // 위치 서서히 히트 포인트에 붇는 시각적 효과
                float veloLeng = math.length(velo);
                vert = math.lerp(vert, ((hitClosePoints[index] - pivot) / scale) + 0.1f * hitCloseNormals[index], (3f + veloLeng) * elapsed);
            }
        }
        // 최종 속도 적용
        vert += velo * elapsed;
        // 덮어쓰기
        vertices[index] = vert;
        velocites[index] = velo;
    }
    float CalculateSignedVolumeOfTriangle(float3 p1, float3 p2, float3 p3)
    {
        return math.dot(p1, math.cross(p2,p3)) / 6f;
    }
    public float CalculateMeshVolume(NativeArray<float3> verts, NativeArray<int> tris)
    {
        float volume = 0f;
        for (int i = 0; i < tris.Length; i += 3)
        {
            float3 v1 = verts[tris[i]];
            float3 v2 = verts[tris[i + 1]];
            float3 v3 = verts[tris[i + 2]];
            volume += CalculateSignedVolumeOfTriangle(v1, v2, v3);
        }
        return Mathf.Abs(volume);
    }
}
