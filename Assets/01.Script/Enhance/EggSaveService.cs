using UnityEngine;

public class EggSaveService
{
    public void Save(EggEnhanceController c)
    {
        if (c.currentInstance != null)
        {
            PlayerPrefs.SetInt("SAVE_hasPokemon", 1);
            PlayerPrefs.SetInt("SAVE_rootID", c.currentInstance.rootData.id);
            PlayerPrefs.SetInt("SAVE_currentID", c.currentInstance.data.id);
            PlayerPrefs.SetInt("SAVE_enhanceLevel", c.currentInstance.enhanceLevel);
            PlayerPrefs.SetInt("SAVE_gender", (int)c.currentInstance.gender);
            PlayerPrefs.SetInt("SAVE_isShiny", c.currentInstance.isShiny ? 1 : 0);
            PlayerPrefs.SetInt("SAVE_formIndex", c.currentInstance.formIndex);
        }
        else
        {
            PlayerPrefs.SetInt("SAVE_hasPokemon", 0);
        }

        PlayerPrefs.SetString("SAVE_gold", c.gold.ToString());
        PlayerPrefs.Save();
    }

    public void Load(EggEnhanceController c)
    {
        if (!PlayerPrefs.HasKey("SAVE_hasPokemon"))
        {
            return;
        }

        if (c.allPokemons == null || c.allPokemons.Count == 0)
        {
            Debug.LogError("PokemonData 아직 로드 안됨");
            return;
        }

        if (!long.TryParse(PlayerPrefs.GetString("SAVE_gold", "0"), out long gold))
            gold = 0L;

        // 기존 세이브가 int로 저장된 경우 음수(-21억)로 깨진 값 보정
        if (gold < 0)
        {
            Debug.LogWarning("gold 값이 음수입니다. 기존 int 오버플로우 세이브로 판단하여 0으로 초기화합니다.");
            gold = 0L;
        }

        c.gold = gold;

        bool hasPokemon = PlayerPrefs.GetInt("SAVE_hasPokemon", 0) == 1;

        if (!hasPokemon)
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

        int rootID = PlayerPrefs.GetInt("SAVE_rootID", 0);
        int currentID = PlayerPrefs.GetInt("SAVE_currentID", 0);
        int enhanceLevel = PlayerPrefs.GetInt("SAVE_enhanceLevel", 0);
        int gender = PlayerPrefs.GetInt("SAVE_gender", 0);
        bool isShiny = PlayerPrefs.GetInt("SAVE_isShiny", 0) == 1;
        int formIndex = PlayerPrefs.GetInt("SAVE_formIndex", 0);

        PokemonData root = c.allPokemons.Find(p => p.id == rootID);
        PokemonData current = c.allPokemons.Find(p => p.id == currentID);

        if (root == null)
        {
            Debug.LogError("root 못 찾음: " + rootID);
            return;
        }

        if (current == null)
        {
            Debug.LogError("current 못 찾음: " + currentID);
            return;
        }

        c.currentInstance = new PokemonInstance(
            root,
            (Gender)gender,
            isShiny,
            formIndex
        );

        c.currentInstance.data = current;
        c.currentInstance.enhanceLevel = enhanceLevel;
        c.currentInstance.formIndex = formIndex;

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