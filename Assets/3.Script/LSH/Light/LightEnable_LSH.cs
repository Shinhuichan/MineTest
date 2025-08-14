using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LightEnable_LSH : MonoBehaviour
{
    public GameObject LightObject;

    private OreData Data;

    private XRSocketInteractor socket;
    void Awake()
    {
        socket = GetComponentInChildren<XRSocketInteractor>();
        LightObject.SetActive(false);
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

        var obj = go.GetComponent<ObjectInfo>();

        Data = obj != null ? obj.oreData : null;

        if (Data != null && Data.electroConduct == true)
        {
            LightObject.SetActive(true);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        LightObject.SetActive(false);
        Data = null;
    }
}
