using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Sentences")]
    [TextArea(2, 5)]
    [SerializeField] private string[] sentences;

    [Header("Sentence Range - 처음 시작 (0~2)")]
    [SerializeField] private int startTutorialFrom = 0;
    [SerializeField] private int startTutorialTo = 2;

    [Header("Sentence Range - 천만원 돌파 (3~)")]
    [SerializeField] private int millionTutorialFrom = 3;
    [SerializeField] private int millionTutorialTo = 3;

    [Header("Sentence Range - 진화아이템 경고")]
    [SerializeField] private int evolveWarningTutorialFrom = 4;
    [SerializeField] private int evolveWarningTutorialTo = 4;

    [Header("Sentence Range - 15강 달성")]
    [SerializeField] private int maxLevelTutorialFrom = 5;
    [SerializeField] private int maxLevelTutorialTo = 5;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool alwaysShowOnStart = false;
    [SerializeField] private bool alwaysShowOnMillion = false;
    [SerializeField] private bool alwaysShowOnEvolveWarning = false;
    [SerializeField] private bool alwaysShowOnMaxLevel = false;

    private int currentIndex;
    private int endIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;

    private const string SaveKeyStart = "TutorialShown_Start";
    private const string SaveKeyMillion = "TutorialShown_Million";
    private const string SaveKeyEvolveWarning = "TutorialShown_EvolveWarning";
    private const string SaveKeyMaxLevel = "TutorialShown_MaxLevel";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (alwaysShowOnStart || !PlayerPrefs.HasKey(SaveKeyStart))
        {
            ShowStartTutorial();
        }
        else
        {
            tutorialCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (!tutorialCanvas.activeSelf) return;
        if (!Input.GetMouseButtonDown(0)) return;

        HandleClick();
    }

    // 처음 시작 튜토리얼 (0~2)
    public void ShowStartTutorial()
    {
        ShowFrom(startTutorialFrom, startTutorialTo, SaveKeyStart);
    }

    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(SaveKeyStart);
        PlayerPrefs.DeleteKey(SaveKeyMillion);
        PlayerPrefs.DeleteKey(SaveKeyEvolveWarning);
        PlayerPrefs.DeleteKey(SaveKeyMaxLevel);
        PlayerPrefs.Save();
    }

    // 천만원 돌파 튜토리얼
    public void TryShowMillionTutorial()
    {
        if (!alwaysShowOnMillion && PlayerPrefs.HasKey(SaveKeyMillion))
        {
            return;
        }

        ShowFrom(millionTutorialFrom, millionTutorialTo, SaveKeyMillion);
    }

    // 진화아이템 경고 첫 등장 튜토리얼
    public void TryShowEvolveWarningTutorial()
    {
        if (!alwaysShowOnEvolveWarning && PlayerPrefs.HasKey(SaveKeyEvolveWarning))
        {
            return;
        }

        ShowFrom(evolveWarningTutorialFrom, evolveWarningTutorialTo, SaveKeyEvolveWarning);
    }

    // 15강 첫 달성 튜토리얼
    public void TryShowMaxLevelTutorial()
    {
        if (!alwaysShowOnMaxLevel && PlayerPrefs.HasKey(SaveKeyMaxLevel))
        {
            return;
        }

        ShowFrom(maxLevelTutorialFrom, maxLevelTutorialTo, SaveKeyMaxLevel);
    }

    private void ShowFrom(int from, int to, string saveKey)
    {
        if (sentences == null || sentences.Length == 0)
        {
            Debug.LogWarning("[TutorialManager] sentences가 비어 있음.");
            return;
        }

        if (from >= sentences.Length)
        {
            Debug.LogWarning("[TutorialManager] from 인덱스가 sentences 범위를 벗어남: " + from);
            return;
        }

        currentIndex = from;
        endIndex = Mathf.Min(to, sentences.Length - 1);
        currentSaveKey = saveKey;

        tutorialCanvas.SetActive(true);
        PlayCurrentSentence();
    }

    private string currentSaveKey;

    private void PlayCurrentSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeSentence(sentences[currentIndex]));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in sentence)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void HandleClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShopSelect();
        }

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
            dialogueText.text = sentences[currentIndex];
            return;
        }

        if (currentIndex >= endIndex)
        {
            PlayerPrefs.SetInt(currentSaveKey, 1);
            PlayerPrefs.Save();
            tutorialCanvas.SetActive(false);
            return;
        }

        currentIndex++;
        PlayCurrentSentence();
    }
}
