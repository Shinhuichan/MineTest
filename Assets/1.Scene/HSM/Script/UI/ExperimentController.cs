using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentController : MonoBehaviour
{
    [Header("실험 정보")]
    [SerializeField]
    private int experimentNumber;

    public void OnExperimentSuccess(ObjectInfo oreObject)
    {
        if (oreObject == null) return;

        GameManager.I.Clear(
            oreObject.oreData,
            this.experimentNumber,
            "");
       
        Debug.Log($"{oreObject.name}으로 {this.experimentNumber}번 실험 완료를 GameManager에 보고했습니다.");
    }
}
  

