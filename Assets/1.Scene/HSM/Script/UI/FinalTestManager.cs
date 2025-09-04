using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class FinalTestManager : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private XRSocketInteractor[] answerSockets;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackText;

    private XRInteractionManager interactionManager;
    private bool isCheckingAnswer = false;

    // ---- 주차 정보 ----
    private struct ParkInfo
    {
        public XRSocketInteractor socket;
        public IXRSelectInteractable interactable;
        public Transform root;              // ← Rigidbody/Grab이 붙은 루트
        public Transform originalParent;
        public bool hadRigidbody;
        public Rigidbody rb;
        public bool rbWasKinematic;
        public XRGrabInteractable grab;
        public bool grabWasEnabled;
    }
    private readonly List<ParkInfo> _parked = new List<ParkInfo>();

    void Start()
    {
        interactionManager = FindObjectOfType<XRInteractionManager>();
        foreach (var s in answerSockets) s.selectEntered.AddListener(OnSocketFilled);
        if (feedbackPanel) feedbackPanel.SetActive(false);
    }

    void OnDestroy()
    {
        foreach (var s in answerSockets) s.selectEntered.RemoveListener(OnSocketFilled);
    }

    void OnSocketFilled(SelectEnterEventArgs args)
    {
        if (isCheckingAnswer) return;

        bool full = true;
        foreach (var s in answerSockets) { if (!s.hasSelection) { full = false; break; } }
        if (full)
        {
            isCheckingAnswer = true;
            checkAnswer();
        }
    }

    public void checkAnswer()
    {
        OreData[] submitted = new OreData[answerSockets.Length];
        for (int i = 0; i < answerSockets.Length; i++)
        {
            var it = answerSockets[i].GetOldestInteractableSelected();
            if (it is Component comp)
            {
                var sheet = comp.GetComponent<answerSheet>();
                if (sheet) submitted[i] = sheet.associatedOreData;
            }
        }
        GameManager.I.CheckFinalAnswer(submitted);
    }

    public void ShowFeedback(int correct, int total)
    {
        if (!feedbackPanel || !feedbackText) return;
        feedbackText.text = (correct == total) ? "정답입니다!" : $"다시 한번 생각해보세요.\n정답 ({correct} / {total})";
        StartCoroutine(FeedbackDisplayRoutine());
        if (correct < total) StartCoroutine(ResetTestRoutine());
    }

    IEnumerator FeedbackDisplayRoutine()
    {
        feedbackPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        feedbackPanel.SetActive(false);
    }

    IEnumerator ResetTestRoutine()
    {
        yield return new WaitForSeconds(2f);

        var list = new List<answerSheet>();
        foreach (var s in answerSockets)
        {
            if (!s.hasSelection) continue;
            var it = s.GetOldestInteractableSelected();
            if (it is Component comp)
            {
                var sheet = comp.GetComponent<answerSheet>();
                if (sheet) list.Add(sheet);
            }
        }
        foreach (var sh in list) sh.ForceCancelInteraction();
        yield return null;
        foreach (var sh in list) sh.StartReturnToPositionRoutine();

        isCheckingAnswer = false;
    }

    // ====== 여기부터: 탭 전환용 주차/복귀 ======

    /// <summary>
    /// Test 탭을 떠나기 직전 호출:
    /// 선택물을 소켓의 attachTransform(없으면 socket.transform) 밑으로 붙이고,
    /// Rigidbody를 kinematic, Grab을 비활성화해서 떨어지지 않게 '정지'시킨다.
    /// </summary>
    public void ParkSelectionsToSocket()
    {
        _parked.Clear();

        foreach (var socket in answerSockets)
        {
            if (socket == null || !socket.hasSelection) continue;

            var it = socket.GetOldestInteractableSelected();
            if (it is not Component comp) continue;

            var grab = comp.GetComponentInParent<XRGrabInteractable>();
            Transform root = grab ? grab.transform : (comp.GetComponentInParent<Rigidbody>()?.transform ?? comp.transform);
            var rb = root.GetComponent<Rigidbody>();

            var info = new ParkInfo
            {
                socket = socket,
                interactable = it,
                root = root,
                originalParent = root.parent,
                hadRigidbody = rb != null,
                rb = rb,
                rbWasKinematic = rb ? rb.isKinematic : false,
                grab = grab,
                grabWasEnabled = grab ? grab.enabled : false
            };

            // 물리 정지
            if (rb)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            // Grab 잠시 비활성 (비활성화 과정에서 이동/해제가 끼어들지 않게)
            if (grab) grab.enabled = false;

            // 소켓의 attach 밑으로 붙이고 정확히 붙여둔다
            Transform parent = socket.attachTransform ? socket.attachTransform : socket.transform;
            root.SetParent(parent, false);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;

            _parked.Add(info);
        }
    }

    /// <summary>
    /// Test 탭으로 돌아온 뒤 호출:
    /// Grab/물리 상태를 원래대로 복구한다.
    /// (부모를 다시 원래로 돌리고 싶으면 restoreOriginalParent=true)
    /// </summary>
    public void UnparkSelections(bool restoreOriginalParent = false)
    {
        foreach (var p in _parked)
        {
            if (!p.root) continue;

            if (restoreOriginalParent && p.originalParent)
                p.root.SetParent(p.originalParent, false);

            if (p.hadRigidbody && p.rb)
                p.rb.isKinematic = p.rbWasKinematic;

            if (p.grab) p.grab.enabled = p.grabWasEnabled;
        }
        _parked.Clear();
    }
}