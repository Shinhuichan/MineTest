using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BoardSwicher : MonoBehaviour
{
    public Transform BoardA;
    public Transform BoardB;

    public float animDuration = 0.7f;
    //목표 위치를 저장할 변수
    public Vector3 PosA_Front;
    public Vector3 PosA_Back;
    public Vector3 PosB_Front;
    public Vector3 PosB_Back;

    [Header("Buttons")]
    public Button mineral_View_Button;
    public Button mineral_Table_Button;
    public Button prev;
    public Button next;

    [Header("UIs")]
    public GameObject mineral_View;
    public GameObject mineral_Table;

    private bool isBInFront = false;
    private bool isViewTrue = true;

    private void Start()
    {
        // 시작할때의 위치를 기반으로 앞 ,뒤 위치를 정의
        PosA_Front = BoardA.position;
        PosB_Back = BoardB.position;

        // 서로의 위치를 목표 지접으로 설정
        PosA_Back = PosB_Back;
        PosB_Front = PosA_Front;

        mineral_View_Button.onClick.AddListener(ViewChange);
        mineral_Table_Button.onClick.AddListener(ViewChange);

        mineral_View.SetActive(isViewTrue);
        mineral_Table.SetActive(!isViewTrue);

        mineral_View_Button.interactable = !isViewTrue;
        mineral_Table_Button.interactable = isViewTrue;
    }

    public void ViewChange()
    {
        isViewTrue = !isViewTrue;

        mineral_View.SetActive(isViewTrue);
        mineral_Table.SetActive(!isViewTrue);

        mineral_View_Button.interactable = !isViewTrue;
        mineral_Table_Button.interactable = isViewTrue;
        OnSwapButtonClick();
    }

    public void OnSwapButtonClick()
    {
        isBInFront = !isBInFront;
        if(isBInFront)
        {
            // B를 앞으로, A를 뒤로 보냄
            //.SetEase()를 통해 부드러운 움직임 종류를 선택 할 수 있습니다.

            BoardA.DOMove(PosA_Back, animDuration).SetEase(Ease.OutQuad);
            BoardB.DOMove(PosB_Front, animDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            // A를 앞으로, B를 뒤로 보냄
            BoardA.DOMove(PosA_Front, animDuration).SetEase(Ease.OutQuad);
            BoardB.DOMove(PosB_Back, animDuration).SetEase(Ease.OutQuad);
        }
    }
}
