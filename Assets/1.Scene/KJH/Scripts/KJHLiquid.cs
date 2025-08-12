// using UnityEngine;
// using Unity.Mathematics;
// using Unity.Burst;
// using Unity.Jobs;
// using Unity.Entities;
// using Unity.Collections;
// using Unity.Transforms;
// using Random = UnityEngine.Random;
// /////////////////
// // IComponentData
// /////////////////
// public struct KJHLiquidTag : IComponentData { }
// /////////////////
// // MonoBehaviour
// /////////////////
// [RequireComponent(typeof(MeshFilter))]
// [RequireComponent(typeof(MeshRenderer))]
// [RequireComponent(typeof(MeshCollider))]
// public class KJHLiquid : PoolBehaviour
// {
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
//     MeshFilter mf;
//     MeshRenderer mr;
//     MeshCollider mc;
//     Mesh original;
//     [HideInInspector] public Mesh copy;
//     [HideInInspector] public NativeArray<float3> vertices;
//     [HideInInspector] public NativeArray<float3> velocites;
//     [HideInInspector] public NativeArray<float3> normals;
//     [HideInInspector] public NativeArray<float3> gravityVelocites;
//     [HideInInspector] public NativeArray<float3> volumeVelocites;
//     [HideInInspector] public NativeArray<float3> curvatureVelocites;
//     [HideInInspector] public NativeArray<LocalInfo> infos;
//     public float globalCurvature = -999f;
//     public float initMeanDensity = -999f;
//     public struct LocalInfo
//     {
//         // 나 자신 버텍스의 정보
//         public float3 pos0;
//         public float3 normal0;
//         public float3 velocity0;
//         // 삼각형 △에서 왼쪽 꼭지점이 나 자신으로 간주하고
//         // 삼각형 오른쪽 꼭지점에 있는 버텍스를 pos1 로 지정
//         public float3 pos1;
//         public float3 normal1;
//         public float3 velocity1;
//         // 삼각형 △에서 왼쪽 꼭지점이 나 자신으로 간주하고
//         // 삼각형 위쪽 꼭지점에 있는 버텍스를 pos2 로 지정
//         public float3 pos2;
//         public float3 normal2;
//         public float3 velocity2;
//         public float localCurvature;
//         // 삼각형 면적 공식으로 근사시킬수 있을듯. 넓이가 넓으면 밀도가 작고, 좁으면 밀도가 크고
//         public float localDensity;
//     }
//     [HideInInspector] public NativeArray<RaycastCommand> rayComms;
//     [HideInInspector] public NativeArray<RaycastHit> hits;
//     [HideInInspector] public NativeArray<float3> hitCloseNormals;
//     [HideInInspector] public NativeArray<float3> hitClosePoints;
//     [HideInInspector] public Vector3[] verticesToArray;
//     [HideInInspector] public Vector3[] normalsToArray;
//     [ReadOnlyInspector] public Transform attachTarget;
//     [HideInInspector] public Vector3 attachTargetInitPosition;
//     [HideInInspector] public Vector3 initPos;
//     [ReadOnlyInspector] public float initVolume = -1f;
//     void Awake()
//     {
//         entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
//         TryGetComponent(out mf);
//         TryGetComponent(out mr);
//         TryGetComponent(out mc);
//     }
//     void OnDisable() => UnInit();
//     void OnDestroy() => UnInit();
//     void UnInit()
//     {
//         attachTarget = null;
//         initPos = Vector3.zero;
//         initVolume = -1f;
//         if (vertices.IsCreated) vertices.Dispose();
//         if (velocites.IsCreated) velocites.Dispose();
//         if (normals.IsCreated) normals.Dispose();
//         if (velocites.IsCreated) gravityVelocites.Dispose();
//         if (rayComms.IsCreated) rayComms.Dispose();
//         if (hits.IsCreated) hits.Dispose();
//         if (hitCloseNormals.IsCreated) hitCloseNormals.Dispose();
//         if (hitClosePoints.IsCreated) hitClosePoints.Dispose();
//         if (volumeVelocites.IsCreated) volumeVelocites.Dispose();
//         if (curvatureVelocites.IsCreated) curvatureVelocites.Dispose();
//         if (infos.IsCreated) infos.Dispose();
//     }
//     int reDrawCount;
//     public void Draw()
//     {
//         for (int i = 0; i < vertices.Length; i++)
//         {
//             verticesToArray[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z);
//         }
//         copy.vertices = verticesToArray;
//         if (attachTarget != null)
//         {
//             transform.position = initPos + attachTarget.position - attachTargetInitPosition;
//         }
//         if (reDrawCount > 15)
//         {
//             copy.RecalculateNormals();
//             copy.RecalculateBounds();
//             mc.sharedMesh = null;
//             mc.sharedMesh = copy;
//             reDrawCount = 0;
//         }
//         else
//         {
//             reDrawCount++;
//         }
//     }
//     void OnEnable()
//     {
//         Init();
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
//             gravityVelocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
//             hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
//             hitCloseNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             hitClosePoints = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             initVolume = 0.5236f * mc.bounds.size.x * mc.bounds.size.y * mc.bounds.size.z;
//             volumeVelocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             curvatureVelocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
//             infos = new NativeArray<LocalInfo>(copy.vertices.Length, Allocator.Persistent);
//             for (int i = 0; i < vertices.Length; i++)
//             {
//                 LocalInfo info = infos[i];
//                 vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
//                 velocites[i] = float3.zero;
//                 normals[i] = new float3(copy.normals[i].x, copy.normals[i].y, copy.normals[i].z);
//                 info.pos0 = vertices[i];
//                 info.normal0 = normals[i];
//                 info.velocity0 = velocites[i];
//                 infos[i] = info;
//             }
//             verticesToArray = new Vector3[vertices.Length];
//             normalsToArray = new Vector3[vertices.Length];

