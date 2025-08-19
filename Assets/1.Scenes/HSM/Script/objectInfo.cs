using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInfo : MonoBehaviour
{
    public string objectName;
    [TextArea(3, 10)]
    public string description;
    public Sprite objImage;
    public OreData oreData;
}
