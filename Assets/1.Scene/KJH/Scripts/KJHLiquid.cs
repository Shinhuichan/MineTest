using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Random = UnityEngine.Random;
/////////////////
// IComponentData
/////////////////
public struct KJHLiquidTag : IComponentData { }
/////////////////
// MonoBehaviour
/////////////////
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class KJHLiquid : PoolBehaviour
{
    [Header("Spring (mesh-based)")]
    [Tooltip("엣지(버텍스-이웃) 스프링 강성")]
    public float springStrength = 50f;
    [Tooltip("스프링 감쇠: 상대속도에 의해 에너지를 흡수")]
    public float springDamping = 2f;
    [Tooltip("충돌 시 '너무 가까움' 판정 임계값 (월드 단위)")]
    public float contactThreshold = 0.02f;
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
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;
    Mesh original;
    [HideInInspector] public Mesh copy;
    [HideInInspector] public NativeArray<float3> vertices;
    [HideInInspector] public NativeArray<float3> velocites;
    [HideInInspector] public NativeArray<RaycastCommand> rayComms;
    [HideInInspector] public NativeArray<RaycastHit> hits;
    [HideInInspector] public NativeArray<LocalInfo> localInfos;
    [HideInInspector] public NativeArray<float3> hitCloseNormals;
    [HideInInspector] public NativeArray<float3> hitClosePoints;
    [HideInInspector] public Vector3[] verticesToArray;
    [HideInInspector] public Vector3[] normalsToArray;
    List<(int index, float dist)> list = new List<(int, float)>();
    public struct LocalInfo
    {
        public float3 pos;
        public float3 normal;
        public float3 velocity;
        public float3 velocity_gravity;
        public float3 velocity_curvature;
        public float3 velocity_volume;
        public float localCurvature;
        public int neighborCount;
        public int n0, n1, n2, n3, n4, n5;
        public float r0, r1, r2, r3, r4, r5;
    }
    [HideInInspector] public Transform attachTarget;
    [HideInInspector] public Vector3 initTargetPos;
    [HideInInspector] public Vector3 initTrPos;
    [ReadOnlyInspector] public float initVolume = -1f;
    int callCount;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Ore1")
        {
            callCount++;
            if (callCount % 20 == 0)
            {
                ParticleManager.I.PlayParticle("DustSmall", collision.contacts[0].point, Quaternion.identity, null);
                SoundManager.I.PlaySFX("LiquidDrop", collision.contacts[0].point, null, 0.8f);
            }
            else if (callCount > 100)
            {
                UnInit();
                Despawn();
            }
        }
    }
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
        callCount = 0;
        if (entityManager.Exists(entity)) entityManager.DestroyEntity(entity);
    }
    void Dispose()
    {
        if (vertices.IsCreated) vertices.Dispose();
        if (velocites.IsCreated) velocites.Dispose();
        if (rayComms.IsCreated) rayComms.Dispose();
        if (hits.IsCreated) hits.Dispose();
        if (localInfos.IsCreated) localInfos.Dispose();
        if (hitCloseNormals.IsCreated) hitCloseNormals.Dispose();
        if (hitClosePoints.IsCreated) hitClosePoints.Dispose();
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
    //////////////////////////
    public void Init()
    {
        seed = (uint)Random.Range(0, 10000);
        if (original == null)
        {
            original = mf.mesh;
            copy = Instantiate(original);
            vertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            velocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
            localInfos = new NativeArray<LocalInfo>(copy.vertices.Length, Allocator.Persistent);
            hitCloseNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            hitClosePoints = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            for (int i = 0; i < vertices.Length; i++)
            {
                LocalInfo info = localInfos[i];
                vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
                velocites[i] = float3.zero;
                info.normal = new float3(copy.normals[i].x, copy.normals[i].y, copy.normals[i].z);
                info.pos = vertices[i];
                info.velocity = float3.zero;
                info.velocity_curvature = float3.zero;
                info.velocity_gravity = float3.zero;
                info.velocity_volume = float3.zero;
                hits[i] = new RaycastHit();
                hitCloseNormals[i] = float3.zero;
                hitClosePoints[i] = float3.zero;
            }
            // === 6개 최근접 이웃 찾기 ===
            for (int i = 0; i < vertices.Length; i++)
            {
                list.Clear();
                float3 pi = vertices[i];
                for (int j = 0; j < vertices.Length; j++)
                {
                    if (i == j) continue;
                    float d = math.length(vertices[j] - pi);
                    list.Add((j, d));
                }
                list.Sort((a, b) => a.dist.CompareTo(b.dist));
                LocalInfo info = localInfos[i];
                info.neighborCount = math.min(6, list.Count);
                if (info.neighborCount > 0) { info.n0 = list[0].index; info.r0 = list[0].dist; }
                if (info.neighborCount > 1) { info.n1 = list[1].index; info.r1 = list[1].dist; }
                if (info.neighborCount > 2) { info.n2 = list[2].index; info.r2 = list[2].dist; }
                if (info.neighborCount > 3) { info.n3 = list[3].index; info.r3 = list[3].dist; }
                if (info.neighborCount > 4) { info.n4 = list[4].index; info.r4 = list[4].dist; }
                if (info.neighborCount > 5) { info.n5 = list[5].index; info.r5 = list[5].dist; }
                localInfos[i] = info;
            }
        }
        else
        {
            copy = Instantiate(original);
        }
        mf.mesh = copy;
        for (int i = 0; i < vertices.Length; i++)
        {
            LocalInfo info = localInfos[i];
            vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
            velocites[i] = float3.zero;
            info.normal = new float3(copy.normals[i].x, copy.normals[i].y, copy.normals[i].z);
            info.pos = vertices[i];
            info.velocity = float3.zero;
            info.velocity_curvature = float3.zero;
            info.velocity_gravity = float3.zero;
            info.velocity_volume = float3.zero;
            hits[i] = new RaycastHit();
            hitCloseNormals[i] = float3.zero;
            hitClosePoints[i] = float3.zero;
        }
        verticesToArray = new Vector3[vertices.Length];
        normalsToArray = new Vector3[vertices.Length];
        InitEntity();
    }
}
/////////////////
// ISystem or SystemBase
/////////////////
[RequireMatchingQueriesForUpdate]
public partial class KJHLiquidSystem : SystemBase
{
    EntityCommandBufferSystem ecbSystem;
    LayerMask layerMask;
    float elapsed;
    JobHandle jobHandle1;
    JobHandle jobHandle2;
    JobHandle jobHandle3;
    JobHandle jobHandle4;
    JobHandle jobHandle5;
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        layerMask = ~(LayerMask.GetMask("Water") | 1 << 2);
    }
    protected override void OnDestroy()
    {
        jobHandle1.Complete();
        jobHandle2.Complete();
        jobHandle3.Complete();
        jobHandle4.Complete();
        jobHandle5.Complete();
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
                localInfos = mono.localInfos,
                rayComms = mono.rayComms,
                layerMask = layerMask,
            };
            jobHandle1 = job1.Schedule(mono.rayComms.Length, 64, this.Dependency);
            this.Dependency = jobHandle1;
            jobHandle1.Complete();
            #endregion
            #region Raycast Command Result Job
            // Raycast Command 실행 및 결과 변환
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
            // if (math.length(mono.hitCloseNormals[0]) > 0.01f)
            // {
            //     Vector3 pos = pivot + new Vector3(tr.localScale.x * mono.vertices[0].x,
            //     tr.localScale.y * mono.vertices[0].y, tr.localScale.z * mono.vertices[0].z);
            //     float distance = math.length(mono.hitClosePoints[0] - new float3(pos.x, pos.y, pos.z));
            //     if (distance < 0.13f)
            //     {
            //         Debug.DrawLine(pos, mono.hitClosePoints[0], Color.red, 0.1f, true);
            //     }
            //     else if (distance < 0.25f)
            //     {
            //         Debug.DrawLine(pos, mono.hitClosePoints[0], Color.yellow, 0.1f, true);
            //     }
            //     else
            //     {
            //         Debug.DrawLine(pos, mono.hitClosePoints[0], Color.gray, 0.1f, true);
            //     }
            // }
            //Debug.Log(math.length(mono.localInfos[0].velocity_volume));
