using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public event Action<ObjectInfo> OnObjectHovered;
    public event Action OnObjectHoverExited;

    public event Action<ObjectInfo> OnExperimentUpdated;
    private void Awake()
    {
        // 싱글톤 초기화는 Awake에서
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void NotifyExperimentUpdated(ObjectInfo info)
    {
        OnExperimentUpdated?.Invoke(info);
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