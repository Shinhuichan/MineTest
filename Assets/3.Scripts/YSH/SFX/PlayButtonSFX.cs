using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButtonSFX : MonoBehaviour
{
    [System.Serializable]
    public class ButtonSFX
    {
        public Button button;       // 연결할 버튼
        public string sfxName;      // 버튼 클릭 시 재생할 SFX 이름
    }

    [SerializeField] private ButtonSFX[] buttonSfxList; // 여러 버튼 관리

    void Start()
    {
        foreach (var entry in buttonSfxList)
        {
            if (entry.button != null)
            {
                string sfxNameCopy = entry.sfxName; // 클로저 문제 방지
                entry.button.onClick.AddListener(() => PlaySFX(sfxNameCopy));
            }
        }
    }

    private void PlaySFX(string sfxName)
    {
        if (SoundManager.I != null)
        {
            SoundManager.I.PlaySFX(sfxName, Vector3.zero);
            Debug.Log($"{sfxName} 실행 완료");
        }
        else
        {
            Debug.LogWarning("SoundManager instance not found!");
        }
    }
}
