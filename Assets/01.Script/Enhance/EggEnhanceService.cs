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
        int cost = EggDataService.GetEnhanceCost(c,nextLevel);

        if (c.gold < cost)
        {
            c.messageText.text = "골드 부족!";
            return;
        }

        c.gold -= cost;
        new EggUIService().UpdateGold(c);

        float success = EggDataService.GetSuccessRate(c, nextLevel);
        float destroy = EggDataService.GetDestroyRate(c, nextLevel);
        float roll = Random.value;

        if (roll < success)
        {
            HandleEnhanceSuccess(c);
        }
        else if (roll < success + destroy)
        {
            HandleEnhanceDestroyed(c);
        }
        else
        {
            c.messageText.text = "강화 실패!";
        }

        new EggUIService().UpdateAll(c);
    }

    private void HandleEnhanceSuccess(EggEnhanceController c)
    {
        c.currentInstance.enhanceLevel++;

        if (c.currentInstance.enhanceLevel == 15)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayEggLevel15();
            }
        }

        string beforeName = c.currentInstance.data.pokemonName;
        bool evolved = TryEvolution(c);

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

        if (evolved)
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

    private bool TryEvolution(EggEnhanceController c)
    {
        if (c == null)
        {
            return false;
        }

        if (c.currentInstance == null)
        {
            return false;
        }

        if (c.currentInstance.data == null)
        {
            return false;
        }

        if (c.evolutionService == null)
        {
            Debug.LogWarning("evolutionService null");
            return false;
        }

        string beforeName = c.currentInstance.data.pokemonName;

        EvolutionOption selectedOption = c.evolutionService.GetRandomAvailableEvolution(
            c.currentInstance,
            c.evolutionItemInventory
        );

        if (selectedOption == null)
        {
            return false;
        }

        bool evolved = c.evolutionService.TryEvolve(
            c.currentInstance,
            selectedOption
        );

        if (!evolved)
        {
            return false;
        }

        if (c.currentInstance.data == null)
        {
            Debug.LogError("진화 후 currentInstance.data null");
            return false;
        }

        return beforeName != c.currentInstance.data.pokemonName;
    }

    private void HandleEnhanceDestroyed(EggEnhanceController c)
    {
        c.messageText.text = "파괴됨!";

        c.currentInstance = null;

        new EggSaveService().Save(c);

        c.pokemonImage.gameObject.SetActive(false);
        c.eggImage.gameObject.SetActive(true);

        new EggUIService().Clear(c);
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