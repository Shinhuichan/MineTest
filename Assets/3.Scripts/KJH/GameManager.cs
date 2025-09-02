using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
[System.Serializable]
public struct LaboratoryAccident
{
    public string accidentName;
    public int accidentWeight;
}
public class GameManager : SingletonBehaviour<GameManager>
{
    protected override bool IsDontDestroy() => true;
    public List<LaboratoryAccident> accidents = new List<LaboratoryAccident>();
    public List<Progress> progreses = new List<Progress>();
    //ActionBasedController[] controllers;
    [ReadOnlyInspector][SerializeField] ResultUI resultUI;

    [SerializeField] blackBoardUI blackboardUI;                    // ***UI용 추가**
    [ReadOnlyInspector][SerializeField] BoardSwitcher boardSwitcher;
    [SerializeField] FinalTestManager finalTestManager;
    [SerializeField] private SequenceController sequenceController;
    // 현재 플레이어가 진행해야 할 실험의 단계를 저장하는 변수
    private int currentActiveExperimentIndex = 0;                  // ***UI용 추가**

    private bool useBlackboardUI = true;
    private bool useFinalTestManager = true;

    [System.Serializable]
    public class Progress
    {
        public string Name;
        public Transform transform;
        public OreData oreData;
        public bool[] isClear = new bool[4];
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void Start()
    {
        Init();
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        Init();
    }