//             // initMeanDensity은 맨처음 한번 계산

//             #region LocalInfo 담기

//             #endregion


//         }
//         InitEntity();
//     }
// }
// /////////////////
// // ISystem or SystemBase
// /////////////////
// [RequireMatchingQueriesForUpdate]
// public partial class KJHLiquidSystem : SystemBase
// {
//     EntityCommandBufferSystem ecbSystem;
//     float elapsed;
//     protected override void OnCreate()
//     {
//         ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
//         layerMask = ~(LayerMask.GetMask("Water") | 1 << 2);
//     }
//     LayerMask layerMask;
//     JobHandle jobHandle1;
//     JobHandle jobHandle2;
//     JobHandle jobHandle3;
//     JobHandle jobHandle4;
//     JobHandle jobHandle5;
//     protected override void OnDestroy()
//     {
//         if (jobHandle1.IsCompleted) jobHandle1.Complete();
//         if (jobHandle2.IsCompleted) jobHandle2.Complete();
//         if (jobHandle3.IsCompleted) jobHandle3.Complete();
//         if (jobHandle4.IsCompleted) jobHandle4.Complete();
//         if (jobHandle5.IsCompleted) jobHandle5.Complete();
//     }
//     protected override void OnUpdate()
//     {
//         Entities.ForEach((KJHLiquid mono) =>
//         {
//             elapsed = (float)SystemAPI.Time.ElapsedTime;
//             Transform tr = mono.transform;
//             if (mono.initPos == Vector3.zero) mono.initPos = mono.transform.position;
//             Vector3 pivot;
//             Vector3 displacement = Vector3.zero;
//             if (mono.attachTarget != null)
//                 displacement = mono.attachTarget.position - mono.attachTargetInitPosition;
//             pivot = tr.position + displacement;

