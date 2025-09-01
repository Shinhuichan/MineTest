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
    [ReadOnlyInspector] public float chemistryCount = 0f;
    bool isGrab;
    void OnEnable()
    {
        startTime = Time.time;
    }
    public void GrabStart()
    {
        isGrab = true;
        SoundManager.I.PlaySFX("DropGlass1", transform.position, null, 0.8f, 0.3f);
    }
    public void GrabEnd()
    {
        isGrab = false;
    }
    bool isCoolTime;
    float startTime;
    IEnumerator CoolTime()
    {
        yield return new WaitForSeconds(1.4f);
        isCoolTime = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        if(Time.time - startTime > 1.4f)
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
            if (!isCoolTime && !isGrab)
            {
                isCoolTime = true;
                SoundManager.I.PlaySFX("DropGlass2", transform.position, null, 0.8f, 0.6f);
                StartCoroutine(nameof(CoolTime));
            }
    }
}
