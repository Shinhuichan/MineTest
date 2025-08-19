using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjScanner_H : MonoBehaviour
{
    [Tooltip("레이를 쏠 최대 거리")]
    public float scanDistance = 5f;
    [Tooltip("어떤 레이어의 오브젝트를 감지할지 설정")]
    public LayerMask scanLayerMask;

    // 마지막으로 감지했던 오브젝트
    private ObjectInfo lastDetectiveObject = null;

    private void Update()
    {
        // 이 스크립트가 부착된 오브젝트의 위치에서 정면으로 레이를 쏜다
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        ObjectInfo detectedInfo = null;

        // 레이캐스트 실행
        if (Physics.Raycast(ray, out hit, scanDistance, scanLayerMask))
        {
  Debug.Log("레이가 맞춘 오브젝트: " + hit.collider.name);

            detectedInfo = hit.collider.GetComponent<ObjectInfo>();
        }
        
        if(detectedInfo != lastDetectiveObject)
        {
            if(detectedInfo != null)
            {
                UIManager.Instance.NotifyHover(detectedInfo);

            }
            else
            {
                UIManager.Instance.NotifyHoverExit();

            }
            lastDetectiveObject = detectedInfo;
        }

        

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * scanDistance);
    }
}
