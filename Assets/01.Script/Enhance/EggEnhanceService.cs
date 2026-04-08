using UnityEngine;

public class EggEnhanceService
{
    public void Enhance(EggEnhanceController c)
    {
        if (c.currentInstance == null)
        {
            Debug.LogWarning("currentInstance null");
            return;
        }

        if (c.currentInstance.data == null)
        {
            Debug.LogError("currentInstance.data null → 복원 실패 상태");
            return;
        }

        if (c.currentInstance.enhanceLevel >= c.maxLevel)
        {
            c.messageText.text = "최대 강화입니다.";
            return;
        }

        int nextLevel = c.currentInstance.enhanceLevel + 1;
        int cost = EggDataService.GetEnhanceCost(nextLevel);

        if (c.gold < cost)
        {
            c.messageText.text = "골드 부족!";
            return;
        }

        c.gold -= cost;
        new EggUIService().UpdateGold(c);

        float success = EggDataService.GetSuccessRate(nextLevel);
        float destroy = EggDataService.GetDestroyRate(nextLevel);
        float roll = Random.value;

        if (roll < success)
        {
            c.currentInstance.enhanceLevel++;

            string beforeName = c.currentInstance.data.pokemonName;

            CheckEvolution(c);

            if (c.currentInstance.data == null)
            {
                Debug.LogError("진화 후 data null");
                return;
            }

            int currentId = c.currentInstance.data.id;

            if (PokedexSaveManager.Instance != null)
            {
                PokedexSaveManager.Instance.RegisterPokemon(currentId);

                if (c.currentInstance.enhanceLevel == 15)
                {
                    PokedexSaveManager.Instance.RegisterMaxEnhance(currentId);
                }
            }

            new EggSaveService().Save(c);

            Sprite[] frames = EggHatchService.LoadSprites(c.currentInstance);
            if (frames != null && frames.Length > 0)
            {
                c.pokemonImage.sprite = frames[0];
                EggHatchService.ApplyAutoScale(c, frames[0]);
            }

            if (beforeName != c.currentInstance.data.pokemonName)
            {
                c.messageText.text = beforeName + " 진화!";
                c.StartCoroutine(EggHatchService.PopEffect(c));
            }
            else
            {
                c.messageText.text = "강화 성공!";
            }

            EggHatchService.StartAnimationLoop(c);
        }
        else if (roll < success + destroy)
        {
            c.messageText.text = "파괴됨!";

            c.currentInstance = null;

            new EggSaveService().Save(c);

            c.pokemonImage.gameObject.SetActive(false);
            c.eggImage.gameObject.SetActive(true);

            new EggUIService().Clear(c);
        }
        else
        {
            c.messageText.text = "강화 실패!";
        }

        new EggUIService().UpdateAll(c);
    }

    private void CheckEvolution(EggEnhanceController c)
    {
        PokemonData root = c.currentInstance.rootData;
        int level = c.currentInstance.enhanceLevel;

        if (root.nextEvolution == null && root.secondEvolution == null)
        {
            return;
        }

        if (root.nextEvolution != null && root.secondEvolution == null && level == 5)
        {
            c.currentInstance.data = root.nextEvolution;
        }

        if (root.nextEvolution != null && root.secondEvolution != null)
        {
            if (level == 3)
            {
                c.currentInstance.data = root.nextEvolution;
            }
            else if (level == 6)
            {
                c.currentInstance.data = root.secondEvolution;
            }
        }
    }

    public void Sell(EggEnhanceController c)
    {
        if (c.currentInstance == null)
        {
            return;
        }

        int sellGold = EggDataService.GetSellPrice(c);
        c.gold += sellGold;

        c.messageText.text = sellGold.ToString("N0") + " 골드 획득!";

        if (c.loopCoroutine != null)
        {
            c.StopCoroutine(c.loopCoroutine);
            c.loopCoroutine = null;
        }

        c.pokemonImage.sprite = null;
        c.currentInstance = null;

        new EggSaveService().Save(c);

        c.pokemonImage.gameObject.SetActive(false);
        c.eggImage.gameObject.SetActive(true);

        new EggUIService().UpdateGold(c);
        new EggUIService().Clear(c);
    }
}