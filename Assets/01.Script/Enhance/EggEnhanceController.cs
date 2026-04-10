using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EggEnhanceController : MonoBehaviour
{
    [Header("UI")]
    public Image eggImage;
    public Image pokemonImage;

    public Button enhanceButton;
    public Button sellButton;

    public TextMeshProUGUI messageText;
    public TextMeshProUGUI enhanceLevelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI successRateText;
    public TextMeshProUGUI destroyRateText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI sellPriceText;

    [Header("Animation")]
    public float frameDelay = 0.12f;

    [Header("Debug")]
    [Range(0f, 1f)]
    public float shinyChance = 0.02f;

    [Header("Balance Data")]
    public EggBalanceData balanceData;

    public List<PokemonData> allPokemons;
    public PokemonInstance currentInstance;

    public int gold = 999999999;
    public int maxLevel = 15;

    [Header("Test Spawn")]
    [Tooltip("0이면 랜덤, 1 이상이면 해당 도감 번호를 강제로 부화")]
    public int testSpawnPokemonId = 0;

    [Tooltip("true면 canHatch/isLegendary 조건을 무시하고 번호만 맞으면 테스트 소환")]
    public bool ignoreHatchConditionForTest = true;

    public Coroutine loopCoroutine;

    [Header("Evolution Inventory")]
    public Dictionary<EvolutionItemType, int> evolutionItemInventory =
        new Dictionary<EvolutionItemType, int>();

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

    private void Start()
    {
        evolutionService = new PokemonEvolutionService();

        enhanceService = new EggEnhanceService();
        hatchService = new EggHatchService();
        uiService = new EggUIService();
        saveService = new EggSaveService();

        enhanceButton.onClick.AddListener(OnEnhanceClick);
        sellButton.onClick.AddListener(OnSellClick);

        PokemonData[] loaded = Resources.LoadAll<PokemonData>("PokemonData");
        allPokemons = new List<PokemonData>(loaded);

        InitializeTestEvolutionItems();

        saveService.Load(this);
        uiService.UpdateGold(this);
        uiService.UpdateAll(this);

        SetButtonsInteractable(true);
    }

    private void InitializeTestEvolutionItems()
    {
        evolutionItemInventory.Clear();

        evolutionItemInventory[EvolutionItemType.WaterStone] = 1;
        evolutionItemInventory[EvolutionItemType.FireStone] = 1;
        evolutionItemInventory[EvolutionItemType.ThunderStone] = 100;
        evolutionItemInventory[EvolutionItemType.IceStone] = 100;
        evolutionItemInventory[EvolutionItemType.KingsRock] = 1;
    }

    private void OnEnhanceClick()
    {
        if (IsProcessing == true)
        {
            return;
        }

        if (currentInstance == null)
        {
            StartCoroutine(hatchService.Hatch(this));
            return;
        }

        BeginProcessing();

        try
        {
            enhanceService.Enhance(this);
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

    [ContextMenu("Reset SAVE_DATA")]
    private void ResetSaveDataInInspector()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        PlayerPrefs.DeleteKey("SAVE_DATA");
        PlayerPrefs.Save();

        currentInstance = null;
        gold = 999999999;

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
            messageText.text = "세이브 데이터 초기화 완료";
        }

        if (uiService == null)
        {
            uiService = new EggUIService();
        }

        uiService.UpdateGold(this);
        uiService.UpdateAll(this);

        Debug.Log("SAVE_DATA 초기화 완료");
    }
}