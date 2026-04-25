using UnityEngine;
using System.Collections.Generic;

public class PokedexSaveManager : MonoBehaviour
{
    public static PokedexSaveManager Instance;

    private HashSet<int> ownedPokemon = new();
    private HashSet<int> anyMaxIds = new();
    private HashSet<string> maxEnhancedKeys = new();
    private HashSet<string> seenFormKeys = new();

    // 별 아이콘 표시용: 성별까지 포함한 정확한 키 (SameVisual도 분리)
    private HashSet<string> genderMaxKeys = new();

    // 진화 체인 전체에 전파되는 최대 강화 정보 (비전설 한정)
    private HashSet<int> chainAnyMaxIds = new();
    private HashSet<int> chainAllMaxIds = new();

    // 폼 키 목록 캐시
    private Dictionary<int, List<string>> allKeysCache = new();
    // SameVisual 성별 구분 all-maxed 체크 캐시
    private Dictionary<int, List<string>> genderAllKeysCache = new();

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
        genderMaxKeys.Add(MakeGenderKey(id, gender, formIndex, isShiny));
        Save();
    }

    public void RegisterFormSeen(int id, Gender gender, int formIndex, bool isShiny, GenderVisualType genderVisualType)
    {
        string key = MakeKey(id, gender, formIndex, isShiny, genderVisualType);
        if (seenFormKeys.Add(key))
        {
            Save();
        }
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

    // 별 아이콘 전용: SameVisual도 성별 구분하여 정확히 일치한 조합만 true
    public bool IsFormMaxedExact(int id, Gender gender, int formIndex, bool isShiny)
    {
        return genderMaxKeys.Contains(MakeGenderKey(id, gender, formIndex, isShiny));
    }

    // 비전설 포켓몬의 진화 체인 전체에 최대 강화를 전파
    public void PropagateMaxEnhanceToChain(PokemonData rootData, int maxedId, PokemonData maxedData,
                                           Gender gender, int formIndex, bool isShiny)
    {
        if (rootData == null || maxedData == null)
        {
            return;
        }
        if (maxedData.isLegendary)
        {
            return;
        }

        List<int> chainIds = new();
        CollectChainIds(rootData, chainIds);

        foreach (int id in chainIds)
        {
            chainAnyMaxIds.Add(id);
        }

        if (IsAllFormsMaxed(maxedId, maxedData))
        {
            foreach (int id in chainIds)
            {
                chainAllMaxIds.Add(id);
            }
        }

        PropagateStarToAncestors(rootData, maxedId, gender, formIndex, isShiny);

        Save();
    }

    // 별 아이콘: 진화 체인 조상들에 star 전파
    private void PropagateStarToAncestors(PokemonData rootData, int maxedId, Gender gender, int formIndex, bool isShiny)
    {
        List<PokemonData> ancestors = new();
        CollectAncestors(rootData, maxedId, ancestors, new List<int>());

        foreach (PokemonData ancestor in ancestors)
        {
            Gender formQueryGender = ancestor.genderVisualType == GenderVisualType.DifferentVisual ? gender : Gender.None;

            List<int> availableForms = PokemonFormUtility.GetAvailableFormIndices(ancestor, formQueryGender, isShiny);
            List<int> targetForms = availableForms.Contains(formIndex) ? new List<int> { formIndex } : availableForms;

            if (ancestor.genderVisualType == GenderVisualType.DifferentVisual)
            {
                // DifferentVisual: 최강화된 개체의 성별만
                foreach (int fi in targetForms)
                {
                    genderMaxKeys.Add(MakeGenderKey(ancestor.id, gender, fi, isShiny));
                }
            }
            else if (ancestor.genderVisualType == GenderVisualType.SameVisual)
            {
                // SameVisual: 암수 외형이 같으므로 존재하는 모든 성별에 전파
                bool hasMale = !Mathf.Approximately(ancestor.maleRatio, 0f);
                bool hasFemale = !Mathf.Approximately(ancestor.maleRatio, 1f);
                foreach (int fi in targetForms)
                {
                    if (hasMale)   { genderMaxKeys.Add(MakeGenderKey(ancestor.id, Gender.Male,   fi, isShiny)); }
                    if (hasFemale) { genderMaxKeys.Add(MakeGenderKey(ancestor.id, Gender.Female, fi, isShiny)); }
                }
            }
            else // GenderVisualType.None
            {
                foreach (int fi in targetForms)
                {
                    genderMaxKeys.Add(MakeGenderKey(ancestor.id, Gender.None, fi, isShiny));
                }
            }
        }
    }

    // rootData에서 targetId까지의 경로 상 조상 목록 수집 (target 본인 제외)
    private bool CollectAncestors(PokemonData current, int targetId, List<PokemonData> ancestors, List<int> visited)
    {
        if (current == null || visited.Contains(current.id))
        {
            return false;
        }
        visited.Add(current.id);

        if (current.id == targetId)
        {
            return true;
        }

        if (current.evolutionOptions == null)
        {
            return false;
        }

        foreach (EvolutionOption opt in current.evolutionOptions)
        {
            if (opt == null || opt.targetData == null)
            {
                continue;
            }
            if (CollectAncestors(opt.targetData, targetId, ancestors, visited))
            {
                ancestors.Add(current);
                return true;
            }
        }
        return false;
    }

    private void CollectChainIds(PokemonData data, List<int> ids)
    {
        if (data == null)
        {
            return;
        }
        if (ids.Contains(data.id))
        {
            return;
        }

        ids.Add(data.id);

        if (data.evolutionOptions == null)
        {
            return;
        }
        foreach (EvolutionOption opt in data.evolutionOptions)
        {
            if (opt != null && opt.targetData != null)
            {
                CollectChainIds(opt.targetData, ids);
            }
        }
    }

    public bool IsAllFormsMaxed(int id, PokemonData data)
    {
        // SameVisual 포켓몬은 성별 구분 키(genderMaxKeys)로 체크
        if (data.genderVisualType == GenderVisualType.SameVisual)
        {
            List<string> genderKeys = GetOrComputeGenderAllKeys(id, data);
            if (genderKeys.Count == 0)
            {
                return false;
            }
            foreach (string k in genderKeys)
            {
                if (!genderMaxKeys.Contains(k))
                {
                    return false;
                }
            }
            return true;
        }

        List<string> allKeys = GetOrComputeAllKeys(id, data);
        if (allKeys.Count == 0)
        {
            return false;
        }

        foreach (string k in allKeys)
        {
            if (!maxEnhancedKeys.Contains(k))
            {
                return false;
            }
        }

        return true;
    }

    // SameVisual 전용: 성별 구분 all-maxed 키 목록 (genderMaxKeys 기준)
    private List<string> GetOrComputeGenderAllKeys(int id, PokemonData data)
    {
        if (genderAllKeysCache.TryGetValue(id, out List<string> cached))
        {
            return cached;
        }

        List<string> keys = new();
        bool[] shinyOptions = { false, true };

        bool hasMale = !Mathf.Approximately(data.maleRatio, 0f);
        bool hasFemale = !Mathf.Approximately(data.maleRatio, 1f);

        foreach (bool shiny in shinyOptions)
        {
            List<int> forms = PokemonFormUtility.GetAvailableFormIndices(data, Gender.None, shiny);
            foreach (int fi in forms)
            {
                if (hasMale)
                {
                    keys.Add(MakeGenderKey(id, Gender.Male, fi, shiny));
                }
                if (hasFemale)
                {
                    keys.Add(MakeGenderKey(id, Gender.Female, fi, shiny));
                }
            }
        }

        genderAllKeysCache[id] = keys;
        return keys;
    }

    private List<string> GetOrComputeAllKeys(int id, PokemonData data)
    {
        if (allKeysCache.TryGetValue(id, out List<string> cached))
        {
            return cached;
        }

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
                    {
                        keys.Add(MakeKey(id, g, fi, shiny, data.genderVisualType));
                    }
                }
            }
        }
        else
        {
            foreach (bool shiny in shinyOptions)
            {
                List<int> forms = PokemonFormUtility.GetAvailableFormIndices(data, Gender.None, shiny);
                foreach (int fi in forms)
                {
                    keys.Add(MakeKey(id, Gender.None, fi, shiny, data.genderVisualType));
                }
            }
        }

        allKeysCache[id] = keys;
        return keys;
    }

    // 별 아이콘 전용: genderVisualType 무관하게 성별을 항상 포함
    private static string MakeGenderKey(int id, Gender gender, int formIndex, bool isShiny)
    {
        string g = gender == Gender.Male ? "M" : (gender == Gender.Female ? "F" : "N");
        string s = isShiny ? "S" : "N";
        return id + "|" + g + "|" + s + "|" + formIndex;
    }

    // 기존 비이로치 키 포맷 유지 + 이로치는 S 접두어 추가
    private static string MakeKey(int id, Gender gender, int formIndex, bool isShiny, GenderVisualType genderVisualType)
    {
        if (genderVisualType == GenderVisualType.DifferentVisual)
        {
            string g = gender == Gender.Male ? "M" : "F";
            return isShiny ? id + "_S" + g + "_" + formIndex : id + "_" + g + "_" + formIndex;
        }

        return isShiny ? id + "_S_" + formIndex : id + "_" + formIndex;
    }

    [ContextMenu("Reset Dex")]
    public void ResetDex()
    {
        PlayerPrefs.DeleteKey("DEX_OWNED");
        PlayerPrefs.DeleteKey("DEX_MAX_V2");
        PlayerPrefs.DeleteKey("DEX_SEEN");
        PlayerPrefs.DeleteKey("DEX_CHAIN_ANY");
        PlayerPrefs.DeleteKey("DEX_CHAIN_ALL");
        PlayerPrefs.DeleteKey("DEX_GENDER_MAX");
        PlayerPrefs.Save();

        ownedPokemon.Clear();
        anyMaxIds.Clear();
        maxEnhancedKeys.Clear();
        seenFormKeys.Clear();
        chainAnyMaxIds.Clear();
        chainAllMaxIds.Clear();
        genderMaxKeys.Clear();
        allKeysCache.Clear();
        genderAllKeysCache.Clear();

        if (PokedexManager.Instance != null)
        {
            PokedexManager.Instance.GenerateCurrentPage();
        }

        Debug.Log("도감 데이터 초기화 완료");
    }

    void Save()
    {
        PlayerPrefs.SetString("DEX_OWNED", string.Join(",", ownedPokemon));
        PlayerPrefs.SetString("DEX_MAX_V2", string.Join(",", maxEnhancedKeys));
        PlayerPrefs.SetString("DEX_SEEN", string.Join(",", seenFormKeys));
        PlayerPrefs.SetString("DEX_CHAIN_ANY", string.Join(",", chainAnyMaxIds));
        PlayerPrefs.SetString("DEX_CHAIN_ALL", string.Join(",", chainAllMaxIds));
        PlayerPrefs.SetString("DEX_GENDER_MAX", string.Join(",", genderMaxKeys));
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
        genderMaxKeys.Clear();

        string owned = PlayerPrefs.GetString("DEX_OWNED", "");
        string maxed = PlayerPrefs.GetString("DEX_MAX_V2", "");
        string seen = PlayerPrefs.GetString("DEX_SEEN", "");

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
        {
            if (int.TryParse(s, out int id))
            {
                chainAllMaxIds.Add(id);
            }
        }

        string genderMax = PlayerPrefs.GetString("DEX_GENDER_MAX", "");
        foreach (string key in genderMax.Split(','))
        {
            if (!string.IsNullOrEmpty(key))
            {
                genderMaxKeys.Add(key);
            }
        }
    }
}
