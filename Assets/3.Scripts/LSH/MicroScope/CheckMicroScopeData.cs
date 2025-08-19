using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
public class CheckMicroScopeData : MonoBehaviour
{
    public Image checkImage;
    
    private ObjectData Data;

    private XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnSelectEntered);
        socket.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var go = (args.interactableObject as Component)?.gameObject;
        if (go == null) return;


        var objectInfo = go.GetComponent<ObjectInfo>();
        var progress = go.GetComponent<ExperimentProgress_H>(); // 진행상태

        if(objectInfo != null && objectInfo.oreData != null)
        {
            // 현미경에 맞는 이미지 표시
            checkImage.sprite = objectInfo.oreData.microShape;
            checkImage.preserveAspect = true;
            
            //실험 상태 업데이트 및 방송
            if(progress != null)
            {
                // 현미경 실험 완료 상태를 트루로 변경
                progress.isMicroScopeCheckd = true;

                UIManager.Instance.NotifyExperimentUpdated(objectInfo);
            }
        }

      
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        checkImage.sprite = null;
        Data = null;
    }
}
