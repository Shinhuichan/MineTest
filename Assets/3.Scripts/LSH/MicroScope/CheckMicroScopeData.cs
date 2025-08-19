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
        var progress = go.GetComponent<ExperimentProgress_H>(); // �������

        if(objectInfo != null && objectInfo.oreData != null)
        {
            // ���̰濡 �´� �̹��� ǥ��
            checkImage.sprite = objectInfo.oreData.microShape;
            checkImage.preserveAspect = true;
            
            //���� ���� ������Ʈ �� ���
            if(progress != null)
            {
                // ���̰� ���� �Ϸ� ���¸� Ʈ��� ����
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
