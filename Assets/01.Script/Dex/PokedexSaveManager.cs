using UnityEngine;
using System.Collections.Generic;

public class PokedexSaveManager : MonoBehaviour
{
    public static PokedexSaveManager Instance;

    private HashSet<int> ownedPokemon = new();
    private HashSet<int> anyMaxIds = new();
    private HashSet<string> maxEnhancedKeys = new();
    private HashSet<string> seenFormKeys = new();

    // 진화 체인 전체에 전파되는 최대 강화 정보 (비전설 한정)
    private HashSet<int> chainAnyMaxIds = new();
    private HashSet<int> chainAllMaxIds = new();

    // 폼 키 목록 캐시 (Resources.LoadAll 반복 방지)
    private Dictionary<int, List<string>> allKeysCache = new();

    void Awake()
    {
        Instance = this;
        Load();
    }

    public void RegisterPokemon(int id)
    {
        ownedPokemon.Add(id);
        Save();
    }

    public void RegisterMaxEnhance(int id, Gender gender, int formIndex, bool isShiny, GenderVisualType genderVisualType)
    {
        string key = MakeKey(id, gender, formIndex, isShiny, genderVisualType);
        maxEnhancedKeys.Add(key);
        anyMaxIds.Add(id);
        Save();
    }

    public void RegisterFormSeen(int id, Gender gender, int formIndex, bool isShiny, GenderVisualType genderVisualType)
    {
        string key = MakeKey(id, gender, formIndex, isShiny, genderVisualType);
        if (seenFormKeys.Add(key))
            Save();
    }

    public bool IsOwned(int id) => ownedPokemon.Contains(id);

    public bool IsAnyFormMaxed(int id) => anyMaxIds.Contains(id) || chainAnyMaxIds.Contains(id);
    public bool IsChainAllMaxed(int id) => chainAllMaxIds.Contains(id);

    public bool IsFormSeen(int id, Gender gender, int formIndex, bool isShiny, GenderVisualType genderVisualType)
    {
        string key = MakeKey(id, gender, formIndex, isShiny, genderVisualType);
        return seenFormKeys.Contains(key) || maxEnhancedKeys.Contains(key);
    }

    public bool IsFormMaxed(int id, Gender gender, int formIndex, bool isShiny, GenderVisualType genderVisualType)
    {
        string key = MakeKey(id, gender, formIndex, isShiny, genderVisualType);
        return maxEnhancedKeys.Contains(key);
    }

    /// <summary>비전설 포켓몬의 진화 체인 전체에 최대 강화를 전파합니다.</summary>
    public void PropagateMaxEnhanceToChain(PokemonData rootData, int maxedId, PokemonData maxedData)
    {
        if (rootData == null || maxedData == null) return;
        if (maxedData.isLegendary) return;

        List<int> chainIds = new();
        CollectChainIds(rootData, chainIds);

        foreach (int id in chainIds)
            chainAnyMaxIds.Add(id);

        if (IsAllFormsMaxed(maxedId, maxedData))
        {
            foreach (int id in chainIds)
                chainAllMaxIds.Add(id);
        }

        Save();
    }

    private void CollectChainIds(PokemonData data, List<int> ids)
    {
        if (data == null) return;
        if (ids.Contains(data.id)) return; // 순환 방지

        ids.Add(data.id);

        if (data.evolutionOptions == null) return;
        foreach (EvolutionOption opt in data.evolutionOptions)
        {
            if (opt != null && opt.targetData != null)
                CollectChainIds(opt.targetData, ids);
        }
    }

    public bool IsAllFormsMaxed(int id, PokemonData data)
    {
        List<string> allKeys = GetOrComputeAllKeys(id, data);
        if (allKeys.Count == 0) return false;

        foreach (string k in allKeys)
        {
            if (!maxEnhancedKeys.Contains(k)) return false;
        }

        return true;
    }

