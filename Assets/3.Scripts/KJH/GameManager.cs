using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
[System.Serializable]
public struct LaboratoryAccident
{
    public string accidentName;
    public int accidentWeight;
}
public class GameManager : SingletonBehaviour<GameManager>
{
    protected override bool IsDontDestroy() => true;
    [SerializeField] List<LaboratoryAccident> accidents = new List<LaboratoryAccident>();
    public List<Progress> progreses;
    ActionBasedController[] controllers;
    [ReadOnlyInspector][SerializeField] ResultUI resultUI;
    [System.Serializable]
    public class Progress
    {
        public string Name;
        public Transform transform;
        public OreData oreData;
        public bool[] isClear = new bool[4];
    }
    protected override void Awake()
    {
        base.Awake();
        controllers = FindObjectsByType<ActionBasedController>(FindObjectsSortMode.InstanceID);
        resultUI = FindAnyObjectByType<ResultUI>();
    }
    void Start()
    {
        Init();
    }
    public void Init()
    {
        accidents.Clear();
        ObjectInfo[] temp = FindObjectsByType<ObjectInfo>(FindObjectsSortMode.InstanceID);
        progreses.Clear();
        for (int i = 0; i < temp.Length; i++)
        {
            Progress pr = new Progress();
            Array.Fill(pr.isClear, false);
            pr.Name = temp[i].transform.name;
            pr.transform = temp[i].transform;
            pr.oreData = temp[i].oreData;
            progreses.Add(pr);
        }
    }
    public void Clear(OreData oreData, int experimentNumber, string boardText)
    {
        if (experimentNumber < 0 || experimentNumber > 3)
        {
            Debug.Log($"experimentNumber는 0(염산반응) , 1(경도) , 2(현미경) , 3(전기전도) 들만 가능합니다. ( {experimentNumber} ) ");
            return;
        }
        int find = progreses.FindIndex(x => x.oreData.type == oreData.type);
        if (find == -1)
        {
            Debug.Log($"{oreData.type.ToString()} 라는 광물은 현재 씬에 없습니다.");
            return;
        }
        if (progreses[find].isClear[experimentNumber])
        {
            Debug.Log("이미 완료한 실험입니다.");
            return;
        }
        // 실험 완료
        progreses[find].isClear[experimentNumber] = true;
        // 햅틱 반응
        foreach (var ctrl in controllers)
        {
            ctrl.SendHapticImpulse(0.5f, 0.2f);
        }
        Debug.Log($"광물 {oreData.type.ToString()}로 실험{experimentNumber}을 완료했습니다.");
        resultUI.ShowText(oreData, experimentNumber, boardText);
    }
    public void EditBoardText(OreData oreData, int experimentNumber, string boardText)
    {
        resultUI.ShowText(oreData, experimentNumber, boardText);
    }
    public string GetBoardText(OreData oreData, int experimentNumber)
    {
        return resultUI.GetText(oreData, experimentNumber);
    }


    // public void ShowText(Vector3 pos, string str)
    // {

    // }
    // public void FadeIn(float time)
    // {

    // }
    // public void FadeOut(float time)
    // {

    // }
    // public void ChangeScene(string sceneName)
    // {

    // }
    // public void ChangeScene(int sceneIndex)
    // {

    // }






}