    public void Init()
    {
        resultUI = FindAnyObjectByType<ResultUI>(FindObjectsInactive.Include);
        boardSwitcher = FindAnyObjectByType<BoardSwitcher>(FindObjectsInactive.Include);
        xROrigin = FindAnyObjectByType<XROrigin>(FindObjectsInactive.Include);
        if (useBlackboardUI)
        {
            blackboardUI = FindFirstObjectByType<blackBoardUI>(FindObjectsInactive.Include);
            if (blackboardUI == null && blackboardUI != null)
            {
                blackboardUI = Instantiate(blackboardUI);
            }
        }
        else blackboardUI = null;

        if (useFinalTestManager)
        {
            finalTestManager = FindFirstObjectByType<FinalTestManager>(FindObjectsInactive.Include);
            if (finalTestManager == null && finalTestManager != null)
            {
                finalTestManager = Instantiate(finalTestManager);
            }
        }
        else finalTestManager = null;

        camera = Camera.main;
        accidents.Clear();
        ObjectInfo[] temp = FindObjectsByType<ObjectInfo>(FindObjectsSortMode.None);
        List<ObjectInfo> list = temp.ToList();
        list.Sort((a, b) => int.Parse((a.transform.name).Split("Ore")[1]).CompareTo(int.Parse((b.transform.name).Split("Ore")[1])));
        temp = list.ToArray();
        progreses.Clear();
        for (int i = 0; i < temp.Length; i++)
        {
            Progress pr = new Progress();
            Array.Fill(pr.isClear, false);
            pr.Name = temp[i].transform.name;
            pr.transform = temp[i].transform;
            pr.oreData = temp[i].oreData;
            progreses.Add(pr);
            //Debug.Log(pr.transform.name);
        }
        if (blackboardUI != null)
        {
            //Debug.Log(currentActiveExperimentIndex);
            blackboardUI.ShowExperimentStatus(currentActiveExperimentIndex);
        }
    }
    public void Clear(OreData oreData, int experimentNumber, string boardText)
    {
        if (progreses.Count == 0) return;
        if (experimentNumber < 0 || experimentNumber > 3)
        {
            Debug.Log($"experimentNumber는 0(화학반응) , 1(경도) , 2(현미경) , 3(전기전도) 들만 가능합니다. ( {experimentNumber} ) ");
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

        // ----- 칠판 UI 업데이트 ---------
        if (blackboardUI != null)
        {
            // f현재 보고 있는 화면의 O/X상태를 갱신
            blackboardUI.UpdateStatusDisplay();
        }
        // 방금 완료한 실험의 모든 오브젝트가 체크되었는지 확인
        CheckForFullExperimentCompletion(experimentNumber);


        // // 햅틱 반응
        // if (controllers != null)
        // {
        //     foreach (var ctrl in controllers)
        //     {
        //         ctrl.SendHapticImpulse(0.5f, 0.2f);
        //     }
        // }

        Debug.Log($"광물 {oreData.type.ToString()}로 실험{experimentNumber}을 완료했습니다.");
        if (resultUI != null)
        {
            resultUI.ShowText(oreData, experimentNumber, boardText);
        }
        boardSwitcher.RefreshPageUI();
    }
    /// <summary>
    /// 특정 실험의 모든 오브젝트가 완료되었는지 확인하고, 완료되었다면 다음 실험으로 넘어가는 함수
    /// </summary>
    public void PrepareNextExperimentUI()
    {
        if (blackboardUI != null)
        {
            Debug.Log($"대화종료. 다음실험 ({currentActiveExperimentIndex})UI를 표시합니다.");
            blackboardUI.ShowExperimentStatus(currentActiveExperimentIndex);
        }
        Debug.Log($"[GM.NextUI] show index={currentActiveExperimentIndex}");
    }

    private void CheckForFullExperimentCompletion(int experimentNumber)
    {
        //현재 진행중인 실험이 아니면 체크할 필요 없음
        if (experimentNumber != currentActiveExperimentIndex) return;

        bool allObjectsCleared = true;

        foreach (var progress in progreses)
        {
            if (!progress.isClear[experimentNumber])
            {
                allObjectsCleared = false;
                break; //하나라도 미완이 있음 루프 중단
            }
        }
        // 모든 오브젝트에 대한 실험을 완료했다면
        if (allObjectsCleared)
        {
            Debug.Log($"실험 {experimentNumber + 1}의 과제를 완료했습니다.다음 실험으로 넘어가겠습니다.");

            // 다음 실험 인덱스로 변경
            currentActiveExperimentIndex++;
            int totalExperiments = progreses[0].isClear.Length;



            if (currentActiveExperimentIndex < totalExperiments)
            {
                if (sequenceController != null)
                {
                    sequenceController.TriggerTestEndDialogue(experimentNumber);
                }
            }
            else
            {
                // 모든 실험 완료! 테스트 모드로 전환
                Debug.Log("모든 실험을 완료했습니다!");
                if (blackboardUI != null)
                {
                    blackboardUI.ShowTestView();
                }
            }
        }
    }
    // 최종 정답을 확인하는 함수
    public void CheckFinalAnswer(OreData[] submittedOreData)
    {
        int correctCount = 0;
        int totalQuestions = progreses.Count;

        for (int i = 0; i < totalQuestions; i++)
        {
            if (submittedOreData[i] == null) continue;

            OreData correctAnswerData = progreses[i].oreData;

            if (submittedOreData[i].type == correctAnswerData.type)
            {
                correctCount++;
            }
        }

        Debug.Log($"맞은 개수 : {correctCount} / {totalQuestions}");

        if (finalTestManager != null)
        {
            finalTestManager.ShowFeedback(correctCount, totalQuestions);
        }
        // 만약 모두 맞았다면, 게임 종료 로직을 호출합니다.
        if (correctCount == totalQuestions)
        {
            Invoke("EndGame", 2f);
        }

    }
    public void EndGame()
    {
        Debug.Log("게임 클리어");

        // 게임 클리어시 필요한 로직
    }

    public void EditBoardText(OreData oreData, int experimentNumber, string boardText)
    {
        resultUI.ShowText(oreData, experimentNumber, boardText);
        boardSwitcher.RefreshPageUI();
    }
    public string GetBoardText(OreData oreData, int experimentNumber)
    {
        return resultUI.GetText(oreData, experimentNumber);
    }

    public bool IsCurrentTestClear(int experimentNumber)
    {
        if (experimentNumber < 0 || experimentNumber > 3) return false;

        int total = progreses.Count;
        int clear = 0;
        foreach (var pr in progreses)
            if (pr.isClear[experimentNumber]) clear++;

        // 모든 광석이 실험이 됐는지 확인.
        // Debug.Log($"실제로 한 실험 : {clear} == 해야되는 실험 : {total}");
        return clear == total;
    }
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(nameof(ChangeScene_co1), sceneName);
    }
    IEnumerator ChangeScene_co1(string sceneName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        while (true)
        {
            yield return null;
            if (ao.isDone) break;
        }
        yield return new WaitForSeconds(0.5f);
        Init();
    }
    public void ChangeScene(int sceneIndex)
    {
        StartCoroutine(nameof(ChangeScene_co2), sceneIndex);
    }
    IEnumerator ChangeScene_co2(int sceneIndex)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneIndex);
        while (true)
        {
            yield return null;
            if (ao.isDone) break;
        }
        yield return new WaitForSeconds(0.5f);
        Init();
    }
    XROrigin xROrigin;
    public void StopPlayer()
    {
        Transform loco = xROrigin.transform.Find("Locomotion System");
        loco.Find("Turn").gameObject.SetActive(false);
        loco.Find("Move").gameObject.SetActive(false);
        xROrigin.transform.Find("Camera Offset/Left Controller").gameObject.SetActive(false);
        xROrigin.transform.Find("Camera Offset/Left Controller Stabilized").gameObject.SetActive(false);
        xROrigin.transform.Find("Camera Offset/Right Controller").gameObject.SetActive(false);
        xROrigin.transform.Find("Camera Offset/Right Controller Stabilized").gameObject.SetActive(false);
        // TrackedPoseDriver tpd = xROrigin.transform.Find("Camera Offset/Main Camera").GetComponent<TrackedPoseDriver>();
        // tpd.enabled = false;
    }
    public void ResumeHand()
    {
        xROrigin.transform.Find("Camera Offset/Left Controller").gameObject.SetActive(true);
        xROrigin.transform.Find("Camera Offset/Left Controller Stabilized").gameObject.SetActive(true);
        xROrigin.transform.Find("Camera Offset/Right Controller").gameObject.SetActive(true);
        xROrigin.transform.Find("Camera Offset/Right Controller Stabilized").gameObject.SetActive(true);
    }
    public void ResumePlayer()
    {
        Transform loco = xROrigin.transform.Find("Locomotion System");
        loco.Find("Turn").gameObject.SetActive(true);
        loco.Find("Move").gameObject.SetActive(true);
        xROrigin.transform.Find("Camera Offset/Left Controller").gameObject.SetActive(true);
        xROrigin.transform.Find("Camera Offset/Left Controller Stabilized").gameObject.SetActive(true);
        xROrigin.transform.Find("Camera Offset/Right Controller").gameObject.SetActive(true);
        xROrigin.transform.Find("Camera Offset/Right Controller Stabilized").gameObject.SetActive(true);
        // TrackedPoseDriver tpd = xROrigin.transform.Find("Camera Offset/Main Camera").GetComponent<TrackedPoseDriver>();
        // tpd.enabled = true;
    }
    Camera camera;
    public void LookTarget(Transform target)
    {
        StopCoroutine(nameof(LookTarget_co));
        StartCoroutine(nameof(LookTarget_co), target.position);
    }
    public void LookTarget(Vector3 targetPosition)
    {
        StopCoroutine(nameof(LookTarget_co));
        StartCoroutine(nameof(LookTarget_co), targetPosition);
    }
    IEnumerator LookTarget_co(Vector3 targetPos)
    {
        Transform camTr = Camera.main.transform;
        float startTime = Time.time;
        //DebugExtension.DebugWireSphere(targetPos, Color.blue, 0.2f, 20f, true);
        //DebugExtension.DebugWireSphere(camTr.position, Color.yellow, 0.2f, 20f, true);
        //Debug.DrawLine(targetPos, camTr.position, Color.blue, 20f, true);
        //Debug.DrawRay(camTr.position, 10f * camTr.forward, Color.yellow, 20f);
        Vector3 forwardXZ = camTr.forward;
        forwardXZ.y = 0f;
        forwardXZ.Normalize();
        Vector3 targetDirXZ = targetPos - camTr.position;
        targetDirXZ.y = 0f;
        targetDirXZ.Normalize();
        float angle = Vector3.SignedAngle(forwardXZ, targetDirXZ, Vector3.up);
        //Debug.Log($"각도 : {angle}");
        while (Time.time - startTime < 3f)
        {
            forwardXZ = camTr.forward;
            forwardXZ.y = 0f;
            forwardXZ.Normalize();
            targetDirXZ = targetPos - camTr.position;
            targetDirXZ.y = 0f;
            targetDirXZ.Normalize();
            angle = Vector3.SignedAngle(forwardXZ, targetDirXZ, Vector3.up);
            if (angle > 5 && angle <= 180)
            {
                xROrigin.RotateAroundCameraPosition(Vector3.up, 150f * Time.deltaTime);
            }
            else if (angle >= -180 && angle <= -5)
            {
                xROrigin.RotateAroundCameraPosition(Vector3.up, -150f * Time.deltaTime);
            }
            else
            {
                break;
            }
            yield return null;
        }
    }






}