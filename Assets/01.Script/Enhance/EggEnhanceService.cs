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
            Debug.LogError("currentInstance.data null — 강화 로직 중단");
            return;
        }

        if (c.currentInstance.enhanceLevel >= c.maxLevel)
        {
            c.messageText.text = "최대 강화입니다.";
            return;
        }

        int nextLevel = c.currentInstance.enhanceLevel + 1;
        long cost = EggDataService.GetEnhanceCost(c, nextLevel);

        if (c.gold < cost)
        {
            c.messageText.text = "골드 부족!";
            return;
        }

        c.gold -= cost;
        new EggUIService().UpdateGold(c);

        float bonus   = GeneralBagPanelController.Instance != null
                        ? GeneralBagPanelController.Instance.GetSuccessRateBonus()
                        : 0f;
        float success = Mathf.Min(EggDataService.GetSuccessRate(c, nextLevel) + bonus, 1f);
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

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayEnhanceFail();
            }
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
            Debug.LogError("��ȭ �� data null");
            return;
        }

        int currentId = c.currentInstance.data.id;

        if (PokedexSaveManager.Instance != null)
        {
            PokedexSaveManager.Instance.RegisterPokemon(currentId);
            PokedexSaveManager.Instance.RegisterFormSeen(
                currentId,
                c.currentInstance.gender,
                c.currentInstance.formIndex,
                c.currentInstance.isShiny,
                c.currentInstance.data.genderVisualType
            );

            if (c.currentInstance.enhanceLevel == 15)
            {
                PokedexSaveManager.Instance.RegisterMaxEnhance(
                    currentId,
                    c.currentInstance.gender,
                    c.currentInstance.formIndex,
                    c.currentInstance.isShiny,
                    c.currentInstance.data.genderVisualType
                );

                PokedexSaveManager.Instance.PropagateMaxEnhanceToChain(
                    c.currentInstance.rootData,
                    currentId,
                    c.currentInstance.data
                );
            }
        }

        c.statusIconController?.Setup(c.currentInstance);

        new EggSaveService().Save(c);

        Sprite[] frames = EggHatchService.LoadSprites(c.currentInstance);

        if (frames != null && frames.Length > 0)
        {
            c.pokemonImage.sprite = frames[0];
            EggHatchService.ApplyAutoScale(c, frames[0]);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnhanceSuccess();
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
            c.currentInstance
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
        c.IsDestroyed = true;
        c.DestroyedSnapshot = new RevivalSnapshot(c.currentInstance);
        c.messageText.text = "파괴됨!";

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnhanceDestroy();
        }

        c.statusIconController?.Hide();

        c.currentInstance = null;

        new EggSaveService().Save(c);

        c.pokemonImage.gameObject.SetActive(false);
        c.eggImage.gameObject.SetActive(true);

        if (GeneralBagPanelController.Instance != null)
            GeneralBagPanelController.Instance.ClearQueue();

        new EggUIService().Clear(c);
    }

    public void Revive(EggEnhanceController c)
    {
        RevivalSnapshot snap = c.DestroyedSnapshot;
        if (snap == null)
        {
            Debug.LogWarning("[Revival] 스냅샷 없음");
            return;
        }

        ShopItemData revivalItem = GeneralBagPanelController.Instance != null
                                   ? GeneralBagPanelController.Instance.GetRevivalItemData()
                                   : null;
        if (revivalItem == null)
        {
            Debug.LogWarning("[Revival] 복구권 아이템 없음");
            return;
        }

        // 필요 갯수 계산
        int costIndex = snap.destroyedAtLevel - 6;
        int cost = 1;
        if (c.balanceData != null && c.balanceData.reviveCosts != null
            && costIndex >= 0 && costIndex < c.balanceData.reviveCosts.Length)
        {
            cost = c.balanceData.reviveCosts[costIndex];
        }

        // 보유 갯수 확인
        int available = 0;
        if (PlayerGeneralInventory.Instance != null)
        {
            foreach (var e in PlayerGeneralInventory.Instance.Entries)
            {
                if (e.item == revivalItem) { available = e.count; break; }
            }
        }

        if (available < cost)
        {
            c.messageText.text = "기력의 덩어리가 부족합니다.";
            return;
        }

        // 아이템 소모
        for (int i = 0; i < cost; i++)
            PlayerGeneralInventory.Instance.RemoveItem(revivalItem, 1);

        // 인스턴스 복원
        c.currentInstance = new PokemonInstance(snap.rootData, snap.gender, snap.isShiny, snap.formIndex);
        c.currentInstance.data         = snap.data;
        c.currentInstance.enhanceLevel = snap.enhanceLevel;

        c.IsDestroyed       = false;
        c.DestroyedSnapshot = null;

        // 이미지 복원
        Sprite[] frames = EggHatchService.LoadSprites(c.currentInstance);
        if (frames != null && frames.Length > 0)
        {
            c.pokemonImage.sprite = frames[0];
            EggHatchService.ApplyAutoScale(c, frames[0]);
        }

        c.eggImage.gameObject.SetActive(false);
        c.pokemonImage.gameObject.SetActive(true);

        c.statusIconController?.Setup(c.currentInstance);
        c.messageText.text = snap.data.pokemonName + " 부활!";

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayEnhanceSuccess();

        new EggSaveService().Save(c);
        new EggUIService().UpdateAll(c);
        EggHatchService.StartAnimationLoop(c);

        if (GeneralBagPanelController.Instance != null)
        {
            GeneralBagPanelController.Instance.ClearQueue();
            GeneralBagPanelController.Instance.RefreshCounts();
        }

        c.RefreshEnhanceButtonText();
    }

    public void Sell(EggEnhanceController c)
    {
        if (c.currentInstance == null)
        {
            return;
        }

        long sellGold = EggDataService.GetSellPrice(c);
        c.gold += sellGold;

        c.messageText.text = sellGold.ToString("N0") + " 골드 획득!";

        if (c.loopCoroutine != null)
        {
            c.StopCoroutine(c.loopCoroutine);
            c.loopCoroutine = null;
        }

        c.statusIconController?.Hide();

        c.IsDestroyed = false;
        c.DestroyedSnapshot = null;
        c.pokemonImage.sprite = null;
        c.currentInstance = null;

        new EggSaveService().Save(c);

        c.pokemonImage.gameObject.SetActive(false);
        c.eggImage.gameObject.SetActive(true);

        new EggUIService().UpdateGold(c);
        new EggUIService().Clear(c);
    }
}