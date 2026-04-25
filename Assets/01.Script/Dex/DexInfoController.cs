using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DexInfoController : MonoBehaviour
{
    public static DexInfoController Instance { get; private set; }

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Image pokemonImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Gender Icon")]
    [SerializeField] private Image genderIcon;
    [SerializeField] private Sprite maleIconSprite;
    [SerializeField] private Sprite femaleIconSprite;

    [Header("Shiny Icon")]
    [SerializeField] private GameObject shinyIcon;

    [Header("Star Icon (15강 달성)")]
    [SerializeField] private GameObject starIcon;

    [Header("Egg Purchase")]
    [SerializeField] private Button buyEggButton;

    // Current view state
    private PokemonData _data;
    private Gender _currentGender;
    private int _currentFormIndex;
    private bool _isShiny;
    private bool _owned;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NavigateNext);
        }

        if (prevButton != null)
        {
            prevButton.onClick.AddListener(NavigatePrev);
        }

        if (buyEggButton != null)
        {
            buyEggButton.onClick.AddListener(OnBuyEggClick);
            buyEggButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (infoPanel == null || !infoPanel.activeSelf)
        {
            return;
        }

        if (!_owned || _data == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleGender();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            NextForm();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleShiny();
        }
    }

    public void ShowInfo(string pokemonName, Sprite sprite, bool owned, PokemonData data = null, Gender gender = Gender.None, int formIndex = 0, bool isShiny = false)
    {
        _data = data;
        _currentFormIndex = formIndex;

        // 성별 고정 포켓몬은 비율에 따라 강제 설정
        if (data != null && data.genderVisualType != GenderVisualType.None)
        {
            if (Mathf.Approximately(data.maleRatio, 0f))
            {
                _currentGender = Gender.Female;
            }
            else if (Mathf.Approximately(data.maleRatio, 1f))
            {
                _currentGender = Gender.Male;
            }
            else
            {
                _currentGender = gender;
            }
        }
        else
        {
            _currentGender = gender;
        }

        // SameVisual + gender=None: 별 아이콘 즉시 표시를 위해 maxed 여부로 성별 초기화
        if (_currentGender == Gender.None && data != null && data.genderVisualType == GenderVisualType.SameVisual && owned && PokedexSaveManager.Instance != null)
        {
            bool hasMale = !Mathf.Approximately(data.maleRatio, 0f);
            bool hasFemale = !Mathf.Approximately(data.maleRatio, 1f);

            _currentGender = hasMale ? Gender.Male : Gender.Female;

            // 수컷이 미달성이고 암컷이 달성된 경우 암컷으로 전환
            if (hasMale && hasFemale && !PokedexSaveManager.Instance.IsFormMaxedExact(data.id, Gender.Male, formIndex, isShiny) && PokedexSaveManager.Instance.IsFormMaxedExact(data.id, Gender.Female, formIndex, isShiny))
            {
                _currentGender = Gender.Female;
            }
        }

        _isShiny = isShiny;
        _owned = owned;

        if (nameText != null)
        {
            nameText.text = owned ? pokemonName : "???";
        }

        if (pokemonImage != null)
        {
            pokemonImage.sprite = sprite;
            pokemonImage.preserveAspect = true;
            pokemonImage.color = owned ? Color.white : Color.black;
        }

        UpdateGenderIcon();
        UpdateShinyIcon();
        UpdateStarIcon();
        UpdateBuyEggButton();

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
    }

    public bool IsInfoOpen => infoPanel != null && infoPanel.activeSelf;

    public void HideInfo()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }

    private void NavigateNext()
    {
        if (_data == null) 
        { 
            return; 
        }
        int maxId = PokedexManager.Instance != null ? PokedexManager.Instance.maxDexCount : 151;
        int nextId = (_data.id >= maxId) ? 1 : _data.id + 1;
        ShowInfoById(nextId);
    }

    private void NavigatePrev()
    {
        if (_data == null)
        {
            return;
        }
        int maxId = PokedexManager.Instance != null ? PokedexManager.Instance.maxDexCount : 151;
        int prevId = (_data.id <= 1) ? maxId : _data.id - 1;
        ShowInfoById(prevId);
    }

    private void ShowInfoById(int id)
    {
        PokemonData data = null;
        if (EggEnhanceController.Instance != null)
        {
            data = EggEnhanceController.Instance.allPokemons.Find(p => p.id == id);
        }

        bool owned = PokedexSaveManager.Instance != null && PokedexSaveManager.Instance.IsOwned(id);
        string pokemonName = data != null ? data.pokemonName : id.ToString();

        Gender gender = Gender.None;
        int formIndex = 0;
        bool isShiny = false;
        Sprite display = null;

        if (owned && data != null)
        {
            SelectDisplayVariant(id, data, out gender, out formIndex, out isShiny);
            string path = PokemonFormUtility.GetLoadPath(data, gender, isShiny, formIndex);
            Sprite[] loaded = Resources.LoadAll<Sprite>(path);
            if (loaded != null && loaded.Length > 0)
            {
                display = loaded[0];
            }
        }

        if (display == null)
        {
            Sprite[] fallback = Resources.LoadAll<Sprite>("Pokemon/" + id.ToString("D3") + "_normal");
            if (fallback != null && fallback.Length > 0)
            {
                display = fallback[0];
            }
        }

        if (display == null)
        {
            Sprite[] iconFallback = Resources.LoadAll<Sprite>("Icon/" + id.ToString("D3"));
            if (iconFallback != null && iconFallback.Length > 0)
            {
                display = iconFallback[0];
            }
        }

        ShowInfo(pokemonName, display, owned, data, gender, formIndex, isShiny);
    }

    private void UpdateBuyEggButton()
    {
        if (buyEggButton == null)
        {
            return;
        }
        buyEggButton.gameObject.SetActive(_data != null && _data.canHatch && _owned);
    }

    private void OnBuyEggClick()
    {
        if (_data == null || !_data.canHatch)
        {
            return;
        }
        if (EggEnhanceController.Instance != null)
        {
            EggEnhanceController.Instance.SelectPokemonForHatch(_data);
        }
    }

    private void SelectDisplayVariant(int id, PokemonData data, out Gender gender, out int formIndex, out bool isShiny)
    {
        var save = PokedexSaveManager.Instance;

        bool seenNonShiny = HasAnySeenForm(id, data, Gender.None, false);
        bool seenShiny = HasAnySeenForm(id, data, Gender.None, true);
        isShiny = !seenNonShiny && seenShiny;

        if (data.genderVisualType == GenderVisualType.DifferentVisual)
        {
            bool seenMale = HasAnySeenForm(id, data, Gender.Male, isShiny);
            bool seenFemale = HasAnySeenForm(id, data, Gender.Female, isShiny);
            gender = (seenMale || (!seenMale && !seenFemale)) ? Gender.Male : Gender.Female;
        }
        else
        {
            gender = Gender.None;
        }

        List<int> availableForms = PokemonFormUtility.GetAvailableFormIndices(data, gender, isShiny);
        formIndex = 0;
        foreach (int fi in availableForms)
        {
            if (save != null && save.IsFormSeen(id, gender, fi, isShiny, data.genderVisualType))
            {
                formIndex = fi;
                break;
            }
        }
    }

    private bool HasAnySeenForm(int id, PokemonData data, Gender genderHint, bool isShiny)
    {
        var save = PokedexSaveManager.Instance;
        if (save == null)
        {
            return false;
        }

        if (data.genderVisualType == GenderVisualType.DifferentVisual && genderHint == Gender.None)
        {
            return HasAnySeenForm(id, data, Gender.Male, isShiny) || HasAnySeenForm(id, data, Gender.Female, isShiny);
        }

        List<int> forms = PokemonFormUtility.GetAvailableFormIndices(data, genderHint, isShiny);
        foreach (int fi in forms)
        {
            if (save.IsFormSeen(id, genderHint, fi, isShiny, data.genderVisualType))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsGenderLocked()
    {
        if (_data == null)
        {
            return false;
        }
        return Mathf.Approximately(_data.maleRatio, 0f) || Mathf.Approximately(_data.maleRatio, 1f);
    }

    // G: 반대 성별로 전환. DifferentVisual이면 스프라이트도 변경, SameVisual이면 아이콘만 변경, None이면 무시
    private void ToggleGender()
    {
        if (_data.genderVisualType == GenderVisualType.None)
        {
            return;
        }

        if (IsGenderLocked())
        {
            return;
        }

        _currentGender = (_currentGender == Gender.Male) ? Gender.Female : Gender.Male;

        if (_data.genderVisualType == GenderVisualType.DifferentVisual)
        {
            // 새 성별에서 현재 폼이 없으면 첫 번째 폼으로 리셋
            List<int> forms = PokemonFormUtility.GetAvailableFormIndices(_data, _currentGender, _isShiny);

            if (!forms.Contains(_currentFormIndex))
            {
                _currentFormIndex = forms.Count > 0 ? forms[0] : 0;
            }

            RefreshSprite();
        }
        else
        {
            // SameVisual: 스프라이트/색상은 그대로, 아이콘만 변경
            UpdateGenderIcon();
            UpdateShinyIcon();
            UpdateStarIcon();
        }
    }

    // F: 다음 폼으로 변경. 폼이 하나뿐이면 무시
    private void NextForm()
    {
        List<int> forms = PokemonFormUtility.GetAvailableFormIndices(_data, _currentGender, _isShiny);

        if (forms.Count <= 1)
        {
            return;
        }

        int idx = forms.IndexOf(_currentFormIndex);
        idx = (idx + 1) % forms.Count;
        _currentFormIndex = forms[idx];

        RefreshSprite();
    }

    // S: 이로치/비이로치 전환
    private void ToggleShiny()
    {
        _isShiny = !_isShiny;

        // 이로치 상태에서 현재 폼이 없으면 첫 번째 폼으로 리셋
        List<int> forms = PokemonFormUtility.GetAvailableFormIndices(_data, _currentGender, _isShiny);

        if (!forms.Contains(_currentFormIndex))
        {
            _currentFormIndex = forms.Count > 0 ? forms[0] : 0;
        }

        RefreshSprite();
    }

    private void RefreshSprite()
    {
        if (_data == null || pokemonImage == null)
        {
            return;
        }

        string path = PokemonFormUtility.GetLoadPath(_data, _currentGender, _isShiny, _currentFormIndex);
        Sprite[] loaded = Resources.LoadAll<Sprite>(path);

        if (loaded != null && loaded.Length > 0)
        {
            pokemonImage.sprite = loaded[0];
        }

        bool seen = PokedexSaveManager.Instance != null && PokedexSaveManager.Instance.IsFormSeen(_data.id, _currentGender, _currentFormIndex, _isShiny, _data.genderVisualType);
        pokemonImage.color = seen ? Color.white : Color.black;

        UpdateGenderIcon();
        UpdateShinyIcon();
        UpdateStarIcon();
    }

    private void UpdateShinyIcon()
    {
        if (shinyIcon == null)
        {
            return;
        }

        shinyIcon.SetActive(_isShiny);
    }

    private void UpdateStarIcon()
    {
        if (starIcon == null)
        {
            return;
        }

        bool maxed = _owned && _data != null && PokedexSaveManager.Instance != null && PokedexSaveManager.Instance.IsFormMaxedExact(_data.id, _currentGender, _currentFormIndex, _isShiny);

        starIcon.SetActive(maxed);
    }

    private void UpdateGenderIcon()
    {
        if (genderIcon == null)
        {
            return;
        }

        if (_data == null || _data.genderVisualType == GenderVisualType.None)
        {
            genderIcon.gameObject.SetActive(false);
            return;
        }

        genderIcon.gameObject.SetActive(true);
        genderIcon.sprite = _currentGender == Gender.Female ? femaleIconSprite : maleIconSprite;
    }
}