//             #region RaycastCommand Start Job
//             var job1 = new KJHLiquidRayCommJob
//             {
//                 pivot = new float3(pivot.x, pivot.y, pivot.z),
//                 scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
//                 rotation = new quaternion(tr.rotation.x, tr.rotation.y, tr.rotation.z, tr.rotation.w),
//                 vertices = mono.vertices,
//                 velocites = mono.velocites,
//                 normals = mono.normals,
//                 rayComms = mono.rayComms,
//                 layerMask = layerMask,
//             };
//             jobHandle1 = job1.Schedule(mono.rayComms.Length, 64, this.Dependency);
//             this.Dependency = jobHandle1;
//             jobHandle1.Complete();
//             #endregion
//             #region RaycastCommand Result Job
//             jobHandle2 = RaycastCommand.ScheduleBatch(mono.rayComms, mono.hits, 1, this.Dependency);
//             this.Dependency = jobHandle2;
//             jobHandle2.Complete();
//             for (int i = 0; i < mono.hits.Length; i++)
//             {
//                 if (mono.hits[i].collider != null && mono.hits[i].distance > 0f)
//                 {
//                     mono.hitClosePoints[i] = new float3(mono.hits[i].point.x, mono.hits[i].point.y, mono.hits[i].point.z);
//                     mono.hitCloseNormals[i] = new float3(mono.hits[i].normal.x, mono.hits[i].normal.y, mono.hits[i].normal.z);
//                     if (mono.attachTarget == null)
//                     {
//                         mono.attachTarget = mono.hits[i].collider.transform;
//                         mono.attachTargetInitPosition = mono.hits[i].collider.transform.position;
//                     }
//                 }
//             }
//             #endregion

//             if (math.length(mono.hitClosePoints[0]) > 0.01f)
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

//             #region 현재 프레임에서 Global Curvature를 계산하기
//             // jobHandle4

//             #endregion



//             #region Move Job
//             elapsed = (float)SystemAPI.Time.ElapsedTime - elapsed;
//             elapsed += SystemAPI.Time.DeltaTime;
//             var job = new KJHLiquidMoveJob
//             {
//                 pivot = new float3(pivot.x, pivot.y, pivot.z),
//                 scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
//                 rotation = new quaternion(tr.rotation.x, tr.rotation.y, tr.rotation.z, tr.rotation.w),
//                 elapsed = elapsed,
//                 gravity = new float3(0, -3.81f, 0),
//                 vertices = mono.vertices,
//                 velocites = mono.velocites,
//                 normals = mono.normals,
//                 gravityVelocites = mono.gravityVelocites,
//                 hitCloseNormals = mono.hitCloseNormals,
//                 hitClosePoints = mono.hitClosePoints,
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
// /////////////////
// // Job
// /////////////////
// [BurstCompile]
// public partial struct KJHLiquidRayCommJob : IJobParallelFor
// {
//     [ReadOnly] public float3 pivot;
//     [ReadOnly] public float3 scale;
//     [ReadOnly] public quaternion rotation;
//     [ReadOnly] public NativeArray<float3> vertices;
//     [ReadOnly] public NativeArray<float3> velocites;
//     [ReadOnly] public NativeArray<float3> normals;
//     [WriteOnly] public NativeArray<RaycastCommand> rayComms;
//     [ReadOnly] public LayerMask layerMask;
//     public void Execute(int index)
//     {
//         float3 direction;

//         direction = normals[index];
//         if (math.length(velocites[index]) >= 0.01f)
//             direction = math.normalize(velocites[index]);

//         QueryParameters queryParameters = new QueryParameters();
//         queryParameters.layerMask = layerMask;
//         queryParameters.hitTriggers = QueryTriggerInteraction.Ignore;
//         queryParameters.hitBackfaces = false;
//         queryParameters.hitMultipleFaces = false;
//         rayComms[index] = new RaycastCommand(pivot + scale * vertices[index], direction, queryParameters, 100f);
//     }
// }
// [BurstCompile]
// public partial struct KJHLiquidMoveJob : IJobParallelFor
// {
//     [ReadOnly] public float3 pivot;
//     [ReadOnly] public float3 scale;
//     [ReadOnly] public quaternion rotation;
//     public float elapsed;
//     public float3 gravity;
//     [ReadOnly] public NativeArray<float3> hitClosePoints;
//     [ReadOnly] public NativeArray<float3> hitCloseNormals;
//     [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
//     [NativeDisableParallelForRestriction] public NativeArray<float3> velocites;
//     [ReadOnly] public NativeArray<float3> normals;
//     [NativeDisableParallelForRestriction] public NativeArray<float3> gravityVelocites;
//     [NativeDisableParallelForRestriction] public NativeArray<float3> volumeVelocites;
//     [NativeDisableParallelForRestriction] public NativeArray<float3> curvatureVelocites;
//     public void Execute(int index)
//     {
//         // 정보 가져오기
//         float3 currentPosition = vertices[index];
//         float3 currentVelocity = velocites[index];
//         float3 gravityVelocity = gravityVelocites[index];
//         float3 volumeVelocity = volumeVelocites[index];
//         float3 curvatureVelocity = curvatureVelocites[index];


