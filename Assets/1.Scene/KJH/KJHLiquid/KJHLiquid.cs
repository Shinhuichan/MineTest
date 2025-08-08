using System;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Random = UnityEngine.Random;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class KJHLiquid : MonoBehaviour
{
    #region Entity Setting
    EntityManager entityManager;
    Entity entity;
    void StartEntity()
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
    [HideInInspector] public NativeArray<float3> currVertices;
    [HideInInspector] public NativeArray<float3> currVelocities;
    [HideInInspector] public NativeArray<RaycastCommand> raycastCommands;
    [HideInInspector] public NativeArray<RaycastHit> hits;
    // 위 NativeArray<RaycastHit>는 다른스레드 잡으로 보낼수 없어서 아래로 변환해서 보낸다.
    // 거리 0.2이하로 레이가 닿았을 경우에만 float3는 hit.normal이 되고 나머지 경우에는 영벡터다  
    [HideInInspector] public NativeArray<float3> isHitNormals;
    Vector3[] currVerticesToArray;
    uint seed;
    void Awake()
    {
        TryGetComponent(out mf);
        TryGetComponent(out mr);
        TryGetComponent(out mc);
        // Entity Setting
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    void OnEnable()
    {
        Init();
    }
    public void Init()
    {
        if (original == null)
        {
            original = mf.mesh;
            copy = Instantiate(original);
            mf.mesh = copy;
            currVertices = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            currVelocities = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            raycastCommands = new NativeArray<RaycastCommand>(copy.vertices.Length, Allocator.Persistent);
            hits = new NativeArray<RaycastHit>(copy.vertices.Length, Allocator.Persistent);
            isHitNormals = new NativeArray<float3>(copy.vertices.Length, Allocator.Persistent);
            for (int i = 0; i < currVertices.Length; i++)
            {
                currVertices[i] = new float3(copy.vertices[i].x, copy.vertices[i].y, copy.vertices[i].z);
                currVelocities[i] = float3.zero;
            }
            currVerticesToArray = new Vector3[currVertices.Length];
        }
        seed = (uint)Random.Range(0, 10000);
        StartEntity();
    }


    // void Update()
    // {
    //     Draw();
    // }
    public void Draw()
    {
        // currVertices를 currVerticesToArray에 변환해서 담은 뒤 copy.vertices에 대입
        for (int i = 0; i < currVertices.Length; i++)
            currVerticesToArray[i] = new Vector3(currVertices[i].x, currVertices[i].y, currVertices[i].z);
        copy.vertices = currVerticesToArray;
        // 노멀 재계산이 필요한 경우
        // copy.RecalculateNormals();
        // MeshCollider의 sharedMesh를 null로 설정하여 기존 참조를 해제합니다 (필수).
        mc.sharedMesh = null;
        // Bake된 메쉬를 MeshCollider에 할당합니다.
        mc.sharedMesh = copy;

        Debug.Log($"{currVertices[0]} vs {copy.vertices[0]}");
    }



    void OnDestroy()
    {
        if (currVertices.IsCreated) currVertices.Dispose();
        if (currVelocities.IsCreated) currVelocities.Dispose();
        if (raycastCommands.IsCreated) raycastCommands.Dispose();
        if (hits.IsCreated) hits.Dispose();
        if (isHitNormals.IsCreated) isHitNormals.Dispose();
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
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        layerMask = ~LayerMask.GetMask("Water");
    }
    JobHandle jobHandle1;
    JobHandle jobHandle2;
    JobHandle jobHandle3;
    LayerMask layerMask;
    protected override void OnUpdate()
    {
        ecb = ecbSystem.CreateCommandBuffer();
        // MonoBehaviour에 저장된 NativeArray를 Job에 전달하여 직접 수정
        Entities.ForEach((KJHLiquid mono) =>
        {
            Vector3 pivot = mono.transform.position;
            var job1 = new KJHLiquidRaycastCommandJob
            {
                pivot = new float3(pivot.x, pivot.y, pivot.z),
                currVertices = mono.currVertices,
                currVelocities = mono.currVelocities,
                raycastCommands = mono.raycastCommands,
                layerMask = layerMask,
            };
            jobHandle1 = job1.Schedule(mono.raycastCommands.Length, 64, this.Dependency);
            this.Dependency = jobHandle1;
            jobHandle1.Complete();


            jobHandle2 = RaycastCommand.ScheduleBatch(mono.raycastCommands, mono.hits, 1, this.Dependency);
            this.Dependency = jobHandle2;
            jobHandle2.Complete();


            // mono.hits 결과를 mono.isHitNormals 로 변환
            for (int i = 0; i < mono.hits.Length; i++)
            {
                // if (i == 0)
                // {
                //     Debug.Log($"{mono.hits[i].collider} , {mono.hits[i].distance}, {mono.currVertices[i]}");
                // }
                // mono.copy.vertices[i] = mono.currVertices[i];

                Debug.Log($"{mono.currVertices[0]} vs {mono.copy.vertices[0]}");
                
                if (mono.hits[i].collider != null && mono.hits[i].distance <= 0.2f && mono.hits[i].distance > 0f)
                {
                    mono.isHitNormals[i] = new float3(mono.hits[i].normal.x, mono.hits[i].normal.y, mono.hits[i].normal.z);
                }
                else
                {
                    mono.isHitNormals[i] = float3.zero;
                }
            }


            // 본격적인 유체 표면 버텍스들 움직임 잡 스케줄링
            var job = new KJHLiquidVertexMoveJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                gravity = new float3(0, -9.81f, 0),
                currVertices = mono.currVertices,
                currVelocities = mono.currVelocities,
                isHitNormals = mono.isHitNormals,
            };
            jobHandle3 = job.Schedule(mono.currVertices.Length, 64, this.Dependency);
            this.Dependency = jobHandle3;
            jobHandle3.Complete();


            // Job이 전부 완료된 후 Draw() 호출
            mono.Draw();


            // if (entityData.pos.y <= entityData.judgeHeight)
            // {
            //     mono.DeSpawn();
            //     ecb.DestroyEntity(entity);
            // }






        }).WithoutBurst().Run();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
    protected override void OnDestroy()
    {
        if (jobHandle1.IsCompleted)
            jobHandle1.Complete();
        if (jobHandle2.IsCompleted)
            jobHandle2.Complete();
        if (jobHandle3.IsCompleted)
            jobHandle3.Complete();
    }
}
/////////////////
// IJob
/////////////////
[BurstCompile]
public partial struct KJHLiquidRaycastCommandJob : IJobParallelFor
{
    [ReadOnly] public float3 pivot;
    [ReadOnly] public NativeArray<float3> currVertices;
    [ReadOnly] public NativeArray<float3> currVelocities;
    [WriteOnly] public NativeArray<RaycastCommand> raycastCommands;
    [ReadOnly] public LayerMask layerMask;
    public void Execute(int index)
    {
        float3 direction;
        if (math.length(currVelocities[index]) == 0)
        {
            direction = math.down();
        }
        else
        {
            direction = math.normalize(currVelocities[index]);
        }
        raycastCommands[index] = new RaycastCommand(pivot + currVertices[index], direction, new QueryParameters { layerMask = layerMask }, 100f);
    }
}
[BurstCompile]
public partial struct KJHLiquidVertexMoveJob : IJobParallelFor
{
    public float deltaTime;
    public float3 gravity;
    [ReadOnly] public NativeArray<float3> isHitNormals;
    [NativeDisableParallelForRestriction] public NativeArray<float3> currVertices;
    [NativeDisableParallelForRestriction] public NativeArray<float3> currVelocities;
    public void Execute(int index)
    {
        float3 currentPosition = currVertices[index];
        float3 currentVelocity = currVelocities[index];

        // 1. 외부 힘 적용
        currentVelocity += gravity * deltaTime;

        // 2. 충돌 여부 확인 및 속도/위치 조절
        // isHitNormals이 영벡터가 아닐 경우 충돌 발생으로 간주합니다.
        if (math.length(isHitNormals[index]) > 0.5f)
        {
            currentVelocity = float3.zero;
        }

        // 3. 이류(Advection) - 속도에 기반하여 위치 업데이트
        currentPosition += currentVelocity * deltaTime;

        // 4. 결과 반영
        currVertices[index] = currentPosition;
        currVelocities[index] = currentVelocity;
    }
}









// public static float Area(float3 a, float3 b, float3 c)
// {
//     float3 vec1 = b - a;
//     float3 vec2 = c - a;
//     float3 crossProduct = math.cross(vec1, vec2);
//     float area = 0.5f * math.length(crossProduct);
//     return area;
// }
// public struct Info
// {
//     // 액체 메쉬를 삼각형 단위의 표면폴리곤들이 모여 있는 관점으로 다룰 것이므로
//     // 자기자신 버텍스는 △모양에서 왼쪽 하단에 있는 점으로 취급한다.
//     public float3 pos_0;
//     public float3 velo_0;
//     public float3 nor_0;
//     // 자기자신 버텍스에 대하여 삼각형 △의 상단 꼭지점은
//     public float3 pos_1;
//     public float3 velo_1;
//     public float3 nor_1;
//     // 자기자신 버텍스에 대하여 삼각형 △의 오른쪽 꼭지점은
//     public float3 pos_2;
//     public float3 velo_2;
//     public float3 nor_2;
//     // 위를 이용하여 국소적인 벡터장의 변화량.. 즉 라플라시안, 회전, 발산같은 벡터미적분학적인 정보를 이산적인 작은 삼각형으로 근사할것이다.
//     // 예를들면 이 버텍스에 대하여 회전(커얼)은 근사적으로 다음과 같이 계산 하려한다.
//     public float3 curl
//     => ((math.dot(velo_0, math.normalize(pos_1 - pos_0)) + math.dot(velo_1, math.normalize(pos_2 - pos_1)) + math.dot(velo_2, math.normalize(pos_0 - pos_2)))
//     / Area(pos_0, pos_1, pos_2)) * nor_0;
//     public float3 divergence => (math.dot(velo_0, nor_0) + math.dot(velo_1, nor_1) + math.dot(velo_2, nor_2)) / Area(pos_0, pos_1, pos_2);
// }
