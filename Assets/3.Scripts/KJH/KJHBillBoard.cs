using UnityEngine;
public class KJHBillBoard : MonoBehaviour
{
    private Camera mainCamera;
    public bool Z_Flip;
    //public bool isIsometric;
    Vector3 originalScale;
    //public bool half_BillBoard = true;
    public bool ignore_Perspective = false;
    public float fixDistance = 7.35f;
    public bool half_Ignore_Perspective = false;
    void Awake()
    {
        mainCamera = Camera.main;
        originalScale = transform.localScale;
    }
    void Update()
    {
        // 메인 카메라가 없으면 아무것도 하지 않습니다.
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return; // 여전히 없으면 리턴
        }
        Vector3 pos = mainCamera.transform.position;
        float distance = Vector3.Distance(pos, transform.position);
        if (distance > mainCamera.farClipPlane || distance < mainCamera.nearClipPlane) 
        { 
            return; 
        }
        int flip = Z_Flip ? -1 : 1 ;
        transform.LookAt(transform.position + flip * (transform.position - mainCamera.transform.position), Vector3.up);
        if (ignore_Perspective && fixDistance > 0)
        {
            float a0 = distance / fixDistance;
            if (!half_Ignore_Perspective)
            {
                if (distance >= fixDistance)
                {
                    transform.localScale = new Vector3(a0 * originalScale.x, a0 * originalScale.y, 1 * originalScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(a0 * originalScale.x, a0 * originalScale.y, 1 * originalScale.z);
                }
            }
            else
            {
                if (distance >= fixDistance)
                {
                    float a1 = (0.5f) + (0.5f * a0);
                    transform.localScale = new Vector3(a1 * originalScale.x, a1 * originalScale.y, 1 * originalScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(a0 * originalScale.x, a0 * originalScale.y, 1 * originalScale.z);
                }
            }
        }

    }
}