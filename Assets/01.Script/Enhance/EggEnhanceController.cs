using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EggEnhanceController : MonoBehaviour
{
    public static EggEnhanceController Instance { get; private set; }
    [Header("UI")]
    public Image eggImage;
    public Image pokemonImage;

    public Button enhanceButton;
    public Button sellButton;

    [Header("되살리기")]
    [SerializeField] private Button reviveButton;
    [SerializeField] private float destroyedEnhanceButtonOffsetY = -140f;

    private Vector2 _enhanceButtonOriginalPos;

    private TMP_Text enhanceButtonText;
    private Color _originalButtonTextColor;
    private bool _pendingEvolveWarning = false;
    private bool _evolveWarningDismissed = false;

    private Queue<(string msg, Color color)> _messageQueue = new();
    private Coroutine _messageFade;
    private bool _skipCurrent;

    [Header("Message Color")]
    [SerializeField] private Color defaultMessageColor = Color.black;

    public TextMeshProUGUI messageText;
    public TextMeshProUGUI enhanceLevelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI successRateText;
    public TextMeshProUGUI destroyRateText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI sellPriceText;

    [Header("Animation")]
    public float frameDelay = 0.12f;
    public Animator shinyEffectAnimator;

    [Header("Status Icon")]
    public PokemonStatusIconController statusIconController;

    [Header("Debug")]
    [Range(0f, 1f)]
    public float shinyChance = 0.02f;

    [HideInInspector] public bool alwaysSucceedEnhance = false;
    [HideInInspector] public bool forceShinyHatch = false;

    public bool AlwaysSucceedEnhance => alwaysSucceedEnhance;
    public bool ForceShinyHatch => forceShinyHatch;

    [Header("Balance Data")]
    public EggBalanceData balanceData;

    public List<PokemonData> allPokemons;
    public PokemonInstance currentInstance;

    public long gold = 999999999L;
    public int maxLevel = 15;

    [Header("Egg Purchase")]
    [Tooltip("알 구매 시 부화마다 차감될 골드")]
    public long eggPurchaseCost = 100000L;
    [SerializeField] private Image selectedPokemonIconImage;
    [SerializeField] private Button selectedPokemonButton;

    public PokemonData SelectedHatchPokemon { get; private set; }

    [Header("Test Spawn")]
    [Tooltip("0이면 랜덤, 1 이상이면 해당 도감 번호를 강제로 부화")]
    public int testSpawnPokemonId = 0;

    [Tooltip("true면 canHatch/isLegendary 조건을 무시하고 번호만 맞으면 테스트 소환")]
    public bool ignoreHatchConditionForTest = true;

    public Coroutine loopCoroutine;
    public int currentFrameIndex = 0;

    public PokemonEvolutionService evolutionService;

    private EggEnhanceService enhanceService;
    private EggHatchService hatchService;
    private EggUIService uiService;
    private EggSaveService saveService;

    public bool IsProcessing
    {
        get;
        private set;
    }

    public bool IsDestroyed
    {
        get;
        set;
    }

    public RevivalSnapshot DestroyedSnapshot
    {
        get;
        set;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            alwaysSucceedEnhance = !alwaysSucceedEnhance;
            Debug.Log("[디버그] 강화 항상 성공: " + alwaysSucceedEnhance);
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            forceShinyHatch = !forceShinyHatch;
            Debug.Log("[디버그] 이로치 강제 부화: " + forceShinyHatch);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (enhanceButton != null)
        {
            enhanceButtonText = enhanceButton.GetComponentInChildren<TMP_Text>();
            if (enhanceButtonText != null)
            {
                _originalButtonTextColor = enhanceButtonText.color;
            }
            _enhanceButtonOriginalPos = enhanceButton.GetComponent<RectTransform>().anchoredPosition;
        }

        if (reviveButton != null)
        {
            reviveButton.onClick.AddListener(OnReviveClick);
            reviveButton.gameObject.SetActive(false);
        }

        evolutionService = new PokemonEvolutionService();
        enhanceService = new EggEnhanceService();
        hatchService = new EggHatchService();
        uiService = new EggUIService();
        saveService = new EggSaveService();

        enhanceButton.onClick.AddListener(OnEnhanceClick);
        sellButton.onClick.AddListener(OnSellClick);

        if (selectedPokemonButton != null)
        {
            selectedPokemonButton.onClick.AddListener(ClearSelectedHatchPokemon);
            selectedPokemonButton.gameObject.SetActive(false);
        }

        PokemonData[] loaded = Resources.LoadAll<PokemonData>("PokemonData");
        allPokemons = new List<PokemonData>(loaded);

        saveService.Load(this);
        uiService.UpdateGold(this);
        uiService.UpdateAll(this);

        if (currentInstance != null)
        {
            statusIconController?.Setup(currentInstance);
        }
        else
        {
            statusIconController?.Hide();
        }

        SetButtonsInteractable(true);
        UpdateDestroyedUI();
    }

    private bool ShouldShowEvolveWarning()
    {
        if (currentInstance == null)
        {
            return false;
        }
        if (evolutionService == null)
        {
            return false;
        }
        if (_evolveWarningDismissed)
        {
            return false;
        }
        return evolutionService.IsAtAnyEvolutionTiming(currentInstance) && !evolutionService.CanEvolveNow(currentInstance);
    }

    public void ShowMessage(string msg, Color color)
    {
        color.a = 1f;
        _messageQueue.Enqueue((msg, color));
        _skipCurrent = true;
        if (_messageFade == null)
        {
            _messageFade = StartCoroutine(ProcessMessageQueue());
        }
    }

    public void ShowMessage(string msg)
    {
        ShowMessage(msg, defaultMessageColor);
    }

    private IEnumerator ProcessMessageQueue()
    {
        while (_messageQueue.Count > 0)
        {
            (string msg, Color color) = _messageQueue.Dequeue();
            messageText.color = color;
            messageText.text = msg;
            _skipCurrent = false;

            float elapsed = 0f;
            while (elapsed < 2.0f && !_skipCurrent)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!_skipCurrent)
            {
                float fade = 0f;
                Color start = messageText.color;
                while (fade < 0.3f)
                {
                    fade += Time.deltaTime;
                    Color c = start;
                    c.a = Mathf.Lerp(1f, 0f, fade / 0.3f);
                    messageText.color = c;
                    yield return null;
                }
            }
            messageText.color = new Color(color.r, color.g, color.b, 0f);
            messageText.text = "";
        }
        _messageFade = null;
    }

    public void ResetEvolveWarning()
    {
        _pendingEvolveWarning = false;
        _evolveWarningDismissed = false;
        if (messageText != null)
        {
            if (_messageFade != null)
            {
                StopCoroutine(_messageFade);
                _messageFade = null;
            }
            _messageQueue.Clear();
            messageText.color = defaultMessageColor;
            messageText.text = "";
        }
        RefreshEnhanceButtonText();
    }

    private void OnEnhanceClick()
    {
        if (IsProcessing == true)
        {
            return;
        }

        if (currentInstance == null)
        {
            _pendingEvolveWarning = false;
            RefreshEnhanceButtonText();
            StartCoroutine(hatchService.Hatch(this));
            return;
        }

        // 진화 타이밍인데 아이템이 선택되지 않은 경우 첫 클릭 시 경고
        if (!_pendingEvolveWarning && ShouldShowEvolveWarning())
        {
            _pendingEvolveWarning = true;
            if (_messageFade != null)
            {
                StopCoroutine(_messageFade);
                _messageFade = null;
            }
            _messageQueue.Clear();
            messageText.color = Color.red;
            messageText.text = "진화아이템을 선택해주세요!";
            RefreshEnhanceButtonText();

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.TryShowEvolveWarningTutorial();
            }

            return;
        }

        // 경고를 무시하고 강화하는 경우에만 dismissed 처리
        if (_pendingEvolveWarning)
        {
            _evolveWarningDismissed = true;
        }
        _pendingEvolveWarning = false;
        RefreshEnhanceButtonText();

        BeginProcessing();

        try
        {
            enhanceService.Enhance(this);

            if (GeneralBagPanelController.Instance != null)
            {
                GeneralBagPanelController.Instance.ConsumeEnhanceItems();
            }
        }
        finally
        {
            EndProcessing();
        }
    }

    private void OnSellClick()
    {
        if (IsProcessing == true)
        {
            return;
        }

        ResetEvolveWarning();
        enhanceService.Sell(this);
    }

    public void BeginProcessing()
    {
        IsProcessing = true;
        SetButtonsInteractable(false);
    }

    public void EndProcessing()
    {
        IsProcessing = false;
        SetButtonsInteractable(true);
    }

    public void RefreshEnhanceButtonText()
    {
        if (enhanceButtonText == null)
        {
            return;
        }

        if (_pendingEvolveWarning)
        {
            enhanceButtonText.text = "경고";
            enhanceButtonText.color = Color.red;
            return;
        }

        enhanceButtonText.color = _originalButtonTextColor;
        enhanceButtonText.text = "강화하기";
    }

    private void OnReviveClick()
    {
        if (IsProcessing) { return; }

        bool revivalQueued = GeneralBagPanelController.Instance != null && GeneralBagPanelController.Instance.HasRevivalItemSelected();
        if (!revivalQueued)
        {
            ShowMessage("기력의 덩어리를 선택해주세요.", Color.red);
            return;
        }

        BeginProcessing();
        try { enhanceService.Revive(this); }
        finally { EndProcessing(); }
    }

    public void UpdateDestroyedUI()
    {
        if (enhanceButton != null)
        {
            RectTransform rt = enhanceButton.GetComponent<RectTransform>();
            Vector2 pos = _enhanceButtonOriginalPos;
            if (IsDestroyed) { pos.y += destroyedEnhanceButtonOffsetY; }
            rt.anchoredPosition = pos;
        }

        if (reviveButton != null)
        {
            reviveButton.gameObject.SetActive(IsDestroyed);
        }
    }

    public void SetButtonsInteractable(bool value)
    {
        if (enhanceButton != null)
        {
            enhanceButton.interactable = value;
        }

        if (sellButton != null)
        {
            sellButton.interactable = value;
        }

        if (reviveButton != null && reviveButton.gameObject.activeSelf)
        {
            reviveButton.interactable = value;
        }
    }

    public void ResetAllGameData()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        PlayerPrefs.DeleteKey("SAVE_DATA");
        PlayerPrefs.DeleteKey("SAVE_hasPokemon");
        PlayerPrefs.DeleteKey("SAVE_rootID");
        PlayerPrefs.DeleteKey("SAVE_currentID");
        PlayerPrefs.DeleteKey("SAVE_enhanceLevel");
        PlayerPrefs.DeleteKey("SAVE_gender");
        PlayerPrefs.DeleteKey("SAVE_isShiny");
        PlayerPrefs.DeleteKey("SAVE_formIndex");
        PlayerPrefs.DeleteKey("SAVE_gold");
        PlayerPrefs.Save();

        IsDestroyed = false;
        DestroyedSnapshot = null;
        currentInstance = null;
        gold = 400000L;

        if (eggImage != null)
        {
            eggImage.gameObject.SetActive(true);
        }
        if (pokemonImage != null)
        {
            pokemonImage.gameObject.SetActive(false);
            pokemonImage.sprite = null;
        }

        statusIconController?.Hide();

        if (GeneralBagPanelController.Instance != null)
        {
            GeneralBagPanelController.Instance.ClearSelection();
        }

        uiService ??= new EggUIService();
        uiService.UpdateGold(this);
        uiService.UpdateAll(this);
        RefreshEnhanceButtonText();
        UpdateDestroyedUI();
    }

    [ContextMenu("Reset SAVE_DATA")]
    private void ResetSaveDataInInspector()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        PlayerPrefs.DeleteKey("SAVE_DATA");
        PlayerPrefs.DeleteKey("EVOLVE_INVENTORY");
        PlayerPrefs.DeleteKey("GENERAL_INVENTORY");
        PlayerPrefs.DeleteKey("DEX_MAX");
        PlayerPrefs.Save();

        if (PokedexSaveManager.Instance != null)
        {
            PokedexSaveManager.Instance.ResetDex();
        }

        currentInstance = null;
        gold = 999999999L;

        if (eggImage != null)
        {
            eggImage.gameObject.SetActive(true);
        }

        if (pokemonImage != null)
        {
            pokemonImage.gameObject.SetActive(false);
            pokemonImage.sprite = null;
        }

        if (messageText != null)
        {
            messageText.text = "";
        }

        statusIconController?.Hide();

        uiService ??= new EggUIService();

        uiService.UpdateGold(this);
        uiService.UpdateAll(this);

        Debug.Log("SAVE_DATA 초기화 완료");
    }

    public void RefreshGoldUI()
    {
        uiService?.UpdateGold(this);
    }

    public void SelectPokemonForHatch(PokemonData data)
    {
        SelectedHatchPokemon = data;

        if(selectedPokemonIconImage != null)
        {
            Sprite[] icons = Resources.LoadAll<Sprite>("Icon/" + data.id.ToString("D3"));

            if (icons != null && icons.Length > 0)
            {
                selectedPokemonIconImage.sprite = icons[0];
            }
        }

        if (selectedPokemonButton != null)
        {
            selectedPokemonButton.gameObject.SetActive(true);
        }
    }

    public void ClearSelectedHatchPokemon()
    {
        SelectedHatchPokemon = null;

        if (selectedPokemonButton != null)
        {
            selectedPokemonButton.gameObject.SetActive(false);
        }
    }
}