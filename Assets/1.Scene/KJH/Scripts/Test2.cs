using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Random = UnityEngine.Random;
public class Test2 : MonoBehaviour
{
    MeshFilter mf;
    MeshRenderer mr;
    MeshCollider mc;
    Mesh original;
    [HideInInspector] public Mesh copy;
    [HideInInspector] public Vector3[] vertices;
    [HideInInspector] public Vector3[] velocites;
    [HideInInspector] public Ray[] rays;
    [HideInInspector] public RaycastHit[] hits;
    [HideInInspector] public NativeArray<LocalInfo> localInfos;
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
    void Start()
    {
        original = mf.mesh;
        copy = Instantiate(original);
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
        }
        // === 6개 최근접 이웃 찾기 ===
        for (int i = 0; i < vertices.Length; i++)
        {
            list.Clear();
            Vector3 pi = vertices[i];
            for (int j = 0; j < vertices.Length; j++)
            {
                if (i == j) continue;
                float d = Vector3.Distance(vertices[j] , pi);
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



}
