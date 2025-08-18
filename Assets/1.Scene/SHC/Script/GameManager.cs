using UnityEngine;

[System.Serializable]
public struct LaboratoryAccident
{
    public string accidentName;
    public int accidentWeight;
}

public class GameManager : SingletonBehaviour<GameManager>
{
    protected override bool IsDontDestroy() => true;
    
}