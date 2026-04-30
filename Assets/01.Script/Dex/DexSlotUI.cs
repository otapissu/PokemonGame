using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DexSlotUI : MonoBehaviour
{
    public Image iconImage;
    public Image outlineImage;
    public Image shinyIconImage;

    [Header("아웃라인 스프라이트 (순서 고정)")]
    public Sprite defaultOutlineSprite;
    public Sprite normalAnyMaxSprite;
    public Sprite normalAllMaxSprite;
    public Sprite legendaryAnyMaxSprite;
    public Sprite legendaryAllMaxSprite;

    private Sprite[] frames;
    private string _pokemonName;
    private int _pokemonId;
    private PokemonData _pokemonData;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (DexInfoController.Instance == null)
        {
            return;
        }

        bool owned = PokedexSaveManager.Instance != null && PokedexSaveManager.Instance.IsOwned(_pokemonId);

        Gender selectedGender = Gender.None;
        int selectedFormIndex = 0;
        bool selectedIsShiny = false;

        Sprite display = null;
        if (owned && _pokemonData != null)
        {
            SelectDisplayVariant(out selectedGender, out selectedFormIndex, out selectedIsShiny);
            string path = PokemonFormUtility.GetLoadPath(_pokemonData, selectedGender, selectedIsShiny, selectedFormIndex);
            Sprite[] loaded = Resources.LoadAll<Sprite>(path);
            if (loaded != null && loaded.Length > 0)
            {
                display = loaded[0];
            }
        }

        if (display == null)
        {
            Gender defaultGender = (_pokemonData != null && _pokemonData.genderVisualType == GenderVisualType.DifferentVisual) ? Gender.Male : Gender.None;
            string fallbackPath = _pokemonData != null
                ? PokemonFormUtility.GetLoadPath(_pokemonData, defaultGender, false, 0)
                : "Pokemon/" + _pokemonId.ToString("D3") + "_normal";
            Sprite[] fallback = Resources.LoadAll<Sprite>(fallbackPath);
            display = (fallback != null && fallback.Length > 0) ? fallback[0] : (frames != null && frames.Length > 0 ? frames[0] : null);
        }

        DexInfoController.Instance.ShowInfo(_pokemonName, display, owned, _pokemonData, selectedGender, selectedFormIndex, selectedIsShiny);
    }

    // 우선순위: 비이로치 > 이로치 / 수컷 > 암컷 / 낮은 폼 인덱스
    private void SelectDisplayVariant(out Gender gender, out int formIndex, out bool isShiny)
    {
        var save = PokedexSaveManager.Instance;
        var data = _pokemonData;
        int id = _pokemonId;

        // 1. 이로치 여부: 비이로치를 본 적 있으면 비이로치, 아니면 이로치
        bool seenNonShiny = HasAnySeenForm(id, data, Gender.None, false);
        bool seenShiny = HasAnySeenForm(id, data, Gender.None, true);
        isShiny = !seenNonShiny && seenShiny;

        // 2. 성별: 수컷 우선 (DifferentVisual/SameVisual 모두 성별 구분)
        if (data.genderVisualType == GenderVisualType.DifferentVisual)
        {
            bool seenMale = HasAnySeenForm(id, data, Gender.Male, isShiny);
            bool seenFemale = HasAnySeenForm(id, data, Gender.Female, isShiny);
            gender = (seenMale || (!seenMale && !seenFemale)) ? Gender.Male : Gender.Female;
        }
        else if (data.genderVisualType == GenderVisualType.SameVisual)
        {
            bool hasMale = !Mathf.Approximately(data.maleRatio, 0f);
            bool hasFemale = !Mathf.Approximately(data.maleRatio, 1f);
            bool seenMale = hasMale && HasAnySeenForm(id, data, Gender.Male, isShiny);
            bool seenFemale = hasFemale && HasAnySeenForm(id, data, Gender.Female, isShiny);
            gender = (seenMale || (!seenMale && !seenFemale)) ? (hasMale ? Gender.Male : Gender.Female) : Gender.Female;
        }
        else
        {
            gender = Gender.None;
        }

        // 3. 폼: 본 폼 중 가장 낮은 인덱스
        List<int> availableForms = PokemonFormUtility.GetAvailableFormIndices(data, gender, isShiny);
        formIndex = 0;
        foreach (int fi in availableForms)
        {
            if (save.IsFormSeen(id, gender, fi, isShiny, data.genderVisualType))
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

        if (genderHint == Gender.None && (data.genderVisualType == GenderVisualType.DifferentVisual || data.genderVisualType == GenderVisualType.SameVisual))
        {
            bool hasMale = !Mathf.Approximately(data.maleRatio, 0f);
            bool hasFemale = !Mathf.Approximately(data.maleRatio, 1f);
            return (hasMale && HasAnySeenForm(id, data, Gender.Male, isShiny)) || (hasFemale && HasAnySeenForm(id, data, Gender.Female, isShiny));
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

    public void Setup(int id, Sprite[] cachedFrames, PokemonData data)
    {
        frames = cachedFrames;
        _pokemonId = id;
        _pokemonData = data;
        _pokemonName = (data != null) ? data.pokemonName : id.ToString();

        if (frames == null || frames.Length == 0)
        {
            iconImage.sprite = null;
            outlineImage.enabled = false;
            return;
        }

        bool owned = PokedexSaveManager.Instance != null && PokedexSaveManager.Instance.IsOwned(id);
        bool anyMaxed = owned && PokedexSaveManager.Instance.IsAnyFormMaxed(id);
        bool allMaxed = anyMaxed && data != null && (PokedexSaveManager.Instance.IsAllFormsMaxed(id, data) || PokedexSaveManager.Instance.IsChainAllMaxed(id));
        bool legendary = data != null && data.isLegendary;

        iconImage.preserveAspect = true;
        iconImage.color = owned ? Color.white : Color.black;
        iconImage.sprite = frames[0];

        outlineImage.enabled = true;
        outlineImage.sprite = GetOutlineSprite(anyMaxed, allMaxed, legendary);

        if (shinyIconImage != null)
        {
            bool shinySeen = data != null && HasAnySeenForm(id, data, Gender.None, true);
            shinyIconImage.color = shinySeen ? Color.white : Color.black;
        }
    }

    public void UpdateFrame(int frameIndex)
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }
        iconImage.sprite = frames[frameIndex % frames.Length];
    }

    private Sprite GetOutlineSprite(bool anyMaxed, bool allMaxed, bool legendary)
    {
        if (!anyMaxed)
        {
            return defaultOutlineSprite;
        }
        if (legendary)
        {
            return allMaxed ? legendaryAllMaxSprite : legendaryAnyMaxSprite;
        }
        
        return allMaxed ? normalAllMaxSprite : normalAnyMaxSprite;
    }
}