//         #region 중력
//         gravityVelocity += gravity * elapsed;
//         // 종단속도
//         gravityVelocity = math.clamp(gravityVelocity, -4f, 0f);
//         #endregion
//         #region 밀도 비교.. 밀도의 그래디언트 방향으로 힘을 받게

//         #endregion
//         #region 오브젝트 전체의 평균곡률과 내 주변 곡률을 비교 (?)

//         #endregion
//         // 가속이 끝난 각각의 속도들을 합산.
//         currentVelocity = gravityVelocity;

//         #region 어딘가에 충돌된 경우
//         if (math.length(hitClosePoints[index]) > 0.1f)
//         {
//             float3 pos = pivot + scale * vertices[index];
//             float distance = math.length(hitClosePoints[index] - pos);
//             if (distance < 0.13f)
//             {
//                 // 충돌해서 외부 벽면에 달라붙은 물방울의 점은(물묻은 점) 
//                 // 벽에 묻었기 떄문에 이점에 대해서는 훅의법칙이고 뭐고 이동이고 뭐고 없음
//                 gravityVelocity = math.lerp(gravityVelocity, float3.zero, 30f * elapsed);
//                 currentVelocity = math.lerp(currentVelocity, float3.zero, 30f * elapsed);
//                 // 위치 보정
//                 float fixPos = math.length(currentVelocity);
//                 currentPosition = math.lerp(currentPosition, ((hitClosePoints[index] - pivot) / scale) + 0.1f * hitCloseNormals[index], (3f + fixPos) * elapsed);
//             }
//         }
//         #endregion

//         // 최종 위치를 업데이트합니다.
//         currentPosition += currentVelocity * elapsed;
//         gravityVelocites[index] = gravityVelocity;
//         volumeVelocites[index] = volumeVelocity;
//         curvatureVelocites[index] = curvatureVelocity;
//         velocites[index] = currentVelocity;
//         vertices[index] = currentPosition;
//     }
// }

using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;
using System.Collections.Generic;

// IComponentData
public struct KJHLiquidTag : IComponentData { }

