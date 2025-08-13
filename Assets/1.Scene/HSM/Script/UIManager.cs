using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public event Action<ObjectInfo> OnObjectHovered;
    public event Action OnObjectHoverExited;
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

    public void NotifyHover(ObjectInfo info)
    {
        OnObjectHovered?.Invoke(info);
    }

    public void NotifyHoverExit()
    {
        OnObjectHoverExited?.Invoke();
    }
}