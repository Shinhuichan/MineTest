using UnityEngine;

[CreateAssetMenu(fileName ="Object", menuName ="ObjectData")]
public class ObjectData : ScriptableObject
{
    public string objectName;
    public string objectDescription;
    public Sprite objectSprite;
    public bool isConductivity;
}