// MonoBehaviour
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class KJHLiquid : PoolBehaviour
{
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
    [HideInInspector] public NativeArray<float3> normals;
    [HideInInspector] public NativeArray<float3> gravityVelocites;
    [HideInInspector] public NativeArray<float3> volumeVelocites;
    [HideInInspector] public NativeArray<float3> curvatureVelocites;
    [HideInInspector] public NativeArray<LocalInfo> infos;
    public float globalCurvature = -999f;
    public float initMeanDensity = -999f;
    public struct LocalInfo
    {
        public float3 pos0;
        public float3 normal0;
        public float3 velocity0;
        public float3 pos1;
        public float3 normal1;
        public float3 velocity1;
        public float3 pos2;
        public float3 normal2;
        public float3 velocity2;
        public float localCurvature;
        public float localDensity;
        public NativeArray<int> adjacentVertices;
    }
    [HideInInspector] public NativeArray<RaycastCommand> rayComms;
    [HideInInspector] public NativeArray<RaycastHit> hits;
    [HideInInspector] public NativeArray<float3> hitCloseNormals;
    [HideInInspector] public NativeArray<float3> hitClosePoints;
    [HideInInspector] public Vector3[] verticesToArray;
    [HideInInspector] public Vector3[] normalsToArray;
    [ReadOnlyInspector] public Transform attachTarget;
    [HideInInspector] public Vector3 attachTargetInitPosition;
    [HideInInspector] public Vector3 initPos;
    [ReadOnlyInspector] public float initVolume = -1f;
    void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        TryGetComponent(out mf);
        TryGetComponent(out mr);
        TryGetComponent(out mc);
    }
    void OnDisable() => UnInit();
    void OnDestroy() => UnInit();
    void UnInit()
    {
        attachTarget = null;
        initPos = Vector3.zero;
        initVolume = -1f;
        if (vertices.IsCreated) vertices.Dispose();
        if (velocites.IsCreated) velocites.Dispose();
        if (normals.IsCreated) normals.Dispose();
        if (gravityVelocites.IsCreated) gravityVelocites.Dispose();
        if (rayComms.IsCreated) rayComms.Dispose();
        if (hits.IsCreated) hits.Dispose();
        if (hitCloseNormals.IsCreated) hitCloseNormals.Dispose();
        if (hitClosePoints.IsCreated) hitClosePoints.Dispose();
        if (volumeVelocites.IsCreated) volumeVelocites.Dispose();
        if (curvatureVelocites.IsCreated) curvatureVelocites.Dispose();
        if (infos.IsCreated) infos.Dispose();
    }
    int reDrawCount;
    public void Draw()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            verticesToArray[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z);
        }
        copy.vertices = verticesToArray;
        if (attachTarget != null)
        {
            transform.position = initPos + attachTarget.position - attachTargetInitPosition;
        }
        if (reDrawCount > 15)
        {
            try
            {
                copy.RecalculateNormals();
                copy.RecalculateBounds();
                mc.sharedMesh = null;
                mc.sharedMesh = copy;
            }
            catch
            {

            }

            reDrawCount = 0;
        }
        else
        {
            reDrawCount++;
        }
    }
    static float Area(float3 a, float3 b, float3 c)
    {
        float3 side1 = b - a;
        float3 side2 = c - a;
        float3 crossProduct = math.cross(side1, side2);
        float area = math.length(crossProduct) * 0.5f;
        return area;
    }
    void OnEnable()
    {
        Init();
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
            gravityVelocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            rayComms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
            hitCloseNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            hitClosePoints = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            initVolume = 0.5236f * mc.bounds.size.x * mc.bounds.size.y * mc.bounds.size.z;
            volumeVelocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            curvatureVelocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            infos = new NativeArray<LocalInfo>(copy.vertices.Length, Allocator.Persistent);
            for (int i = 0; i < vertices.Length; i++)
            {
                LocalInfo info = infos[i];
                vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
                velocites[i] = float3.zero;
                normals[i] = new float3(copy.normals[i].x, copy.normals[i].y, copy.normals[i].z);
                info.pos0 = vertices[i];
                info.normal0 = normals[i];
                info.velocity0 = velocites[i];
                infos[i] = info;
            }
            verticesToArray = new Vector3[vertices.Length];
            normalsToArray = new Vector3[vertices.Length];
            #region LocalInfo 담기
            var adjacentVerticesMap = new Dictionary<int, List<int>>();
            var triangles = copy.GetTriangles(0);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v0 = triangles[i];
                int v1 = triangles[i + 1];
                int v2 = triangles[i + 2];
                // Add adjacent vertices for each vertex
                if (!adjacentVerticesMap.ContainsKey(v0)) adjacentVerticesMap[v0] = new List<int>();
                if (!adjacentVerticesMap[v0].Contains(v1)) adjacentVerticesMap[v0].Add(v1);
                if (!adjacentVerticesMap[v0].Contains(v2)) adjacentVerticesMap[v0].Add(v2);
                if (!adjacentVerticesMap.ContainsKey(v1)) adjacentVerticesMap[v1] = new List<int>();
                if (!adjacentVerticesMap[v1].Contains(v0)) adjacentVerticesMap[v1].Add(v0);
                if (!adjacentVerticesMap[v1].Contains(v2)) adjacentVerticesMap[v1].Add(v2);
                if (!adjacentVerticesMap.ContainsKey(v2)) adjacentVerticesMap[v2] = new List<int>();
                if (!adjacentVerticesMap[v2].Contains(v0)) adjacentVerticesMap[v2].Add(v0);
                if (!adjacentVerticesMap[v2].Contains(v1)) adjacentVerticesMap[v2].Add(v1);
            }

            initMeanDensity = 0f;
            for (int i = 0; i < vertices.Length; i++)
            {
                var info = infos[i];
                if (adjacentVerticesMap.ContainsKey(i))
                {
                    // This part would need to be re-designed to handle NativeArray in a Burst-friendly way
                    // For simplicity and getting the logic, we'll assign the first two adjacent vertices
                    if (adjacentVerticesMap[i].Count >= 2)
                    {
                        info.pos1 = vertices[adjacentVerticesMap[i][0]];
                        info.normal1 = normals[adjacentVerticesMap[i][0]];
                        info.velocity1 = velocites[adjacentVerticesMap[i][0]];

                        info.pos2 = vertices[adjacentVerticesMap[i][1]];
                        info.normal2 = normals[adjacentVerticesMap[i][1]];
                        info.velocity2 = velocites[adjacentVerticesMap[i][1]];
                    }
                }
                infos[i] = info;
                initMeanDensity += Area(info.pos0, info.pos1, info.pos2);
            }
            initMeanDensity /= vertices.Length;
            #endregion


        }
        InitEntity();
    }
}

