using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightExperiment : MonoBehaviour
{
    public GameObject gameObject;
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

        if (Data != null && Data.isConductivity)
        {
            gameObject.SetActive(true);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        Data = null;
        gameObject.SetActive(false);
    }
}
