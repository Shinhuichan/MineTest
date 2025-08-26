using CustomInspector;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class NameTagController : MonoBehaviour
{
    [SerializeField] private TextMesh[] nameTags;
    [SerializeField][ReadOnly] private ObjectInfo[] ores;

    void Awake()
    {
        ores = FindObjectsByType<ObjectInfo>(FindObjectsSortMode.None);
        Init();
    }

    void Init()
    {
        for (int i = 0; i < nameTags.Length; i++)
            nameTags[i].text = ores[i].oreData.type.ToString();
    }
}
