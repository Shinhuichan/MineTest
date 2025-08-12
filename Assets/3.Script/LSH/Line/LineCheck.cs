using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCheck : MonoBehaviour
{
    private LineRenderer line;

    public Transform pos1;
    public Transform pos2;
    Vector3 pos1_transform;
    Vector3 pos2_transform;

    void Start()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.enabled = true;

        pos1_transform = new Vector3(pos1.position.x, pos1.position.y, pos1.position.z);
        pos2_transform = new Vector3(pos2.position.x, pos2.position.y, pos2.position.z);
        line.SetPosition(0, pos1_transform);
        line.SetPosition(1, pos2_transform);
    }
}
