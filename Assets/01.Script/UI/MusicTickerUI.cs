using UnityEngine;
using TMPro;

public class MusicTickerUI : MonoBehaviour
{
    [Header("텍스트")]
    [SerializeField] private TMP_Text musicText;
    [Tooltip("스크롤 속도 (px/sec, Canvas 기준)")]
    [SerializeField] private float scrollSpeed = 80f;
    [Tooltip("텍스트가 완전히 나간 뒤 다시 오른쪽에서 나오기까지 여백")]
    [SerializeField] private float gap = 40f;

    private RectTransform textRect;
    private Canvas rootCanvas;
    private float textWidth;
    private string lastBgmName;
    private bool initialized;

    private void Awake()
    {
        textRect   = musicText.GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        while (rootCanvas != null && !rootCanvas.isRootCanvas)
        {
            rootCanvas = rootCanvas.transform.parent.GetComponentInParent<Canvas>();
        }
    }

    private void Update()
    {
        if (!initialized)
        {
            initialized = true;
            RefreshText();
            ResetToRightEdge();
            return;
        }

        string current = SoundManager.Instance != null ? SoundManager.Instance.GetCurrentBgmName() : "";
        if (current != lastBgmName)
        {
            lastBgmName = current;
            RefreshText();
            ResetToRightEdge();
            return;
        }

        textRect.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

        // 텍스트가 완전히 왼쪽으로 나가면 오른쪽 끝으로 리셋
        if (textRect.anchoredPosition.x + textWidth < 0f)
        {
            ResetToRightEdge();
        }
    }

    private void RefreshText()
    {
        string name = SoundManager.Instance != null ? SoundManager.Instance.GetCurrentBgmName() : "";
        musicText.text = string.IsNullOrEmpty(name) ? "" : "♪  " + name;

        musicText.ForceMeshUpdate();
        textWidth = musicText.preferredWidth;
    }

    private void ResetToRightEdge()
    {
        // Canvas 스케일을 반영한 화면 오른쪽 끝 좌표
        float canvasWidth = rootCanvas != null ? rootCanvas.GetComponent<RectTransform>().rect.width : Screen.width;

        textRect.anchoredPosition = new Vector2(canvasWidth + gap, textRect.anchoredPosition.y);
    }
}
