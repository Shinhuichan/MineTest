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
public struct KJHLiquidTag : IComponentData { }
// --- MonoBehaviour ---
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class KJHLiquid : PoolBehaviour
{
    public LayerMask collisionMask;
    public float gravityForce = 3f;
    public float neighborSpring = 50f;
    public float neighborDamping = 0.5f;
    public float farSpring = 50f;
    public float farDamping = 0.5f;
    public float volumeSpringK = 5f;
    public float maxSpeed;
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;
    Mesh original;
    [HideInInspector] public Mesh copy;
    #region Entity Setting
    EntityManager entityManager;
    Entity entity;
    [HideInInspector] public NativeArray<float3> vertices;
    [HideInInspector] public NativeArray<VertInfo> infos;
    [HideInInspector] public NativeArray<RaycastCommand> rayComms;
    [HideInInspector] public NativeArray<RaycastHit> hits;
    uint seed;
    public float3 initScale;
    public float initVolume;
    Vector3[] verticesToArray;
    [HideInInspector] public Transform attachTarget;
    [HideInInspector] public Vector3 initTargetPos;
    [HideInInspector] public Vector3 initTrPos;
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
        public FixedList64Bytes<int> neighborIndex;
        public FixedList64Bytes<float> neighborInitDistance;
        // 1. 가장 멀리있는 대척점 버텍스
        // 2. 
        public FixedList64Bytes<int> farIndex;
        public FixedList64Bytes<float>  farInitDistance;
    }
    void InitEntity()
    {
        if (entityManager.Exists(entity)) entityManager.DestroyEntity(entity);
        entity = entityManager.CreateEntity(typeof(KJHLiquidTag));
        entityManager.AddComponentObject(entity, this);
    }
    #endregion
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
    public void Init()
    {
        if (original == null)
        {
            original = mf.mesh;
            copy = Instantiate(original);
            mf.mesh = copy;
            vertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            infos = new NativeArray<VertInfo>(copy.vertices.Length, Allocator.Persistent);
            rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
            // 가장 가까운 6개 찾기
            for (int i = 0; i < copy.vertices.Length; i++)
            {
                // 각 루프마다 리스트와 해시셋 초기화
                List<(float dist, int index)> nearestList = new List<(float, int)>();
                HashSet<Vector3> addedPositions = new HashSet<Vector3>();
                VertInfo info = new VertInfo();
                info.neighborIndex = new FixedList64Bytes<int>();
                info.neighborInitDistance = new FixedList64Bytes<float>();
                for (int j = 0; j < copy.vertices.Length; j++)
                {
                    // 현재 정점은 건너뛰기
                    if (i == j) continue;
                    if (copy.vertices[i] == copy.vertices[j]) continue;
                    // 이미 추가된 위치의 정점은 건너뛰기
                    if (addedPositions.Contains(copy.vertices[j]))
                    {
                        continue;
                    }

                    float dist = Vector3.Distance(copy.vertices[i], copy.vertices[j]);
                    nearestList.Add((dist, j));
                    addedPositions.Add(copy.vertices[j]);
                }
                nearestList.Sort((a, b) => a.dist.CompareTo(b.dist));
                int neighborCount = math.min(6, nearestList.Count);
                for (int n = 0; n < neighborCount; n++)
                {
                    info.neighborIndex.Add(nearestList[n].index);
                    info.neighborInitDistance.Add(nearestList[n].dist);
                }
                infos[i] = info;
            }
        }
        else
        {
            copy = Instantiate(original);
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
        initVolume = CalculateMeshVolume(copy);
        seed = (uint)Random.Range(0, 10000);
        initScale = transform.lossyScale;
        attachTarget = null;
        initTargetPos = Vector3.zero;
        initTrPos = transform.position;
        InitEntity();
    }
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
        mc.sharedMesh = null;
        mc.sharedMesh = copy;
        for (int i = 0; i < copy.vertices.Length; i++)
        {
            VertInfo info = infos[i];
            vertices[i] = copy.vertices[i];
            info.vertex = copy.vertices[i];
            info.normal = copy.normals[i];
            infos[i] = info;
        }
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
// --- ISystem or SystemBase ---
[RequireMatchingQueriesForUpdate]
public partial class KJHLiquidSystem : SystemBase
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
        Entities.ForEach((KJHLiquid mono) =>
        {
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
                KJHLiquid.VertInfo info = mono.infos[i];
                if (mono.hits[i].collider != null && mono.hits[i].distance <= 1f && mono.hits[i].distance > 0f)
                {
                    info.hitNormal = new float3(mono.hits[i].normal.x, mono.hits[i].normal.y, mono.hits[i].normal.z);
                    info.hitPoint = new float3(mono.hits[i].point.x, mono.hits[i].point.y, mono.hits[i].point.z);
                }
                else
                {
                    info.hitNormal = float3.zero;
                    info.hitPoint = float3.zero;
                }
                mono.infos[i] = info;
            }
#if UNITY_EDITOR
            if (math.length(mono.infos[0].hitNormal) > 0.01f)
            {
                Vector3 pos = pivot + new Vector3(tr.lossyScale.x * mono.infos[0].vertex.x,
                tr.lossyScale.y * mono.infos[0].vertex.y, tr.lossyScale.z * mono.infos[0].vertex.z);
                float distance = math.length(mono.infos[0].hitPoint - new float3(pos.x, pos.y, pos.z));
                if (distance < 0.13f)
                {
                    Debug.DrawLine(pos, mono.infos[0].hitPoint, Color.red, 0.1f, true);
                }
                else if (distance < 0.25f)
                {
                    Debug.DrawLine(pos, mono.infos[0].hitPoint, Color.yellow, 0.1f, true);
                }
                else
                {
                    Debug.DrawLine(pos, mono.infos[0].hitPoint, Color.gray, 0.1f, true);
                }
            }

#endif
            #endregion
            #region Move Job
            float currVolume = mono.CalculateMeshVolume(mono.copy);
            //Debug.Log($"{mono.initVolume},{currVolume},{math.clamp(mono.initVolume / currVolume, 0.1f, 10f)}");
            deltaTime = SystemAPI.Time.DeltaTime;
            var job = new KJHLiquidMoveJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.lossyScale.x, tr.lossyScale.y, tr.lossyScale.z),
                deltaTime = deltaTime,
                infos = mono.infos,
                gravityForce = mono.gravityForce,
                volumeSpringK = mono.volumeSpringK,
                neighborSpring = mono.neighborSpring,
                neighborDamping = mono.neighborDamping,
                maxSpeed = mono.maxSpeed,
                initVolume = mono.initVolume,
                currVolume = currVolume,
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
public partial struct KJHLiquidRayCommJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public float3 scale;
    [ReadOnly] public NativeArray<KJHLiquid.VertInfo> infos;
    [WriteOnly] public NativeArray<RaycastCommand> rayComms;
    [ReadOnly] public LayerMask layerMask;
    public void Execute(int index)
    {
        float3 direction = infos[index].velocity;
        if (math.length(direction) < 0.0001f)
            direction = infos[index].normal;
        if (math.length(direction) < 0.0001f)
            direction = math.down();
        direction = math.normalize(direction);
        QueryParameters queryParameters = new QueryParameters();
        queryParameters.layerMask = layerMask;
        queryParameters.hitTriggers = QueryTriggerInteraction.Ignore;
        queryParameters.hitBackfaces = false;
        queryParameters.hitMultipleFaces = false;
        rayComms[index] = new RaycastCommand(pivot + scale * infos[index].vertex, direction, queryParameters, 10f);
    }
}
// Job
[BurstCompile]
public partial struct KJHLiquidMoveJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public float3 scale;
    [ReadOnly] public float deltaTime;
    [ReadOnly] public uint seed;
    [ReadOnly] public float gravityForce;
    [ReadOnly] public float volumeSpringK;
    [ReadOnly] public float neighborSpring;
    [ReadOnly] public float neighborDamping;
    [ReadOnly] public float maxSpeed;
    [ReadOnly] public float initVolume;
    [ReadOnly] public float currVolume;
    [NativeDisableParallelForRestriction] public NativeArray<KJHLiquid.VertInfo> infos;
    public void Execute(int index)
    {
        // 불러오기
        KJHLiquid.VertInfo info = infos[index];
        if (info.isAttach == 2) return;
        uint seed = this.seed + (uint)index;

        // 중력
        if (info.isAttach == 0)
        {
            info.velocity_gravity += gravityForce * math.down() * deltaTime;
            info.velocity_gravity = ClampMagnitude(info.velocity_gravity, maxSpeed);
        }

        // 이웃 버텍스 스프링 --> 메쉬 표면 찢어짐 방지
        float3 neighborForce = float3.zero;
        for (int n = 0; n < info.neighborIndex.Length; n++)
        {
            int nbIndex = info.neighborIndex[n];
            var nbInfo = infos[nbIndex];
            //
            float distance = math.length(info.vertex - nbInfo.vertex);
            float diff = distance - info.neighborInitDistance[n];
            if (diff <= 0.001f) diff = 0f;
            float3 dir = nbInfo.vertex - info.vertex;
            dir = math.normalize(dir);
            float d = math.length(info.velocity_edge);
            neighborForce += (0.1666f * neighborSpring * diff) * dir;
            neighborForce += -info.velocity_edge * neighborDamping;
            info.velocity_edge += neighborForce * deltaTime;
        }

        // 대척점 버텍스 스프링 --> 부피 볼륨 유지 스프링
        

        // 진행하다가 전방에 충돌된 경우 (완전 비탄성 --> 벽에 달라붙어서 속도 0 --> 충돌 포인트에서 더이상 이동 하지않음)
        if (math.length(info.hitNormal) > 0.01f || info.isAttach >= 1)
        {
            float3 pos = pivot + scale * info.vertex;
            float distance = math.length(info.hitPoint - pos);
            if (distance < 0.01f)
            {
                info.isAttach = 2;
                info.velocity_gravity = float3.zero;
                info.velocity_edge = float3.zero;
                info.velocity_volume = float3.zero;
                info.velocity = float3.zero;
                info.vertex = info.attachVertex;
            }
            else if (distance < 0.05f)
            {
                info.isAttach = 1;
                info.attachVertex = 0.5f * (info.vertex + ((info.hitPoint - pivot) / scale) + 0.07f * info.hitNormal);
                info.velocity_gravity = math.lerp(info.velocity_gravity, float3.zero, 30f * deltaTime);
                info.velocity_edge = math.lerp(info.velocity_edge, float3.zero, 30f * deltaTime);
                info.velocity_volume = math.lerp(info.velocity_volume, float3.zero, 30f * deltaTime);
                info.velocity = math.lerp(info.velocity, float3.zero, 30f * deltaTime);
                // 위치 서서히 히트 포인트에 붇는 시각적 효과
                float veloLeng = math.length(info.velocity);
                info.vertex = math.lerp(info.vertex, ((info.hitPoint - pivot) / scale) + 0.07f * info.hitNormal, 0.05f * (3f + veloLeng) * deltaTime);
            }
        }

        // 최종 속도 적용
        info.velocity = info.velocity_gravity + info.velocity_edge;
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
















