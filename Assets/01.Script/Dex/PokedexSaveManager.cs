using UnityEngine;
using System.Collections.Generic;

public class PokedexSaveManager : MonoBehaviour
{
    public static PokedexSaveManager Instance;

    private HashSet<int> ownedPokemon = new HashSet<int>();
    private HashSet<int> maxEnhancedPokemon = new HashSet<int>();

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

    public void RegisterMaxEnhance(int id)
    {
        maxEnhancedPokemon.Add(id);
        Save();
    }

    public bool IsOwned(int id)
    {
        return ownedPokemon.Contains(id);
    }

    public bool IsMaxEnhanced(int id)
    {
        return maxEnhancedPokemon.Contains(id);
    }

    public void ResetDex()
    {
        PlayerPrefs.DeleteKey("DEX_OWNED");
        PlayerPrefs.DeleteKey("DEX_MAX");

        ownedPokemon.Clear();
        maxEnhancedPokemon.Clear();

        Debug.Log("도감 데이터 초기화 완료");
    }

    public void ResetDexButton()
    {
        ResetDex();

        PokedexManager manager = FindFirstObjectByType<PokedexManager>();
        if (manager != null)
        {
            manager.GenerateCurrentPage();
        }
    }

    void Save()
    {
        string owned = string.Join(",", ownedPokemon);
        string maxed = string.Join(",", maxEnhancedPokemon);

        PlayerPrefs.SetString("DEX_OWNED", owned);
        PlayerPrefs.SetString("DEX_MAX", maxed);
    }

    void Load()
    {
        ownedPokemon.Clear();
        maxEnhancedPokemon.Clear();

        string owned = PlayerPrefs.GetString("DEX_OWNED", "");
        string maxed = PlayerPrefs.GetString("DEX_MAX", "");

        foreach (var s in owned.Split(','))
            if (int.TryParse(s, out int id))
                ownedPokemon.Add(id);

        foreach (var s in maxed.Split(','))
            if (int.TryParse(s, out int id))
                maxEnhancedPokemon.Add(id);
    }
}