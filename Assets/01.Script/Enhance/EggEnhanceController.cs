using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EggEnhanceController : MonoBehaviour
{
    public static EggEnhanceController Instance { get; private set; }
    [Header("UI")]
    public Image eggImage;
    public Image pokemonImage;

    public Button enhanceButton;
    public Button sellButton;

    private TMP_Text enhanceButtonText;

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

    [Header("Balance Data")]
    public EggBalanceData balanceData;

    public List<PokemonData> allPokemons;
    public PokemonInstance currentInstance;

    public long gold = 999999999L;
    public int maxLevel = 15;

    [Header("Test Spawn")]
    [Tooltip("0이면 랜덤, 1 이상이면 해당 도감 번호를 강제로 부화")]
    public int testSpawnPokemonId = 0;

    [Tooltip("true면 canHatch/isLegendary 조건을 무시하고 번호만 맞으면 테스트 소환")]
    public bool ignoreHatchConditionForTest = true;

    public Coroutine loopCoroutine;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (enhanceButton != null)
            enhanceButtonText = enhanceButton.GetComponentInChildren<TMP_Text>();

        evolutionService = new PokemonEvolutionService();

        enhanceService = new EggEnhanceService();
        hatchService = new EggHatchService();
        uiService = new EggUIService();
        saveService = new EggSaveService();

        enhanceButton.onClick.AddListener(OnEnhanceClick);
        sellButton.onClick.AddListener(OnSellClick);

        PokemonData[] loaded = Resources.LoadAll<PokemonData>("PokemonData");
        allPokemons = new List<PokemonData>(loaded);

        saveService.Load(this);
        uiService.UpdateGold(this);
        uiService.UpdateAll(this);

        if (currentInstance != null)
            statusIconController?.Setup(currentInstance);
        else
            statusIconController?.Hide();

        SetButtonsInteractable(true);
    }

    private void OnEnhanceClick()
    {
        if (IsProcessing == true)
        {
            return;
        }

        if (currentInstance == null)
        {
            bool revivalQueued = IsDestroyed
                && GeneralBagPanelController.Instance != null
                && GeneralBagPanelController.Instance.HasRevivalItemQueued();

            if (revivalQueued)
            {
                BeginProcessing();
                try   { enhanceService.Revive(this); }
                finally { EndProcessing(); }
                return;
            }

            StartCoroutine(hatchService.Hatch(this));
            return;
        }

        BeginProcessing();

        try
        {
            enhanceService.Enhance(this);

            if (GeneralBagPanelController.Instance != null)
                GeneralBagPanelController.Instance.ConsumeEnhanceItems();
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
        if (enhanceButtonText == null) return;

        bool showRevive = IsDestroyed
            && GeneralBagPanelController.Instance != null
            && GeneralBagPanelController.Instance.HasRevivalItemQueued();

        enhanceButtonText.text = showRevive ? "되살리기" : "강화하기";
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
    }

    public void ResetAllGameData()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        PlayerPrefs.DeleteKey("SAVE_DATA");
        PlayerPrefs.Save();

        IsDestroyed       = false;
        DestroyedSnapshot = null;
        currentInstance   = null;
        gold              = 400000L;

        if (eggImage != null)  eggImage.gameObject.SetActive(true);
        if (pokemonImage != null)
        {
            pokemonImage.gameObject.SetActive(false);
            pokemonImage.sprite = null;
        }

        statusIconController?.Hide();

        if (GeneralBagPanelController.Instance != null)
            GeneralBagPanelController.Instance.ClearQueue();

        uiService ??= new EggUIService();
        uiService.UpdateGold(this);
        uiService.UpdateAll(this);
        RefreshEnhanceButtonText();
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
}