    private List<string> GetOrComputeAllKeys(int id, PokemonData data)
    {
        if (allKeysCache.TryGetValue(id, out List<string> cached))
            return cached;

        List<string> keys = new();

        bool[] shinyOptions = { false, true };

        if (data.genderVisualType == GenderVisualType.DifferentVisual)
        {
            Gender[] genders = { Gender.Male, Gender.Female };
            foreach (bool shiny in shinyOptions)
            {
                foreach (Gender g in genders)
                {
                    List<int> forms = PokemonFormUtility.GetAvailableFormIndices(data, g, shiny);
                    foreach (int fi in forms)
                        keys.Add(MakeKey(id, g, fi, shiny, data.genderVisualType));
                }
            }
        }
        else
        {
            foreach (bool shiny in shinyOptions)
            {
                List<int> forms = PokemonFormUtility.GetAvailableFormIndices(data, Gender.None, shiny);
                foreach (int fi in forms)
                    keys.Add(MakeKey(id, Gender.None, fi, shiny, data.genderVisualType));
            }
        }

        allKeysCache[id] = keys;
        return keys;
    }

    // 기존 비이로치 키 포맷 유지 (하위 호환) + 이로치는 S 접두어 추가
    private static string MakeKey(int id, Gender gender, int formIndex, bool isShiny, GenderVisualType genderVisualType)
    {
        if (genderVisualType == GenderVisualType.DifferentVisual)
        {
            string g = gender == Gender.Male ? "M" : "F";
            return isShiny
                ? id + "_S" + g + "_" + formIndex
                : id + "_" + g + "_" + formIndex;
        }

        return isShiny
            ? id + "_S_" + formIndex
            : id + "_" + formIndex;
    }

    [ContextMenu("Reset Dex")]
    public void ResetDex()
    {
        PlayerPrefs.DeleteKey("DEX_OWNED");
        PlayerPrefs.DeleteKey("DEX_MAX_V2");
        PlayerPrefs.DeleteKey("DEX_SEEN");
        PlayerPrefs.DeleteKey("DEX_CHAIN_ANY");
        PlayerPrefs.DeleteKey("DEX_CHAIN_ALL");
        PlayerPrefs.Save();

        ownedPokemon.Clear();
        anyMaxIds.Clear();
        maxEnhancedKeys.Clear();
        seenFormKeys.Clear();
        chainAnyMaxIds.Clear();
        chainAllMaxIds.Clear();
        allKeysCache.Clear();

        if (PokedexManager.Instance != null)
            PokedexManager.Instance.GenerateCurrentPage();

        Debug.Log("도감 데이터 초기화 완료");
    }

    void Save()
    {
        PlayerPrefs.SetString("DEX_OWNED", string.Join(",", ownedPokemon));
        PlayerPrefs.SetString("DEX_MAX_V2", string.Join(",", maxEnhancedKeys));
        PlayerPrefs.SetString("DEX_SEEN", string.Join(",", seenFormKeys));
        PlayerPrefs.SetString("DEX_CHAIN_ANY", string.Join(",", chainAnyMaxIds));
        PlayerPrefs.SetString("DEX_CHAIN_ALL", string.Join(",", chainAllMaxIds));
        PlayerPrefs.Save();
    }

    void Load()
    {
        ownedPokemon.Clear();
        anyMaxIds.Clear();
        maxEnhancedKeys.Clear();
        seenFormKeys.Clear();
        chainAnyMaxIds.Clear();
        chainAllMaxIds.Clear();

        string owned = PlayerPrefs.GetString("DEX_OWNED", "");
        string maxed = PlayerPrefs.GetString("DEX_MAX_V2", "");
        string seen  = PlayerPrefs.GetString("DEX_SEEN", "");

        foreach (string s in owned.Split(','))
            if (int.TryParse(s, out int id))
                ownedPokemon.Add(id);

        foreach (string key in maxed.Split(','))
        {
            if (string.IsNullOrEmpty(key)) continue;
            maxEnhancedKeys.Add(key);

            string[] parts = key.Split('_');
            if (parts.Length > 0 && int.TryParse(parts[0], out int id))
                anyMaxIds.Add(id);
        }

        foreach (string key in seen.Split(','))
        {
            if (!string.IsNullOrEmpty(key))
                seenFormKeys.Add(key);
        }

        string chainAny = PlayerPrefs.GetString("DEX_CHAIN_ANY", "");
        string chainAll = PlayerPrefs.GetString("DEX_CHAIN_ALL", "");

        foreach (string s in chainAny.Split(','))
            if (int.TryParse(s, out int id))
                chainAnyMaxIds.Add(id);

        foreach (string s in chainAll.Split(','))
            if (int.TryParse(s, out int id))
                chainAllMaxIds.Add(id);
    }
}
