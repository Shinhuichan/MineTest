using UnityEngine;

public enum OreType
{
    Calcite = 0, // 방해석
    Sodium, // 나트륨
    Copper, // 구리
    Graphite, // 흑연
    Sphalerite // 섬아연석
}
public enum ChemicalType
{
    None = 0,
    Water,
    Acid
}
[CreateAssetMenu(fileName = "OreData", menuName = "Ore")]
public class OreData : ScriptableObject
{
    public OreType type;
    public ChemicalType isReactingToChem;
    public float hardness;
    public Sprite microShape;
    public bool electroConduct;
}
