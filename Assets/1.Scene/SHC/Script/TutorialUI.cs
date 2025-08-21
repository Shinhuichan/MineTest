using CustomInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum IntroduceType
{
    Image,
    Video
}

[System.Serializable]
public struct Introduce
{
    public IntroduceType type;
    public Sprite sprite;
    public VideoClip videoClip;
    [TextArea] public string introduceText;
}

public class TutorialUI : MonoBehaviour
{
    public Introduce[] introduces;

    [Header("Show Component Setup")]
    public Image imageUI;
    public VideoPlayer videoPlayer;
    public TextMesh textUI;

    [Header("Button Setup")]
    [SerializeField] GameObject prevButton;
    [SerializeField] GameObject nextButton;
    [SerializeField] GameObject nextSceneButton;

    [Header("UI Show Index")]
    [SerializeField, ReadOnly] private int currentIndex = 0;

    private void Start()
    {
        ShowIntroduce(currentIndex);
    }

    private void IndexCheck()
    {
        if (currentIndex.Equals(0)) prevButton.SetActive(false);
        else prevButton.SetActive(true);
        if (currentIndex >= introduces.Length - 1)
        {
            nextButton.SetActive(false);
            nextSceneButton.SetActive(true);
        }
        else
        {
            nextButton.SetActive(true);
            nextSceneButton.SetActive(false);
        }
    }
        
    public void ShowIntroduce(int index)
    {
        if (index < 0 || index >= introduces.Length) return;

        Introduce current = introduces[index];
        textUI.text = current.introduceText;

        // Type이 Image인 경우
        if (current.type == IntroduceType.Image)
        {
            imageUI.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(false);

            imageUI.sprite = current.sprite;
        }
        // Type이 Video인 경우
        else if (current.type == IntroduceType.Video)
        {
            imageUI.gameObject.SetActive(false);
            videoPlayer.gameObject.SetActive(true);

            videoPlayer.clip = current.videoClip;
            videoPlayer.Play();
        }
    }

    #region UI 전환 메서드
    public void Next()
    {
        currentIndex++;
        if (currentIndex >= introduces.Length) currentIndex = 0;
        ShowIntroduce(currentIndex);
        IndexCheck();
    }

    public void Prev()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = introduces.Length - 1;
        ShowIntroduce(currentIndex);
        IndexCheck();
    }
    
    #endregion
}