#endif
            #endregion
            #region Move Job
            elapsed = (float)SystemAPI.Time.ElapsedTime - elapsed;
            elapsed += SystemAPI.Time.DeltaTime;
            var job = new KJHLiquidMoveJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
                elapsed = elapsed,
                gravityForce = new float3(0, -3.81f, 0),
                vertices = mono.vertices,
                velocites = mono.velocites,
                localInfos = mono.localInfos,
                hitCloseNormals = mono.hitCloseNormals,
                hitClosePoints = mono.hitClosePoints,
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
/////////////////
// Job
/////////////////
[BurstCompile]
public partial struct KJHLiquidRayCommJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public float3 scale;
    [ReadOnly] public NativeArray<float3> vertices;
    [ReadOnly] public NativeArray<KJHLiquid.LocalInfo> localInfos;
    [WriteOnly] public NativeArray<RaycastCommand> rayComms;
    [ReadOnly] public LayerMask layerMask;
    public void Execute(int index)
    {
        float3 direction;
        direction = localInfos[index].normal;
        if (math.length(localInfos[index].velocity) >= 0.01f)
            direction = math.normalize(localInfos[index].velocity);
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
    public float elapsed;
    public float3 gravityForce;
    [ReadOnly] public NativeArray<float3> hitClosePoints;
    [ReadOnly] public NativeArray<float3> hitCloseNormals;
    [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeArray<float3> velocites;
    [NativeDisableParallelForRestriction] public NativeArray<KJHLiquid.LocalInfo> localInfos;
    public float springK;
    public float springDamping;
    public float contactThreshold;

    public void Execute(int index)
    {
        float3 currentPosition = vertices[index];
        KJHLiquid.LocalInfo info = localInfos[index];
        float3 currentVelocity = info.velocity;
        float3 currentVelocity_gravity = info.velocity_gravity;
        float3 currentVelocity_volume = info.velocity_volume;

        #region 중력 & 종단속력
        currentVelocity_gravity += gravityForce * elapsed;
        currentVelocity_gravity = math.clamp(currentVelocity_gravity, -5f, 0f);
        #endregion

        #region 스프링힘
        
        #endregion

        // 속도 합산
        currentVelocity = currentVelocity_gravity + currentVelocity_volume;


        #region 어딘가에 충돌된 경우
        if (math.length(hitClosePoints[index]) > 0.01f)
        {
            float3 pos = pivot + scale * vertices[index];
            float distance = math.length(hitClosePoints[index] - pos);
            if (distance < 0.05f)
            {
                // 충돌해서 외부 벽면에 달라붙은 물방울의 점은(물묻은 점) 
                // 벽에 묻었기 떄문에 이점에 대해서는 훅의법칙이고 뭐고 이동이고 뭐고 없음
                currentVelocity_gravity = math.lerp(currentVelocity_gravity, float3.zero, 30f * elapsed);
                currentVelocity_volume = math.lerp(currentVelocity_volume, float3.zero, 30f * elapsed);
                currentVelocity = math.lerp(currentVelocity, float3.zero, 30f * elapsed);
                // 위치 보정
                float fixPos = math.length(currentVelocity);
                currentPosition = math.lerp(currentPosition, ((hitClosePoints[index] - pivot) / scale) + 0.1f * hitCloseNormals[index], (3f + fixPos) * elapsed);
            }
        }
        #endregion

        // 위치 업데이트 (로컬)
        currentPosition += currentVelocity * elapsed;

        // 결과 저장
        info.velocity_gravity = currentVelocity_gravity;
        info.velocity_volume = currentVelocity_volume;
        info.velocity = currentVelocity;
        velocites[index] = currentVelocity;
        info.pos = currentPosition;
        vertices[index] = currentPosition;
        localInfos[index] = info;
    }
}