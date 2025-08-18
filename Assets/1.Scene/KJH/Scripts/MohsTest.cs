using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Random = UnityEngine.Random;
using System;
// --- IComponentData ---
public struct MohsTestTag : IComponentData { }
// --- MonoBehaviour ---
[RequireComponent(typeof(MeshCollider))]
public class MohsTest : MonoBehaviour
{
    public int number;
    Vector3 prevDisplacement = Vector3.zero;
    Vector3 displacement = Vector3.zero;
    Vector3 velocity;
    MeshCollider mc;
    LineRenderer lr;
    LineRenderer targetLr;
    int lrIndex;
    //[SerializeField] List<Vector3> record = new List<Vector3>();
    void Awake()
    {
        TryGetComponent(out mc);
        lr = GetComponentInParent<LineRenderer>();
    }
    void OnEnable()
    {
        MeshFilter meshFilter = transform.parent.GetComponent<MeshFilter>();
        mc.sharedMesh = null;
        mc.sharedMesh = meshFilter.sharedMesh;
        Vector3[] temp = new Vector3[100];
        Array.Fill(temp, Vector3.zero);
        lr.SetPositions(temp);
    }
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out ObjectInfo info))
        {
            prevDisplacement = displacement;
            displacement = transform.position - other.transform.position;
            velocity = displacement - prevDisplacement;
            if (velocity.magnitude > 0.005f)
            {
                if (number < info.oreData.hardness)
                {
                    Collider myColl = transform.parent.GetComponent<Collider>();
                    Vector3 closet = myColl.ClosestPoint(other.transform.position);
                    DebugExtension.DebugWireSphere(closet, Color.blue, 0.01f, 0.02f, true);
                    lrIndex++;
                    lrIndex %= 100;
                    lr.SetPosition(lrIndex, transform.InverseTransformPoint(closet));
                }
                else
                {
                    Collider targetColl = other.transform.GetComponent<Collider>();
                    targetLr = other.transform.GetComponent<LineRenderer>();
                    Vector3 closet = targetColl.ClosestPoint(transform.position);
                    lrIndex++;
                    lrIndex %= 100;
                    targetLr.SetPosition(lrIndex, other.transform.InverseTransformPoint(closet));

                }
            }
        }
    }




}
