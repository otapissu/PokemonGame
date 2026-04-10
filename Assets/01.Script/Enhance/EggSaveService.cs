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
            data.formIndex = c.currentInstance.formIndex;
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

        // 기존 세이브가 int로 저장된 경우 음수(-21억)로 깨진 값 보정
        if (data.gold < 0)
        {
            Debug.LogWarning("gold 값이 음수입니다. 기존 int 오버플로우 세이브로 판단하여 0으로 초기화합니다.");
            data.gold = 0L;
        }

        c.gold = data.gold;

        if (data.hasPokemon == false)
        {
            c.currentInstance = null;

            if (c.eggImage != null)
            {
                c.eggImage.gameObject.SetActive(true);
            }

            if (c.pokemonImage != null)
            {
                c.pokemonImage.gameObject.SetActive(false);
            }

            new EggUIService().UpdateAll(c);
            return;
        }

        PokemonData root = c.allPokemons.Find(p => p.id == data.rootID);
        PokemonData current = c.allPokemons.Find(p => p.id == data.currentID);

        if (root == null)
        {
            Debug.LogError("root 못 찾음: " + data.rootID);
            return;
        }

        if (current == null)
        {
            Debug.LogError("current 못 찾음: " + data.currentID);
            return;
        }

        c.currentInstance = new PokemonInstance(
            root,
            (Gender)data.gender,
            data.isShiny,
            data.formIndex
        );

        c.currentInstance.data = current;
        c.currentInstance.enhanceLevel = data.enhanceLevel;
        c.currentInstance.formIndex = data.formIndex;

        RestoreVisual(c);
        new EggUIService().UpdateAll(c);
    }

    private void RestoreVisual(EggEnhanceController c)
    {
        if (c.currentInstance == null)
        {
            return;
        }

        Sprite[] frames = EggHatchService.LoadSprites(c.currentInstance);

        if (frames == null || frames.Length == 0)
        {
            Debug.LogError("로드 후 스프라이트 없음: " + c.currentInstance.data.id);
            return;
        }

        if (c.eggImage != null)
        {
            c.eggImage.gameObject.SetActive(false);
        }

        if (c.pokemonImage != null)
        {
            c.pokemonImage.gameObject.SetActive(true);
            c.pokemonImage.sprite = frames[0];
        }

        EggHatchService.ApplyAutoScale(c, frames[0]);
        EggHatchService.StartAnimationLoop(c);
    }
}