using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjScanner_H : MonoBehaviour
{
    [Tooltip("레이를 쏠 최대 거리")]
    public float scanDistance = 5f;
    [Tooltip("어떤 레이어의 오브젝트를 감지할지 설정")]
    public LayerMask scanLayerMask;

    // 현재 바라보고 있는 오브젝트를 저장하기 위한 변수
    private ObjectInfo currentHoveredObject = null;

    private void Update()
    {
        // 이 스크립트가 부착된 오브젝트의 위치에서 정면으로 레이를 쏜다
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // 레이캐스트 실행
        if (Physics.Raycast(ray, out hit, scanDistance, scanLayerMask))
        {
            // 레이에 맞은 오브젝트에서 objectInfo 컴포넌트를 찾아본다
            ObjectInfo detectedInfo = hit.collider.GetComponent<ObjectInfo>();
            //objectInfo를 찾았고 이전에 보던것과 다른 오브젝트라면
            if (detectedInfo != null && detectedInfo != currentHoveredObject)
            {
                // 현재 바라보는 오브젝트를 새로고치고 UI 매니저에게 알린다
                currentHoveredObject = detectedInfo;
                UIManager.Instance.NotifyHover(currentHoveredObject);
            }
        }
        else
        {
            //레이에 아무것도 맞지 않았는데, 이전에 바라보던 오브젝트가 있었다면
            if (currentHoveredObject != null)
            {
                //바라보기를 멈췄다고 ui매니저에게 알린다
                currentHoveredObject = null;
                UIManager.Instance.NotifyHoverExit();
            }
        }
        

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * scanDistance);
    }
}
