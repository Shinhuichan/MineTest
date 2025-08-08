using System;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Random = UnityEngine.Random;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class KJHLiquid : MonoBehaviour
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
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;
    Mesh original;
    [HideInInspector] public Mesh copy;
    //////////////////////////
    uint seed;
    [HideInInspector] public NativeArray<float3> vertices;
    [HideInInspector] public NativeArray<float3> velocites;
    [HideInInspector] public NativeArray<RaycastCommand> raycomms;
    [HideInInspector] public NativeArray<RaycastHit> hits;
    [HideInInspector] public NativeArray<VertexInfo> infos;
    [HideInInspector] public NativeArray<float3> hitCloseNormals;
    [HideInInspector] public NativeArray<float3> hitClosePoints;
    [HideInInspector] public Vector3[] verticesToArray;
    [HideInInspector] public Vector3[] normalsToArray;
    public struct VertexInfo
    {
        // 액체를 삼각형 단위의 표면 폴리곤들이 모여 있는 관점으로 다룰 것이므로
        public float3 pos;
        public float3 velo;
        public float3 normal;
        // 자기자신 버텍스에 대하여 삼각형 △의 상단 꼭지점은
        public float3 upPos;
        public float3 upVelo;
        public float3 upNormal;
        // 자기자신 버텍스에 대하여 삼각형 △의 오른쪽 꼭지점은
        public float3 rightPos;
        public float3 rightVelo;
        public float3 rightNormal;
        // 위를 이용하여 국소적인 벡터장의 변화량.. 즉 라플라시안, 회전, 발산같은 벡터미적분학적인 정보를 이산적인 작은 삼각형으로 근사할것이다.

        // 예를들면 이 버텍스에 대하여 회전(커얼)은 근사적으로 다음과 같이 계산 하려한다.
        public float3 curl => ((math.dot(velo, math.normalize(upPos - pos)) + math.dot(upVelo, math.normalize(rightPos - upPos)) + math.dot(rightVelo, math.normalize(pos - rightPos)))
        / Area(pos, upPos, rightPos)) * normal;
        public float3 divergence => (math.dot(velo, normal) + math.dot(upVelo, upNormal) + math.dot(rightVelo, rightNormal)) / Area(pos, upPos, rightPos);
        public float3 Pressure()
        {
            float idealDensity = 1.0f;
            float springConstant = 10.0f;
            float currentArea = Area(pos, upPos, rightPos);
            float currentDensity = 1.0f / currentArea; // 질량이 1이라고 가정
            float pressure = springConstant * (currentDensity - idealDensity);
            float3 pressureForce = pressure * normal;
            return pressureForce;
        }
        public float3 Viscosity()
        {
            float viscosityConstant = 5.0f; // 점성 계수
            float3 relativeVeloUp = upVelo - velo;
            float3 relativeVeloRight = rightVelo - velo;
            float3 viscosityForce = viscosityConstant * (relativeVeloUp + relativeVeloRight);
            return viscosityForce;
        }
    }
    public static float Area(float3 a, float3 b, float3 c)
    {
        float3 vec1 = b - a;
        float3 vec2 = c - a;
        float3 crossProduct = math.cross(vec1, vec2);
        float area = 0.5f * math.length(crossProduct);
        return area;
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
    void OnDestroy() => UnInit();
    void UnInit()
    {
        if (vertices.IsCreated) vertices.Dispose();
        if (velocites.IsCreated) velocites.Dispose();
        if (raycomms.IsCreated) raycomms.Dispose();
        if (hits.IsCreated) hits.Dispose();
        if (infos.IsCreated) infos.Dispose();
        if (hitCloseNormals.IsCreated) hitCloseNormals.Dispose();
        if (hitClosePoints.IsCreated) hitClosePoints.Dispose();
    }
    public void Init()
    {
        seed = (uint)Random.Range(0, 10000);
        if (original == null)
        {
            original = mf.mesh;
            copy = Instantiate(original);
            mf.mesh = copy;
            //
            vertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            velocites = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            raycomms = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
            infos = new NativeArray<VertexInfo>(copy.vertices.Length, Allocator.Persistent);
            hitCloseNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            hitClosePoints = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            //
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
                velocites[i] = float3.zero;
            }
            // verticesToArray = new Vector3[vertices.Length];
        }
        InitEntity();
    }
    public void Draw()
    {
        // vertices를 verticesToArray에 변환해서 담은 뒤 copy.vertices에 대입
        for (int i = 0; i < vertices.Length; i++)
            verticesToArray[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z);
        copy.vertices = verticesToArray;
        // 노멀 재계산이 필요한 경우
        // copy.RecalculateNormals();
        mc.sharedMesh = null;
        mc.sharedMesh = copy;
    }


}
/////////////////
// IComponentData
/////////////////
public struct KJHLiquidTag : IComponentData { }
/////////////////
// ISystem or SystemBase
/////////////////
[RequireMatchingQueriesForUpdate]
public partial class KJHLiquidSystem : SystemBase
{
    EntityCommandBufferSystem ecbSystem;
    EntityCommandBuffer ecb;
    JobHandle jobHandle1;
    JobHandle jobHandle2;
    JobHandle jobHandle3;
    LayerMask layerMask;
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        layerMask = ~LayerMask.GetMask("Water");
    }
    protected override void OnUpdate()
    {
        ecb = ecbSystem.CreateCommandBuffer();
        Entities.ForEach((KJHLiquid mono) =>
        {














        }).WithoutBurst().Run();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
    protected override void OnDestroy()
    {
        if (jobHandle1.IsCompleted) jobHandle1.Complete();
        if (jobHandle2.IsCompleted) jobHandle2.Complete();
        if (jobHandle3.IsCompleted) jobHandle3.Complete();
    }



}
/////////////////
// Job
/////////////////
[BurstCompile]
public partial struct KJHLiquidRayCommJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public NativeArray<float3> vertices;
    [ReadOnly] public NativeArray<float3> velos;
    [WriteOnly] public NativeArray<RaycastCommand> rayComms;
    [ReadOnly] public LayerMask layerMask;
    public void Execute(int index)
    {
        float3 direction;
        if (math.length(velos[index]) == 0)
        {
            direction = math.down();
        }
        else
        {
            direction = math.normalize(velos[index]);
        }
        rayComms[index] = new RaycastCommand(pivot + vertices[index], direction, new QueryParameters { layerMask = layerMask }, 100f);
    }
}
[BurstCompile]
public partial struct KJHLiquidMoveJob : IJobParallelFor
{
    public float deltaTime;
    public float3 gravity;
    [ReadOnly] public NativeArray<float3> hitClosePoints;
    [ReadOnly] public NativeArray<float3> hitCloseNormals;
    [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
    [NativeDisableParallelForRestriction] public NativeArray<float3> velos;
    public void Execute(int index)
    {
        float3 currentPosition = vertices[index];
        float3 currentVelocity = velos[index];

        // 1. 외부 힘 적용
        currentVelocity += gravity * deltaTime;

        // 2. 충돌 여부 확인 및 속도/위치 조절
        // hitCloseNormals이 영벡터가 아닐 경우 충돌 발생으로 간주합니다.
        if (math.length(hitCloseNormals[index]) > 0.5f)
        {
            currentVelocity = float3.zero;
        }

        // 3. 이류(Advection) - 속도에 기반하여 위치 업데이트
        currentPosition += currentVelocity * deltaTime;

        // 4. 결과 반영
        vertices[index] = currentPosition;
        velos[index] = currentVelocity;
    }
}











