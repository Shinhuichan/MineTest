using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class blackBoardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mainExperimentTiltle;
    [SerializeField] private List<TMP_Text> objectStatusTexts;

    [SerializeField] private Button Mineral_Table_Button;
    [SerializeField] private Button Mineral_Test_Button;
    [SerializeField] private List<string> experimentNames;

    public GameObject experimentStatusPanel;
    public GameObject testPanel;

    private int currentlyDisplayedIndex = 0;

    public void Start()
    {
        ShowExperimentView();
        Mineral_Table_Button.gameObject.SetActive(true);
        Mineral_Test_Button.gameObject.SetActive(false);
    }
    public void ShowExperimentView()
    {
        experimentStatusPanel.SetActive(true);
        testPanel.SetActive(false);
    }
    public void ShowTestView()
    {
        experimentStatusPanel.SetActive(false);
        testPanel.SetActive(true);

        Mineral_Table_Button.gameObject.SetActive(false);
        Mineral_Test_Button.gameObject.SetActive(true);
    }
    private int GetTotalExperimentCount()
    {
        if(GameManager.I != null &&
            GameManager.I.progreses != null &&
            GameManager.I.progreses.Count > 0 &&
            GameManager.I.progreses[0].isClear != null)
        {
            return GameManager.I.progreses[0].isClear.Length;
        }
        return experimentNames != null ? experimentNames.Count : 0;
    }
    private string GetExperimentTitleSafe(int idx)
    {
        if(experimentNames != null &&
            idx >= 0 &&
            idx < experimentNames.Count&&
            !string.IsNullOrEmpty(experimentNames[idx]))
        {
            return experimentNames[idx];

        }
        return $"실험{idx + 1}";
    }
    public void ShowExperimentStatus(int experimentIndex)
    {
        //������ ������ ��ȿ�� ���� ���� �ִ��� Ȯ��
        int total = GetTotalExperimentCount();
        if(experimentIndex < 0 || experimentIndex >= total)
        {
            
            Debug.LogError($"[Board] 잘못된 실험 인덱스: {experimentIndex} / total={total}");
            return;
        }
        currentlyDisplayedIndex = experimentIndex;


        // 1. ���� ������Ʈ 
        mainExperimentTiltle.text = experimentNames[currentlyDisplayedIndex];

        // 2. O/X ���� ������Ʈ
        UpdateStatusDisplay();
Debug.Log($"[Board] ShowExperimentStatus({experimentIndex})");
    }

    public void UpdateStatusDisplay()
    {
        if (GameManager.I == null || GameManager.I.progreses == null) return;

        /* if(currentlyDisplayedIndex < 0 ||
             GameManager.I.progreses.Count == 0 ||
             GameManager.I.progreses[0].isClear == null ||
             currentlyDisplayedIndex >= GameManager.I.progreses[0].isClear.Length)
         {
             Debug.LogError($"[Blackboard] invalid currentDisplayedIndex = {currentlyDisplayedIndex}");
             return;
         }*/
        // 인덱스 유효성 가드
        if (GameManager.I.progreses.Count == 0) return;
        int totalCols = GameManager.I.progreses[0].isClear.Length;
        //var cols = GameManager.I.progreses[0].isClear;
        if (currentlyDisplayedIndex < 0  || currentlyDisplayedIndex >= totalCols)
        {
            Debug.LogWarning($"[Board] invalid currentlyDisplayedIndex={currentlyDisplayedIndex} / total={totalCols}");
            return;
        }


        int loopCount = Mathf.Min(objectStatusTexts.Count, GameManager.I.progreses.Count);

        for(int i = 0; i < loopCount; i++)
        {
            
            bool isDone = GameManager.I.progreses[i].isClear[currentlyDisplayedIndex];
            objectStatusTexts[i].text = isDone ? "O" : "X";
            objectStatusTexts[i].color = isDone ? Color.green : Color.red;
        }

Debug.Log($"[Board] UpdateStatusDisplay idx={currentlyDisplayedIndex}");
    }

}
