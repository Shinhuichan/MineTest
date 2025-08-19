using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMesh nameText;
    public TextMesh contentText;
    public TextMesh choiceText; // 현재 선택지 표시
    public Button leftButton;
    public Button rightButton;
    public Button confirmButton;

    private Dictionary<string, DialogueLine> dialogueDict = new Dictionary<string, DialogueLine>();
    private DialogueLine currentLine;
    private int choiceIndex = 0;

    void Start()
    {
        LoadCSV();
        // Start에서 바로 ShowLine 호출 제거
        SetupButtons();

        // 처음에는 UI 숨김
        nameText.gameObject.SetActive(false);
        contentText.gameObject.SetActive(false);
        choiceText.gameObject.SetActive(false);
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
    }

    public void ActivateDialogue()
    {
        gameObject.SetActive(true); // DialogueManager 활성화

        // UI 요소 전체 켜기
        nameText.gameObject.SetActive(true);
        contentText.gameObject.SetActive(true);
        choiceText.gameObject.SetActive(true);
        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);

        ShowLine("1"); // 시작 ID
    }

    public void HideDialogue()
    {
        // UI 숨기기
        nameText.gameObject.SetActive(false);
        contentText.gameObject.SetActive(false);
        choiceText.gameObject.SetActive(false);
        leftButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);

        // 필요 시 코루틴 정지
        StopAllCoroutines();
    }

    void LoadCSV()
    {
        TextAsset csvData = Resources.Load<TextAsset>("MINETEST_Dialogue"); // 확장자 없이 경로만

        if (csvData == null)
        {
            Debug.LogError("Dialogue CSV 파일을 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvData.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        // 헤더는 ID, ParentID, Character, Content, NextID, Choice1, Choice2, ...
        for (int i = 1; i < lines.Length; i++) // 첫 줄 헤더 스킵
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');

            if (cols.Length < 5)
            {
                Debug.LogWarning($"CSV 데이터가 부족합니다. {line}");
                continue;
            }

            DialogueLine dialogueLine = new DialogueLine
            {
                ID = cols[0].Trim(),
                ParentID = cols[1].Trim(),
                Character = cols[2].Trim(),
                // ContentText 내 ' (backtick) 문자를 \n으로 변환해서 UI에 출력할 때 사용
                ContentText = (cols[3].Trim().ToLower() == "none" ? "" : cols[3].Trim()),
                NextID = cols[4].Trim().ToLower() == "none" ? "" : cols[4].Trim(),
                Choices = new List<string>()
            };

            for (int c = 5; c < cols.Length; c++)
            {
                string choiceId = cols[c].Trim();
                if (!string.IsNullOrEmpty(choiceId) && choiceId.ToLower() != "none")
                    dialogueLine.Choices.Add(choiceId);
            }

            dialogueDict[dialogueLine.ID] = dialogueLine;
        }
    }

    void SetupButtons()
    {
        leftButton.onClick.AddListener(() =>
        {
            if (currentLine.Choices.Count > 0)
            {
                choiceIndex = (choiceIndex - 1 + currentLine.Choices.Count) % currentLine.Choices.Count;
                UpdateChoiceText();
            }
        });

        rightButton.onClick.AddListener(() =>
        {
            if (currentLine.Choices.Count > 0)
            {
                choiceIndex = (choiceIndex + 1) % currentLine.Choices.Count;
                UpdateChoiceText();
            }
        });

        confirmButton.onClick.AddListener(() =>
        {
            if (currentLine.Choices.Count > 0)
            {
                string selectedChoice = currentLine.Choices[choiceIndex];
                ShowLine(selectedChoice);
            }
        });
    }

    void ShowLine(string id)
    {
        if (!dialogueDict.ContainsKey(id))
        {
            Debug.LogWarning($"대사 ID '{id}'가 존재하지 않습니다.");
            return;
        }

        StopAllCoroutines();

        currentLine = dialogueDict[id];
        nameText.text = currentLine.Character;
        // ' (backtick) 를 줄바꿈 문자로 교체하여 UI에 출력
        contentText.text = currentLine.ContentText.Replace('`', '\n');

        if (currentLine.Choices.Count > 0)
        {
            choiceIndex = 0;
            UpdateChoiceText();

            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
            confirmButton.gameObject.SetActive(true);
            choiceText.gameObject.SetActive(true);
        }
        else
        {
            choiceText.text = "";
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            confirmButton.gameObject.SetActive(false);
            choiceText.gameObject.SetActive(false);

            StartCoroutine(AutoNextLine());
        }
    }

    void UpdateChoiceText()
    {
        if (currentLine.Choices.Count > 0)
            choiceText.text = currentLine.Choices[choiceIndex];
        else
            choiceText.text = "";
    }

    IEnumerator AutoNextLine()
    {
        yield return new WaitForSeconds(3f);

        if (!string.IsNullOrEmpty(currentLine.NextID))
        {
            ShowLine(currentLine.NextID);
        }
        else
        {
            Debug.Log("대화가 종료되었습니다.");
        }
    }
}
