using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public event Action<objectInfo> OnObjectHovered;
    public event Action OnObjectHoverExited;

    private void Awake()
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
    public void NotifyHover(objectInfo info)
    {
        OnObjectHovered?.Invoke(info);
    }
    public void NotifyHoverExit()
    {
        OnObjectHoverExited?.Invoke();
    }
}
