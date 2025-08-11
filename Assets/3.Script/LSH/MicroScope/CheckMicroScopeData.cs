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

        var obj = go.GetComponent<ItemObject>();
        Data = obj != null ? obj.data : null;

        if(Data !=null && Data.objectSprite != null)
        {
            checkImage.sprite = Data.objectSprite;
            checkImage.preserveAspect = true;
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        checkImage.sprite = null;
        Data = null;
    }
}
