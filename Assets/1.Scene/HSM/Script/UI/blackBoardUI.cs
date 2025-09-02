using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class blackBoardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mainExperimentTiltle;
    [SerializeField] private List<TMP_Text> objectStatusTexts;
    
    [SerializeField] private List<string> experimentNames;

    public GameObject experimentStatusPanel;
    public GameObject testPanel;

    private int currentlyDisplayedIndex = 0;

    public void Start()
    {
        ShowExperimentView();
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
    }
    public void ShowExperimentStatus(int experimentIndex)
    {
        //������ ������ ��ȿ�� ���� ���� �ִ��� Ȯ��
        if(experimentIndex < 0)
        {
            Debug.Log(experimentNames.Count);
            Debug.LogError("�߸��� ���� �ε��� �Դϴ�");
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
        var cols = GameManager.I.progreses[0].isClear;
        if (currentlyDisplayedIndex < 0 || cols == null || currentlyDisplayedIndex >= cols.Length)
        {
            Debug.LogWarning($"[Blackboard] invalid index={currentlyDisplayedIndex}");
            return;
        }


        int totalProgressCount = GameManager.I.progreses.Count;

        int loopCount = Mathf.Min(objectStatusTexts.Count, totalProgressCount);

        for(int i = 0; i < loopCount; i++)
        {
            
            bool isDone = GameManager.I.progreses[i].isClear[currentlyDisplayedIndex];
            objectStatusTexts[i].text = isDone ? "O" : "X";
            objectStatusTexts[i].color = isDone ? Color.green : Color.red;
        }

Debug.Log($"[Board] UpdateStatusDisplay idx={currentlyDisplayedIndex}");
    }

}
