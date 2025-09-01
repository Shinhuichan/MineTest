using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;

public class BoardSwitcher : MonoBehaviour
{
    [Header("Boards")]
    public Transform BoardA;
    public Transform BoardB;
    public float animDuration = 0.7f;
    // 목표 위치
    private Vector3 PosA_Front;
    private Vector3 PosA_Back;
    private Vector3 PosB_Front;
    private Vector3 PosB_Back;

    [Header("Top Tabs")]
    public Button mineral_View_Button;
    public Button mineral_Table_Button;

    [Header("Prev/Next")]
    public Button prev;
    public Button next;

    [Header("Image")]
    public Image image;            // 광물 대표 이미지 (예: 현미경 이미지 or 아이콘)

    [Header("Images")]
    public List<Sprite> pageImages;

    [Header("View UI (간단보기)")]
    public GameObject mineral_View;
    public TextMeshProUGUI viewReactionText;  // 화학반응
    public TextMeshProUGUI viewHardnessText;  // 경도
    public TextMeshProUGUI viewMicroText;     // 현미경
    public TextMeshProUGUI viewConductText;   // 전기전도

    [Header("Table UI (표형식)")]
    public GameObject mineral_Table;

    // 내부 상태
    private bool isBInFront = false;
    private bool isViewTrue = true;
    // 페이지(광물) 인덱싱
    [ReadOnlyInspector] public int currentIndex = 0;
    [ReadOnlyInspector] public List<GameManager.Progress> pages = new List<GameManager.Progress>();

    private void Start()
    {
        // 시작 위치 저장
        PosA_Front = BoardA.position;
        PosB_Back = BoardB.position;
        PosA_Back = PosB_Back;
        PosB_Front = PosA_Front;
        // 탭 버튼
        mineral_View_Button.onClick.AddListener(ViewChange);
        mineral_Table_Button.onClick.AddListener(ViewChange);
        // Prev/Next 버튼
        if (prev) prev.onClick.AddListener(OnPrev);
        if (next) next.onClick.AddListener(OnNext);
        // 탭 초기 상태
        mineral_View.SetActive(isViewTrue);
        mineral_Table.SetActive(!isViewTrue);
        mineral_View_Button.interactable = !isViewTrue;
        mineral_Table_Button.interactable = isViewTrue;
        StartCoroutine(DelayInit());
    }

