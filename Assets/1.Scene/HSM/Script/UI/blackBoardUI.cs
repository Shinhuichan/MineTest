using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class blackBoardUI : MonoBehaviour
{
    //텍스트 컴포넌트를 연결할 변수
    public TMP_Text statusText;

    [System.Serializable]
    public class TextRow
    {
        public TMP_Text[] colums;
    }
    public TextRow[] rows; //텍스트 컴포넌트를 담을 배열

    public int totalExperiments = 4;
    public int objectPerExperiment = 3;
    //데이터 저장은 bool 2차원 배열로 관리
    private bool[,] experimentsStatus;

    private void Start()
    {
        experimentsStatus = new bool[totalExperiments, objectPerExperiment];

        InitializeDisplay();
    }

    /// <summary>
    /// 게임 시작 시 모든 텍스트를 초기상태로 설정
    /// </summary>
    
    private void InitializeDisplay()
    {
        for(int i =0; i<totalExperiments; i++)
        {
            for (int j =0; j<objectPerExperiment; j++)
            {
                UpdateCell(i, j, false);
            }
        }
    }
    /// <summary>
    /// * 특정 실험의 특정 오브젝트가 완료되었을 때 외부에서 호출하는 함수
    /// </summary>
    /// <param name="experimentIndex">완료된 실험의 인덱스(0~3) </param>
    /// <param name="objectIndex">완료된 오브젝트의 인덱스(0~2) </param>

    public void MarkAsCompleted(int experimentIndex, int objectIndex)
    {
        if(experimentIndex >= totalExperiments || objectIndex >= objectPerExperiment)
        {
            Debug.LogError("잘못된 실험또는 오브젝트 인덱스 입니다.");
            return;
        }
        // 데이터 상태 변경
        experimentsStatus[experimentIndex, objectIndex] = true;
        // 데이터 변경되었으니, 해당 셀의 UI만 업데이트
        UpdateCell(experimentIndex, objectIndex, true);
    }

    public void UpdateCell(int row, int col, bool isCompleted)
    {
        if(isCompleted)
        {
            rows[row].colums[col].text = " O ";
            rows[row].colums[col].color = Color.green;
        }
        else
        {
            rows[row].colums[col].text = " X ";
            rows[row].colums[col].color = Color.red;
        }
    }
}
