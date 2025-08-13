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
    [HideInInspector] public NativeArray<float3> gravityVelocites;
    [HideInInspector] public NativeArray<RaycastCommand> rayComms;
    [HideInInspector] public NativeArray<RaycastHit> hits;
    [HideInInspector] public NativeArray<LocalInfo> infos;
    [HideInInspector] public NativeArray<float3> hitCloseNormals;
    [HideInInspector] public NativeArray<float3> hitClosePoints;
    [HideInInspector] public Vector3[] verticesToArray;
    [HideInInspector] public Vector3[] normalsToArray;
    public struct LocalInfo
    {
        public float3 pos;
        public float3 normal;
        public float3 velocity;
        public float localCurvature;
        public int neighborA;
        public int neighborB;
        public float restLenA;
        public float restLenB;
    }
    // public static float Area(float3 a, float3 b, float3 c)
    // {
    //     float3 vec1 = b - a;
    //     float3 vec2 = c - a;
    //     float3 crossProduct = math.cross(vec1, vec2);
    //     float area = 0.5f * math.length(crossProduct);
    //     return area;
    // }
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
    void OnDestroy() => UnInit();
    void UnInit()
    {
        if (vertices.IsCreated) vertices.Dispose();
        if (velocites.IsCreated) velocites.Dispose();
        if (velocites.IsCreated) gravityVelocites.Dispose();
        if (rayComms.IsCreated) rayComms.Dispose();
        if (hits.IsCreated) hits.Dispose();
        if (infos.IsCreated) infos.Dispose();
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
            mf.mesh = copy;
            vertices = new NativeArray<float3>(copy.vertices.Length, Allocator.TempJob);
            velocites = new NativeArray<float3>(copy.vertices.Length, Allocator.TempJob);
            gravityVelocites = new NativeArray<float3>(copy.vertices.Length, Allocator.TempJob);
            rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.TempJob);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.TempJob);
            infos = new NativeArray<LocalInfo>(copy.vertices.Length, Allocator.TempJob);
            hitCloseNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.TempJob);
            hitClosePoints = new NativeArray<float3>(copy.vertices.Length, Allocator.TempJob);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
                velocites[i] = float3.zero;
            }
            verticesToArray = new Vector3[vertices.Length];
            normalsToArray = new Vector3[vertices.Length];
        }
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
        if (jobHandle1.IsCompleted) jobHandle1.Complete();
        if (jobHandle2.IsCompleted) jobHandle2.Complete();
        if (jobHandle3.IsCompleted) jobHandle3.Complete();
        if (jobHandle4.IsCompleted) jobHandle4.Complete();
        if (jobHandle5.IsCompleted) jobHandle5.Complete();
    }
    protected override void OnUpdate()
    {
        Entities.ForEach((KJHLiquid mono) =>
        {
            elapsed = (float)SystemAPI.Time.ElapsedTime;
            Transform tr = mono.transform;
            Vector3 pivot = tr.position;
            // Raycast Command Job 스케줄링
            var job1 = new KJHLiquidRayCommJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
                vertices = mono.vertices,
                velocites = mono.velocites,
                rayComms = mono.rayComms,
                layerMask = layerMask,
            };
            jobHandle1 = job1.Schedule(mono.rayComms.Length, 64, this.Dependency);
            this.Dependency = jobHandle1;
            jobHandle1.Complete();
            // Raycast Command 실행 및 결과 변환
            jobHandle2 = RaycastCommand.ScheduleBatch(mono.rayComms, mono.hits, 1, this.Dependency);
            this.Dependency = jobHandle2;
            jobHandle2.Complete();
            for (int i = 0; i < mono.hits.Length; i++)
            {
                if (mono.hits[i].collider != null && mono.hits[i].distance <= 0.08f && mono.hits[i].distance > 0f)
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
            // Move Job
            elapsed = (float)SystemAPI.Time.ElapsedTime - elapsed;
            elapsed += SystemAPI.Time.DeltaTime;
            var job = new KJHLiquidMoveJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
                elapsed = elapsed,
                gravity = new float3(0, -3.81f, 0),
                vertices = mono.vertices,
                velocites = mono.velocites,
                gravityVelocites = mono.gravityVelocites,
                infos = mono.infos,
                hitCloseNormals = mono.hitCloseNormals,
                hitClosePoints = mono.hitClosePoints,
            };
            jobHandle3 = job.Schedule(mono.vertices.Length, 64, this.Dependency);
            this.Dependency = jobHandle3;
            jobHandle3.Complete();

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
    [ReadOnly] public NativeArray<float3> velocites;
    [WriteOnly] public NativeArray<RaycastCommand> rayComms;
    [ReadOnly] public LayerMask layerMask;
    public void Execute(int index)
    {
        float3 direction;
        if (math.length(velocites[index]) == 0)
        {
            direction = math.down();
        }
        else
        {
            direction = math.normalize(velocites[index]);
        }
        QueryParameters queryParameters = new QueryParameters();
        queryParameters.layerMask = layerMask;
        queryParameters.hitTriggers = QueryTriggerInteraction.Ignore;
        queryParameters.hitBackfaces = false;
        queryParameters.hitMultipleFaces = false;
        rayComms[index] = new RaycastCommand(pivot + scale * vertices[index], direction, queryParameters, 100f);
    }
}
[BurstCompile]
public partial struct KJHLiquidMoveJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public float3 scale;
    public float elapsed;
    public float3 gravity;
    [ReadOnly] public NativeArray<float3> hitClosePoints;
    [ReadOnly] public NativeArray<float3> hitCloseNormals;
    [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeArray<float3> velocites;
    [NativeDisableParallelForRestriction] public NativeArray<float3> gravityVelocites;
    [NativeDisableParallelForRestriction] public NativeArray<KJHLiquid.LocalInfo> infos;
    public void Execute(int index)
    {
        float3 currentPosition = vertices[index];
        float3 currentVelocity = velocites[index];





        // ////////////////////////////
        float3 gravityVelocity = gravityVelocites[index];
        gravityVelocity += gravity * elapsed;
        gravityVelocity = math.clamp(gravityVelocity, -4f, 0f);
        if (math.length(hitCloseNormals[index]) > 0.1f)
        {
            float3 penetrateDir = -hitCloseNormals[index];
            float dot = math.dot(gravityVelocity, penetrateDir);
            if (dot > 0f)
            {
                gravityVelocity = 0f;
                currentVelocity = 0f;
                // currentVelocity -= dot * penetrateDir;
                currentPosition = math.lerp(currentPosition, ((hitClosePoints[index] - pivot) / scale) + 0.1f * -penetrateDir, 4.5f * elapsed);
            }
        }
        currentVelocity += gravityVelocity;
        // ////////////////////////////











        // 5. 최종 위치를 업데이트합니다.
        currentPosition += currentVelocity * elapsed;

        // 6. 결과 반영
        gravityVelocites[index] = gravityVelocity;
        vertices[index] = currentPosition;
        velocites[index] = currentVelocity;
    }
}

