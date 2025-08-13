using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public event Action<ObjectInfo> OnObjectHovered;
    public event Action OnObjectHoverExited;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void NotifyHover(ObjectInfo info)
    {
        OnObjectHovered?.Invoke(info);
    }
    public void NotifyHoverExit()
    {
        OnObjectHoverExited?.Invoke();
    }
}
