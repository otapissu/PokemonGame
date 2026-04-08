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

    public List<PokemonData> allPokemons;

    public PokemonInstance currentInstance;

    public int gold = 999999999;
    public int maxLevel = 15;

    public Coroutine loopCoroutine;

    public int[] sellPrices =
    {
        0, 500, 1000, 2000, 6000,
        20000, 50000, 150000, 500000,
        1000000, 2000000, 5000000,
        10000000, 25000000, 50000000
    };

    private EggEnhanceService enhanceService;
    private EggHatchService hatchService;
    private EggUIService uiService;
    private EggSaveService saveService;

    void Start()
    {
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
    }

    void OnEnhanceClick()
    {
        if (currentInstance == null)
        {
            StartCoroutine(hatchService.Hatch(this));
        }
        else
        {
            enhanceService.Enhance(this);
        }
    }

    void OnSellClick()
    {
        enhanceService.Sell(this);
    }
}