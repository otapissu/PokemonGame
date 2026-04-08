using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
}