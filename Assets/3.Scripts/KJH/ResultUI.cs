using UnityEngine;
using TMPro;
public class ResultUI : MonoBehaviour
{
    TMP_Text[] tMP_Texts;
    void Awake()
    {
        tMP_Texts = transform.GetChild(0).GetComponentsInChildren<TMP_Text>();
        //Debug.Log(tMP_Texts.Length);
    }
    public void ShowText(OreData oreData, int experimentNumber, string boardText)
    {
        int row = OreDataToIndex(oreData) + 1;
        if (row <= 0 || row >= 4) return;
        int colum = experimentNumber + 1;
        if (colum <= 0 || colum >= 5) return;
        int gridIndex = 0;
        if (row == 1)
            gridIndex = 5 + colum;
        else if (row == 2)
            gridIndex = 10 + colum;
        else if (row == 3)
            gridIndex = 15 + colum;
        if (gridIndex == 0) return;
        tMP_Texts[gridIndex].text = boardText;
    }
    public string GetText(OreData oreData, int experimentNumber)
    {
        int row = OreDataToIndex(oreData) + 1;
        if (row <= 0 || row >= 4) return "";
        int colum = experimentNumber + 1;
        if (colum <= 0 || colum >= 5) return "";
        int gridIndex = 0;
        if (row == 1)
            gridIndex = 5 + colum;
        else if (row == 2)
            gridIndex = 10 + colum;
        else if (row == 3)
            gridIndex = 15 + colum;
        if (gridIndex == 0) return "";
        return tMP_Texts[gridIndex].text;
    }
    int OreDataToIndex(OreData oreData)
    {
        int find = GameManager.I.progreses.FindIndex(x => x.oreData == oreData);
        return find;
    }
}
