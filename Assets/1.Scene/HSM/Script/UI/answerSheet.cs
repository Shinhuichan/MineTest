using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class answerSheet : MonoBehaviour
{
    // 인스펙터에서 이 정답 판에 해당하는 OreData애셋을 연결합니다
    public OreData associatedOreData;
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private Vector3 originalPosition;
    private Quaternion originalRotaion;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        originalPosition = transform.position;
        originalRotaion = transform.rotation;
    }


    public void ForceCancelInteraction()
    {
        if (grabInteractable.isSelected)
        {
            XRInteractionManager interactionManager = grabInteractable.interactionManager;
            interactionManager.CancelInteractableSelection(grabInteractable);
        }
    }

    public void StartReturnToPositionRoutine()
    {
        StartCoroutine(ReturnToPositionRoutine());
    }
    private IEnumerator ReturnToPositionRoutine()
    {
        grabInteractable.enabled = false;
       
        if(rb != null)
        {
            rb.isKinematic = true;
        }
        yield return null;

        transform.position = originalPosition;
        transform.rotation = originalRotaion;

        //Debug.Break(); // 여기서 에디터를 강제로 일시정지시킵니다.
        Debug.Log($"<color=orange>{gameObject.name}을(를) 원래 위치로 되돌렸습니다.</color>");
        yield return null;

        grabInteractable.enabled = true;
    }
}