// ISystem or SystemBase
[RequireMatchingQueriesForUpdate]
public partial class KJHLiquidSystem : SystemBase
{
    EntityCommandBufferSystem ecbSystem;
    float elapsed;
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        layerMask = ~(LayerMask.GetMask("Water") | 1 << 2);
    }
    LayerMask layerMask;
    JobHandle jobHandle1;
    JobHandle jobHandle2;
    JobHandle jobHandle3;
    JobHandle jobHandle4;
    JobHandle jobHandle5;
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
            if (mono.initPos == Vector3.zero) mono.initPos = mono.transform.position;
            Vector3 pivot;
            Vector3 displacement = Vector3.zero;
            if (mono.attachTarget != null)
                displacement = mono.attachTarget.position - mono.attachTargetInitPosition;
            pivot = tr.position + displacement;

            #region RaycastCommand Start Job
            var job1 = new KJHLiquidRayCommJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
                rotation = new quaternion(tr.rotation.x, tr.rotation.y, tr.rotation.z, tr.rotation.w),
                vertices = mono.vertices,
                velocites = mono.velocites,
                normals = mono.normals,
                rayComms = mono.rayComms,
                layerMask = layerMask,
            };
            jobHandle1 = job1.Schedule(mono.rayComms.Length, 64, this.Dependency);
            this.Dependency = jobHandle1;
            jobHandle1.Complete();
            #endregion
            #region RaycastCommand Result Job
            jobHandle2 = RaycastCommand.ScheduleBatch(mono.rayComms, mono.hits, 1, this.Dependency);
            this.Dependency = jobHandle2;
            jobHandle2.Complete();
            for (int i = 0; i < mono.hits.Length; i++)
            {
                if (mono.hits[i].collider != null && mono.hits[i].distance > 0f)
                {
                    mono.hitClosePoints[i] = new float3(mono.hits[i].point.x, mono.hits[i].point.y, mono.hits[i].point.z);
                    mono.hitCloseNormals[i] = new float3(mono.hits[i].normal.x, mono.hits[i].normal.y, mono.hits[i].normal.z);
                    if (mono.attachTarget == null)
                    {
                        mono.attachTarget = mono.hits[i].collider.transform;
                        mono.attachTargetInitPosition = mono.hits[i].collider.transform.position;
                    }
                }
            }
            #endregion

