using UnityEngine;

public class EggSaveService
{
    public void Save(EggEnhanceController c)
    {
        EnhanceSaveData data = new EnhanceSaveData();

        if (c.currentInstance != null)
        {
            data.hasPokemon = true;
            data.rootID = c.currentInstance.rootData.id;
            data.currentID = c.currentInstance.data.id;
            data.enhanceLevel = c.currentInstance.enhanceLevel;
            data.gender = (int)c.currentInstance.gender;
            data.isShiny = c.currentInstance.isShiny;
        }
        else
        {
            data.hasPokemon = false;
        }

        data.gold = c.gold;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SAVE_DATA", json);
        PlayerPrefs.Save();
    }

    public void Load(EggEnhanceController c)
    {
        if (!PlayerPrefs.HasKey("SAVE_DATA"))
        {
            return;
        }

        if (c.allPokemons == null || c.allPokemons.Count == 0)
        {
            Debug.LogError("PokemonData 아직 로드 안됨");
            return;
        }

        string json = PlayerPrefs.GetString("SAVE_DATA");
        EnhanceSaveData data = JsonUtility.FromJson<EnhanceSaveData>(json);

        c.gold = data.gold;

        if (!data.hasPokemon)
        {
            return;
        }

        PokemonData root = c.allPokemons.Find(p => p.id == data.rootID);

        if (root == null)
        {
            Debug.LogError("root 못 찾음: " + data.rootID);
            return;
        }

        c.currentInstance = new PokemonInstance(
            root,
            (Gender)data.gender,
            data.isShiny
        );

        c.currentInstance.enhanceLevel = data.enhanceLevel;
    }
}