    private IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => GameManager.I.progreses != null);
        yield return new WaitUntil(() => GameManager.I.progreses.Count > 0);
        // 페이지 데이터 읽기
        LoadPagesFromGameManager();
        // 첫 페이지 표시
        ClampCurrentIndex();
        RefreshPageUI();  // 내용 반영
        RefreshNavInteractable(); // prev/next 활성화 상태
    }
    private void LoadPagesFromGameManager()
    {
        pages.Clear();
        if (GameManager.I != null && GameManager.I.progreses != null)
        {
            pages.AddRange(GameManager.I.progreses);
            // 이름/타입 기준으로 정렬하고 싶다면 아래 주석 해제:
            pages.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        }
        else
        {
            Debug.LogWarning("[BoardSwitcher] GameManager.progreses 를 찾을 수 없습니다.");
        }
    }
    private void ClampCurrentIndex()
    {
        if (pages.Count == 0) { currentIndex = 0; return; }
        currentIndex = Mathf.Clamp(currentIndex, 0, pages.Count - 1);
    }
    private void RefreshNavInteractable()
    {
        if (prev) prev.interactable = (pages.Count > 0 && currentIndex > 0);
        if (next) next.interactable = (pages.Count > 0 && currentIndex < pages.Count - 1);
    }
    public void ViewChange()
    {
        isViewTrue = !isViewTrue;
        mineral_View.SetActive(isViewTrue);
        mineral_Table.SetActive(!isViewTrue);
        mineral_View_Button.interactable = !isViewTrue;
        mineral_Table_Button.interactable = isViewTrue;
        // 탭 전환 시 보드 슬라이드 애니메이션
        OnSwapSlide();
        // 같은 페이지의 다른 레이아웃이므로 내용 갱신
        RefreshPageUI();
        SoundManager.I.PlaySFX("UIClickNext", transform.position, null);
    }
    private void OnPrev()
    {
        if (pages.Count == 0) return;
        currentIndex = Mathf.Max(0, currentIndex - 1);
        RefreshNavInteractable();
        // 페이지 넘길 때 보드 슬라이드
        //OnSwapSlide();
        RefreshPageUI();
        SoundManager.I.PlaySFX("UIClickNext", transform.position, null);
    }
    private void OnNext()
    {
        if (pages.Count == 0) return;
        currentIndex = Mathf.Min(pages.Count - 1, currentIndex + 1);
        RefreshNavInteractable();
        //OnSwapSlide();
        RefreshPageUI();
        SoundManager.I.PlaySFX("UIClickNext", transform.position, null);
    }
    /// <summary>
    /// 기존 보드 전환 애니메이션 재사용
    /// </summary>
    private void OnSwapSlide()
    {
        isBInFront = !isBInFront;
        if (isBInFront)
        {
            BoardA.DOMove(PosA_Back, animDuration).SetEase(Ease.OutQuad);
            BoardB.DOMove(PosB_Front, animDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            BoardA.DOMove(PosA_Front, animDuration).SetEase(Ease.OutQuad);
            BoardB.DOMove(PosB_Back, animDuration).SetEase(Ease.OutQuad);
        }
    }
    /// <summary>
    /// 현재 currentIndex에 해당하는 광물 정보를 UI에 채운다.
    /// ResultUI의 저장 텍스트가 있으면 우선 사용하고, 없으면 OreData 기반 기본 설명을 구성.
    /// </summary>
    public void RefreshPageUI()
    {
        if (pages.Count == 0)
        {
            if (image) image.sprite = null;
            SetAllViewTexts("—");
            return;
        }
        var pr = pages[currentIndex];
        var ore = pr.oreData;

        if (image)
        {
            Sprite s = null;
            if (pageImages != null & pageImages.Count > 0)
            {
                if (currentIndex >= 0 && currentIndex < pageImages.Count)
                {
                    s = pageImages[currentIndex];
                }
            }

            image.sprite = s;
            image.enabled = (s != null);
        }
        // 4개 실험 텍스트(0~3): 화학반응/경도/현미경/전도 — 실험 잠금 처리

        string t0 = GetBoardOrFallback(ore, 0, () => BuildChemicalText(ore));
        string t1 = GetBoardOrFallback(ore, 1, () => BuildChemicalText(ore));
        string t2 = GetBoardOrFallback(ore, 2, () => BuildChemicalText(ore));
        string t3 = GetBoardOrFallback(ore, 3, () => BuildChemicalText(ore));

        // string t0 = pr.isClear[0]
        //     ? GetBoardOrFallback(ore, 0, () => BuildChemicalText(ore))
        //     : "-";
        // string t1 = pr.isClear[1]
        //     ? GetBoardOrFallback(ore, 1, () => BuildHardnessText(ore))
        //     : "-";
        // string t2 = pr.isClear[2]
        //     ? GetBoardOrFallback(ore, 2, () => BuildMicroText(ore))
        //     : "-";
        // string t3 = pr.isClear[3]
        //     ? GetBoardOrFallback(ore, 3, () => BuildConductText(ore))
        //     : "-";

        // View 레이아웃
        if (viewReactionText) viewReactionText.text = t0;
        if (viewHardnessText) viewHardnessText.text = t1;
        if (viewMicroText) viewMicroText.text = t2;
        if (viewConductText) viewConductText.text = t3;
        // 썸네일 밝기: 해당 광물에서 하나도 완료 안 됐으면 흐리게
        bool anyCleared = pr.isClear[0] || pr.isClear[1] || pr.isClear[2] || pr.isClear[3];
        if (image) image.color = anyCleared ? Color.white : new Color(1f, 1f, 1f, 0.4f);

        // Debug.Log("1...." + viewReactionText.text);
        // Debug.Log("2...." + t0);

    }
    private void SetAllViewTexts(string s)
    {
        if (viewReactionText) viewReactionText.text = s;
        if (viewHardnessText) viewHardnessText.text = s;
        if (viewMicroText) viewMicroText.text = s;
        if (viewConductText) viewConductText.text = s;
    }

    private string GetBoardOrFallback(OreData ore, int experimentNumber, Func<string> fallbackBuilder)
    {
        string fromBoard = "";
        if (GameManager.I != null)
        {
            fromBoard = GameManager.I.GetBoardText(ore, experimentNumber);
        }
        if (!string.IsNullOrEmpty(fromBoard)) return fromBoard;
        return fallbackBuilder != null ? fallbackBuilder() : "";
    }
    // ------- Fallback 문구 생성기 (ResultUI 텍스트가 없을 때) -------
    private string BuildChemicalText(OreData ore)
    {
        return "-";
        // // isReactingToChem 플래그 해석
        // var reacts = ore.isReactingToChem;
        // bool w = (reacts & ChemicalType.Water) != 0;
        // bool a = (reacts & ChemicalType.Acid) != 0;
        // if (!w && !a) return "반응 없음";
        // if (w && a) return "물/산 모두 반응";
        // if (w) return "물에 반응";
        // return "산에 반응";
    }
    private string BuildHardnessText(OreData ore)
    {
        return "-";
        //return $"{ore.hardness:0.0}";
    }
    private string BuildMicroText(OreData ore)
    {
        return ore.microShape ? "현미경: 표본 이미지 확인됨" : "현미경: 표본 이미지 없음";
    }
    private string BuildConductText(OreData ore)
    {
        string c = ore.electroConduct ? "O" : "X";
        string tx = ore.isToxicElements ? " / O" : "X";
        return $"전기전도: {c}{tx}";
    }
}