#if UNITY_EDITOR
            if (math.length(mono.hitClosePoints[0]) > 0.01f)
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

            #region 현재 프레임에서 Global Curvature를 계산하기
            // jobHandle4
            // 이 부분에 globalCurvature 계산 로직을 추가
            #endregion

            #region Calculate Volume and Curvature Velocities Job
            // 이 부분에 volume과 curvature 계산 잡을 추가
            // jobHandle5
            // var job5 = new KJHLiquidVolumeAndCurvatureJob { /* ... */ };
            // jobHandle5 = job5.Schedule(mono.vertices.Length, 64, this.Dependency);
            // this.Dependency = jobHandle5;
            // jobHandle5.Complete();
            #endregion


            #region Move Job
            elapsed = (float)SystemAPI.Time.ElapsedTime - elapsed;
            elapsed += SystemAPI.Time.DeltaTime;
            var job = new KJHLiquidMoveJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                scale = new float3(tr.localScale.x, tr.localScale.y, tr.localScale.z),
                rotation = new quaternion(tr.rotation.x, tr.rotation.y, tr.rotation.z, tr.rotation.w),
                elapsed = elapsed,
                gravity = new float3(0, -5.81f, 0),
                vertices = mono.vertices,
                velocites = mono.velocites,
                normals = mono.normals,
                gravityVelocites = mono.gravityVelocites,
                hitCloseNormals = mono.hitCloseNormals,
                hitClosePoints = mono.hitClosePoints,
                volumeVelocites = mono.volumeVelocites,
                curvatureVelocites = mono.curvatureVelocites,
                infos = mono.infos,
                initMeanDensity = mono.initMeanDensity,
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
    [ReadOnly] public quaternion rotation;
    [ReadOnly] public NativeArray<float3> vertices;
    [ReadOnly] public NativeArray<float3> velocites;
    [ReadOnly] public NativeArray<float3> normals;
    [WriteOnly] public NativeArray<RaycastCommand> rayComms;
    [ReadOnly] public LayerMask layerMask;
    public void Execute(int index)
    {
        float3 direction = normals[index];
        if (math.length(velocites[index]) >= 0.01f)
            direction = math.normalize(velocites[index]);

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
    [ReadOnly] public quaternion rotation;
    public float elapsed;
    public float3 gravity;
    [ReadOnly] public NativeArray<float3> hitClosePoints;
    [ReadOnly] public NativeArray<float3> hitCloseNormals;
    [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeArray<float3> velocites;
    [ReadOnly] public NativeArray<float3> normals;
    [NativeDisableParallelForRestriction] public NativeArray<float3> gravityVelocites;
    [NativeDisableParallelForRestriction] public NativeArray<float3> volumeVelocites;
    [NativeDisableParallelForRestriction] public NativeArray<float3> curvatureVelocites;
    [ReadOnly] public float initMeanDensity;
    [NativeDisableParallelForRestriction] public NativeArray<KJHLiquid.LocalInfo> infos;
    public void Execute(int index)
    {
        // 정보 가져오기
        float3 currentPosition = vertices[index];
        float3 currentVelocity = velocites[index];
        float3 gravityVelocity = gravityVelocites[index];
        float3 volumeVelocity = volumeVelocites[index];
        float3 curvatureVelocity = curvatureVelocites[index];


        #region 중력
        gravityVelocity += gravity * elapsed;
        gravityVelocity = math.clamp(gravityVelocity, -10f, 0f);
        #endregion
        #region 밀도 비교.. 밀도의 그래디언트 방향으로 힘을 받게






        #endregion
        #region 오브젝트 전체의 평균곡률과 내 주변 곡률을 비교 (?)
        // TODO: 표면 장력 로직 추가
        #endregion

        // 가속이 끝난 각각의 속도들을 합산.
        currentVelocity = gravityVelocity + volumeVelocity + curvatureVelocity;

        #region 어딘가에 충돌된 경우
        if (math.length(hitClosePoints[index]) > 0.1f)
        {
            float3 pos = pivot + scale * vertices[index];
            float distance = math.length(hitClosePoints[index] - pos);
            if (distance < 0.13f)
            {
                gravityVelocity = math.lerp(gravityVelocity, float3.zero, 30f * elapsed);
                currentVelocity = math.lerp(currentVelocity, float3.zero, 30f * elapsed);
                float fixPos = math.length(currentVelocity);
                currentPosition = math.lerp(currentPosition, ((hitClosePoints[index] - pivot) / scale) + 0.1f * hitCloseNormals[index], (3f + fixPos) * elapsed);
            }
        }
        #endregion

        // 최종 위치를 업데이트합니다.
        currentPosition += currentVelocity * elapsed;
        gravityVelocites[index] = gravityVelocity;
        volumeVelocites[index] = volumeVelocity;
        curvatureVelocites[index] = curvatureVelocity;
        velocites[index] = currentVelocity;
        vertices[index] = currentPosition;
    }
}











