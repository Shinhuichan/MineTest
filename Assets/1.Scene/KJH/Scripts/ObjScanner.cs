using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjScanner : MonoBehaviour
{
    [Tooltip("���̸� �� �ִ� �Ÿ�")]
    public float scanDistance = 5f;
    [Tooltip("� ���̾��� ������Ʈ�� �������� ����")]
    public LayerMask scanLayerMask;

    // ���� �ٶ󺸰� �ִ� ������Ʈ�� �����ϱ� ���� ����
    private ObjectInfo currentHoveredObject = null;

    private void Update()
    {
        // �� ��ũ��Ʈ�� ������ ������Ʈ�� ��ġ���� �������� ���̸� ���
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // ����ĳ��Ʈ ����
        if (Physics.Raycast(ray, out hit, scanDistance, scanLayerMask))
        {
            // ���̿� ���� ������Ʈ���� objectInfo ������Ʈ�� ã�ƺ���
            ObjectInfo detectedInfo = hit.collider.GetComponent<ObjectInfo>();
            //objectInfo�� ã�Ұ� ������ �����Ͱ� �ٸ� ������Ʈ���
            if (detectedInfo != null && detectedInfo != currentHoveredObject)
            {
                // ���� �ٶ󺸴� ������Ʈ�� ���ΰ�ġ�� UI �Ŵ������� �˸���
                currentHoveredObject = detectedInfo;
                UIManager.Instance.NotifyHover(currentHoveredObject);
            }
        }
        else
        {
            //���̿� �ƹ��͵� ���� �ʾҴµ�, ������ �ٶ󺸴� ������Ʈ�� �־��ٸ�
            if (currentHoveredObject != null)
            {
                //�ٶ󺸱⸦ ����ٰ� ui�Ŵ������� �˸���
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
