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

    private int currentlyDisplayedIndex = -1;

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
        if(experimentIndex < 0 || experimentIndex >= experimentNames.Count)
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
    }

    public void UpdateStatusDisplay()
    {
        if (GameManager.I == null || GameManager.I.progreses == null) return;

        int totalProgressCount = GameManager.I.progreses.Count;

        int loopCount = Mathf.Min(objectStatusTexts.Count, totalProgressCount);

        for(int i = 0; i < loopCount; i++)
        {
            // i��° ������ ���� ǥ�� ���� ������ �Ϸ� ���� Ȯ��
            bool isDone = GameManager.I.progreses[i].isClear[currentlyDisplayedIndex];

            if(isDone)
            {
                objectStatusTexts[i].text = "O";
                objectStatusTexts[i].color = Color.green;
            }
            else
            {
                objectStatusTexts[i].text = "X";
                objectStatusTexts[i].color = Color.red;
            }
        }

    }

}
