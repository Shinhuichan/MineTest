using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Random = UnityEngine.Random;
// --- IComponentData ---
public struct KJHLiquidDropTag : IComponentData { }
// --- MonoBehaviour ---
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class KJHLiquidDrop : PoolBehaviour
{
    public LayerMask collisionMask;
    public float gravityForce = 3f;
    public float gravityMaxSpeed;
    public float stopRatio;
    public float triggerThick = 0.01f;
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;
    Mesh original;
    [HideInInspector] public Mesh copy;
    [HideInInspector] public Mesh mcMesh;
    #region Entity Setting
    EntityManager entityManager;
    Entity entity;
    [HideInInspector] public NativeArray<float3> vertices;
    [HideInInspector] public NativeArray<VertInfo> infos;
    [HideInInspector] public NativeArray<RaycastCommand> rayComms;
    [HideInInspector] public NativeArray<RaycastHit> hits;
    [HideInInspector] public Vector3 worldCenter;
    uint seed;
    Vector3[] verticesToArray;
    public struct VertInfo
    {
        public float3 initVertex;
        public float3 vertex;
        public float3 normal;
        public float3 velocity;
        public float3 velocity_gravity;
        public float3 velocity_volume;
        public float3 velocity_edge;
        public int isAttach;
        public float3 attachVertex;
        public float3 hitNormal;
        public float3 hitPoint;
    }
    void InitEntity()
    {
        if (entityManager.Exists(entity)) entityManager.DestroyEntity(entity);
        entity = entityManager.CreateEntity(typeof(KJHLiquidDropTag));
        entityManager.AddComponentObject(entity, this);
    }
    #endregion
    void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        TryGetComponent(out mf);
        TryGetComponent(out mr);
        mc = GetComponentInChildren<MeshCollider>();
    }
    void OnEnable()
    {
        Init();
    }
    void OnDisable() => UnInit();
    void OnDestroy() => Dispose();
    public void Init()
    {
        if (original == null)
        {
            original = mf.mesh;
            copy = Instantiate(original);
            mcMesh = Instantiate(original);
            mf.mesh = copy;
            vertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            infos = new NativeArray<VertInfo>(copy.vertices.Length, Allocator.Persistent);
            rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
        }
        else
        {
            copy = Instantiate(original);
            mcMesh = Instantiate(original);
            mf.mesh = copy;
        }
        for (int i = 0; i < copy.vertices.Length; i++)
        {
            vertices[i] = copy.vertices[i];
            VertInfo info = infos[i];
            info.initVertex = copy.vertices[i];
            info.vertex = copy.vertices[i];
            info.normal = copy.normals[i];
            info.velocity = float3.zero;
            info.velocity_gravity = float3.zero;
            info.velocity_volume = float3.zero;
            info.velocity_edge = float3.zero;
            info.isAttach = 0;
            info.attachVertex = float3.zero;
            info.hitNormal = float3.zero;
            info.hitPoint = float3.zero;
            infos[i] = info;
        }
        verticesToArray = new Vector3[copy.vertices.Length];
        seed = (uint)Random.Range(0, 10000);
        InitEntity();
    }
    public void UnInit()
    {
        try
        {
            if (entityManager != null && entityManager.Exists(entity))
                entityManager.DestroyEntity(entity);
        }
        catch (System.Exception)
        {
            
        }
    }
    public void Dispose()
    {
        if (vertices.IsCreated) vertices.Dispose();
        if (infos.IsCreated) infos.Dispose();
        if (rayComms.IsCreated) rayComms.Dispose();
        if (hits.IsCreated) hits.Dispose();
    }
    public void Draw()
    {
        for (int i = 0; i < copy.vertices.Length; i++)
        {
            var info = infos[i];
            verticesToArray[i] = new Vector3(info.vertex.x, info.vertex.y, info.vertex.z);
        }
        copy.vertices = verticesToArray;
        copy.RecalculateNormals();
        copy.RecalculateBounds();
        Vector3[] vert = mcMesh.vertices;
        for (int i = 0; i < vert.Length; i++)
        {
            vert[i] = copy.vertices[i] + triggerThick * copy.normals[i];
        }
        mcMesh.vertices = vert;
        mcMesh.RecalculateNormals();
        mcMesh.RecalculateBounds();
        mc.sharedMesh = null;
        mc.sharedMesh = mcMesh;
        for (int i = 0; i < copy.vertices.Length; i++)
        {
            VertInfo info = infos[i];
            vertices[i] = copy.vertices[i];
            info.vertex = copy.vertices[i];
            info.normal = copy.normals[i];
            infos[i] = info;
        }
        worldCenter = mc.bounds.center;
    }
    public float3 ClampMagnitude(float3 v, float maxLen)
    {
        float len = math.length(v);
        if (len > maxLen)
            return (v / len) * maxLen;
        return v;
    }
}
// --- ISystem or SystemBase ---
[RequireMatchingQueriesForUpdate]
public partial class KJHLiquidDropSystem : SystemBase
{
    EntityCommandBufferSystem ecbSystem;
    float deltaTime;
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
    int count = 0;
    bool temp = false;
    protected override void OnUpdate()
    {
        Entities.ForEach((KJHLiquidDrop mono) =>
        {
            Transform tr = mono.transform;
            Vector3 pivot = tr.position;
            #region Raycast Command Start Job
            var job1 = new KJHLiquidDropRayCommJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.lossyScale.x, tr.lossyScale.y, tr.lossyScale.z),
                infos = mono.infos,
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
                KJHLiquidDrop.VertInfo info = mono.infos[i];
                if (mono.hits[i].collider != null && mono.hits[i].distance <= 2f && mono.hits[i].distance > 0f)
                {
                    info.hitNormal = new float3(mono.hits[i].normal.x, mono.hits[i].normal.y, mono.hits[i].normal.z);
                    info.hitPoint = new float3(mono.hits[i].point.x, mono.hits[i].point.y, mono.hits[i].point.z);
                    //DebugExtension.DebugWireSphere(info.hitPoint, 0.0005f, 0.02f, true);
                }
                else
                {
                    info.hitNormal = float3.zero;
                    info.hitPoint = float3.zero;
                }
                mono.infos[i] = info;
            }
#if UNITY_EDITOR
#endif
            #endregion
            #region Move Job
            int attachCount = 0;
            for (int i = 0; i < mono.infos.Length; i++)
            {
                if (mono.infos[i].isAttach > 0) attachCount++;
            }
            float ratio = (float)attachCount / mono.infos.Length;
            deltaTime = SystemAPI.Time.DeltaTime;
            var job = new KJHLiquidDropMoveJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.lossyScale.x, tr.lossyScale.y, tr.lossyScale.z),
                deltaTime = deltaTime,
                infos = mono.infos,
                gravityForce = mono.gravityForce,
                gravityMaxSpeed = mono.gravityMaxSpeed,
                stopRatio = mono.stopRatio,
                ratio = ratio,
            };
            jobHandle3 = job.Schedule(mono.infos.Length, 64, this.Dependency);
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
public partial struct KJHLiquidDropRayCommJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public float3 scale;
    [ReadOnly] public NativeArray<KJHLiquidDrop.VertInfo> infos;
    [WriteOnly] public NativeArray<RaycastCommand> rayComms;
    [ReadOnly] public LayerMask layerMask;
    public void Execute(int index)
    {
        float3 direction = math.down();
        QueryParameters queryParameters = new QueryParameters();
        queryParameters.layerMask = layerMask;
        queryParameters.hitTriggers = QueryTriggerInteraction.Ignore;
        queryParameters.hitBackfaces = false;
        queryParameters.hitMultipleFaces = false;
        rayComms[index] = new RaycastCommand(pivot + scale * infos[index].vertex, direction, queryParameters, 2f);
    }
}
// Job
[BurstCompile]
public partial struct KJHLiquidDropMoveJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public float3 scale;
    [ReadOnly] public float deltaTime;
    [ReadOnly] public uint seed;
    [ReadOnly] public float gravityForce;
    [ReadOnly] public float gravityMaxSpeed;
    [ReadOnly] public float stopRatio;
    [ReadOnly] public float ratio;
    [NativeDisableParallelForRestriction] public NativeArray<KJHLiquidDrop.VertInfo> infos;
    public void Execute(int index)
    {
        // 불러오기
        KJHLiquidDrop.VertInfo info = infos[index];
        if (info.isAttach == 2) return;
        uint seed = this.seed + (uint)index;
        if (info.isAttach == 0)
        {
            if (ratio >= stopRatio && math.length(info.hitPoint) > 0.001f)
            {
                if (math.length(info.attachVertex) < 0.001f)
                    info.attachVertex = info.vertex;
                info.velocity_gravity = float3.zero;
                info.velocity = float3.zero;
                info.vertex = math.lerp(info.vertex, 0.5f * ((info.hitPoint - pivot) / scale + info.attachVertex), 1.5f * deltaTime);
                // 덮어쓰기
                infos[index] = info;
                return;
            }
            // 중력
            info.velocity_gravity += gravityForce * math.down() * deltaTime;
            info.velocity_gravity = ClampMagnitude(info.velocity_gravity, gravityMaxSpeed);
        }
        // 진행하다가 전방에 충돌된 경우 (완전 비탄성 --> 벽에 달라붙어서 속도 0 --> 충돌 포인트에서 더이상 이동 하지않음)
        if (math.length(info.hitNormal) > 0.01f || info.isAttach >= 1)
        {
            float3 pos = pivot + scale * info.vertex;
            float distance = math.length(info.hitPoint - pos);
            if (distance < 0.011f)
            {
                info.isAttach = 2;
                info.velocity_gravity = float3.zero;
                info.velocity = float3.zero;
                info.vertex = ((info.hitPoint - pivot) / scale) + 0.01f * math.up();
            }
            else if (distance < 0.05f)
            {
                info.isAttach = 1;
                if (math.length(info.attachVertex) < 0.001f)
                    info.attachVertex = info.vertex;
                info.velocity_gravity = float3.zero;
                info.velocity = float3.zero;
                // 위치 서서히 히트 포인트에 붇는 시각적 효과
                float veloLeng = math.length(info.velocity);
                info.vertex = math.lerp(info.vertex, ((info.hitPoint - pivot) / scale) + 0.01f * math.up(), 0.8f * (2f + veloLeng) * deltaTime);
            }
            else if (info.isAttach >= 1)
            {
                info.isAttach = 0;
            }
        }
        // 최종 속도 적용
        info.velocity = info.velocity_gravity + info.velocity_edge + info.velocity_volume;
        info.vertex += info.velocity * deltaTime;
        // 덮어쓰기
        infos[index] = info;
    }
    float3 ClampMagnitude(float3 v, float maxLen)
    {
        float len = math.length(v);
        if (len > maxLen)
            return (v / len) * maxLen;
        return v;
    }
}
















