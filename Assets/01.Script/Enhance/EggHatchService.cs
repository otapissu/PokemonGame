using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EggHatchService
{
    public IEnumerator Hatch(EggEnhanceController c)
    {
        if (c == null)
        {
            yield break;
        }

        if (c.IsProcessing == true)
        {
            yield break;
        }

        c.IsDestroyed = false;
        c.DestroyedSnapshot = null;
        c.UpdateDestroyedUI();

        if (c.SelectedHatchPokemon != null)
        {
            if (c.gold < c.eggPurchaseCost)
            {
                c.ShowMessage("골드 부족!", Color.red);
                yield break;
            }
            c.gold -= c.eggPurchaseCost;
            new EggUIService().UpdateGold(c);
        }

        c.BeginProcessing();

        try
        {
            if (c.loopCoroutine != null)
            {
                c.StopCoroutine(c.loopCoroutine);
                c.loopCoroutine = null;
            }

            c.pokemonImage.gameObject.SetActive(false);
            c.eggImage.gameObject.SetActive(true);

            if (c.shinyEffectAnimator != null)
            {
                c.shinyEffectAnimator.gameObject.SetActive(false);
            }

            yield return c.StartCoroutine(EggShake(c));

            PokemonData selected = GetRandomHatchPokemon(c);

            if (selected == null)
            {
                Debug.LogError("selected null → 부화 중단");
                yield break;
            }

            bool shinyCharmActive = GeneralBagPanelController.Instance != null && GeneralBagPanelController.Instance.IsShinyCharmSelected();
            float effectiveShinyChance = shinyCharmActive ? 0.10f : c.shinyChance;
            bool isShiny = Random.value < effectiveShinyChance;
            Gender genderFilter = GeneralBagPanelController.Instance != null ? GeneralBagPanelController.Instance.GetGenderFilter() : Gender.None;
            Gender gender = RollGender(selected, genderFilter);
            int formIndex = PokemonFormUtility.GetRandomFormIndex(selected, gender, isShiny);

            c.currentInstance = new PokemonInstance(selected, gender, isShiny, formIndex);
            c.currentInstance.enhanceLevel = 1;

            Debug.Log(
                "[부화 결과] " +
                "ID: " + selected.id.ToString("D3") +
                ", 이름: " + selected.pokemonName +
                ", 성별: " + gender +
                ", 샤이니: " + isShiny +
                ", 폼 인덱스: " + formIndex
            );

            new EggSaveService().Save(c);

            if (PokedexSaveManager.Instance != null)
            {
                PokedexSaveManager.Instance.RegisterPokemon(selected.id);
            }

            Sprite[] frames = LoadSprites(c.currentInstance);

            if (frames == null || frames.Length == 0)
            {
                Debug.LogError("스프라이트 없음: " + selected.id);

                c.currentInstance = null;
                new EggSaveService().Save(c);

                yield break;
            }

            c.pokemonImage.sprite = frames[0];
            ApplyAutoScale(c, frames[0]);

            c.eggImage.gameObject.SetActive(false);
            c.pokemonImage.gameObject.SetActive(true);

            // Setup 먼저 → 처음 본 폼이면 "new" 스프라이트 표시
            c.statusIconController?.Setup(c.currentInstance);

            // 아이콘 표시 후 "본 적 있음" 등록
            if (PokedexSaveManager.Instance != null)
            {
                PokedexSaveManager.Instance.RegisterFormSeen(
                    c.currentInstance.data.id,
                    c.currentInstance.gender,
                    c.currentInstance.formIndex,
                    c.currentInstance.isShiny,
                    c.currentInstance.data.genderVisualType
                );
            }

            c.ShowMessage(selected.pokemonName + " 등장!");

            if (isShiny == true)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayShinyAppear();
                }

                if (c.shinyEffectAnimator != null)
                {
                    c.shinyEffectAnimator.gameObject.SetActive(true);
                    c.StartCoroutine(DisableShinyEffectAfterPlay(c));
                }
            }

            new EggUIService().UpdateAll(c);

            // 부화 시 큐에 담긴 아이템 소모
            if (GeneralBagPanelController.Instance != null)
            {
                GeneralBagPanelController.Instance.ApplySelectedItems();
            }

            yield return c.StartCoroutine(PopEffect(c));

            StartAnimationLoop(c);
        }
        finally
        {
            c.EndProcessing();
        }
    }

    private IEnumerator DisableShinyEffectAfterPlay(EggEnhanceController c)
    {
        // 한 프레임 대기 후 Animator 상태 확인
        yield return null;

        AnimatorStateInfo info = c.shinyEffectAnimator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);

        if (c.shinyEffectAnimator != null)
        {
            c.shinyEffectAnimator.gameObject.SetActive(false);
        }
    }

    public IEnumerator EggShake(EggEnhanceController c)
    {
        RectTransform rt = c.eggImage.rectTransform;
        Vector2 originalPos = rt.anchoredPosition;

        float duration = 0.2f;
        float timer = 0f;
        float strength = 8f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float offsetX = Random.Range(-1f, 1f) * strength;
            float offsetY = Random.Range(-1f, 1f) * strength;

            rt.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
            yield return null;
        }

        rt.anchoredPosition = originalPos;
    }

    public static IEnumerator PopEffect(EggEnhanceController c)
    {
        RectTransform rt = c.pokemonImage.rectTransform;

        float duration = 0.1f;
        float timer = 0f;

        Vector3 baseScale = Vector3.one;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float scale = Mathf.Lerp(0.85f, 1.05f, t);
            rt.localScale = baseScale * scale;

            yield return null;
        }

        rt.localScale = baseScale;
    }

    public static void StartAnimationLoop(EggEnhanceController c)
    {
        if (c.loopCoroutine != null)
        {
            c.StopCoroutine(c.loopCoroutine);
        }

        Sprite[] frames = LoadSprites(c.currentInstance);
        c.loopCoroutine = c.StartCoroutine(PlayLoop(c, frames));
    }

    private static IEnumerator PlayLoop(EggEnhanceController c, Sprite[] frames)
    {
        if (frames == null || frames.Length == 0)
        {
            yield break;
        }

        int index = c.currentFrameIndex % frames.Length;

        while (true)
        {
            c.pokemonImage.sprite = frames[index];
            ApplyAutoScale(c, frames[index]);

            c.currentFrameIndex = index;
            index = (index + 1) % frames.Length;

            yield return new WaitForSeconds(c.frameDelay);
        }
    }

    public static void ApplyAutoScale(EggEnhanceController c, Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        float spriteWidth = sprite.rect.width;
        float spriteHeight = sprite.rect.height;

        float maxSide = Mathf.Max(spriteWidth, spriteHeight);

        float targetSize = 480f;
        float scale = targetSize / maxSide;

        RectTransform rt = c.pokemonImage.rectTransform;
        rt.sizeDelta = new Vector2(spriteWidth * scale, spriteHeight * scale);
        rt.localScale = Vector3.one;
    }

    private PokemonData GetRandomHatchPokemon(EggEnhanceController c)
    {
        if (c.allPokemons == null || c.allPokemons.Count == 0)
        {
            Debug.LogError("PokemonData 로드 안됨");
            return null;
        }

        if (c.SelectedHatchPokemon != null)
        {
            PokemonData selected = c.allPokemons.Find(p => p.id == c.SelectedHatchPokemon.id);
            if (selected != null && HasAnyUsableSprite(selected))
            {
                return selected;
            }
        }

        if (c.testSpawnPokemonId > 0)
        {
            PokemonData testTarget = c.allPokemons.Find(p => p.id == c.testSpawnPokemonId);

            if (testTarget == null)
            {
                Debug.LogError("테스트 소환 실패: 해당 번호의 포켓몬이 없음 → " + c.testSpawnPokemonId);
                return null;
            }

            if (HasAnyUsableSprite(testTarget) == false)
            {
                Debug.LogError("테스트 소환 실패: 사용 가능한 스프라이트가 없음 → " + c.testSpawnPokemonId);
                return null;
            }

            if (c.ignoreHatchConditionForTest == false && testTarget.canHatch == false)
            {
                Debug.LogError("테스트 소환 실패: canHatch가 false임 → " + c.testSpawnPokemonId);
                return null;
            }

            Debug.Log("테스트 소환 적용 → ID: " + testTarget.id + ", 이름: " + testTarget.pokemonName);
            return testTarget;
        }

        float roll = Random.value;

        Gender genderFilter = GeneralBagPanelController.Instance != null ? GeneralBagPanelController.Instance.GetGenderFilter() : Gender.None;
        bool incenseActive = genderFilter != Gender.None;

        List<PokemonData> legendaryList = c.allPokemons.FindAll(p => p.canHatch && p.isLegendary && HasAnyUsableSprite(p) && (!incenseActive || p.genderVisualType != GenderVisualType.None));

        List<PokemonData> normalList = c.allPokemons.FindAll(p => p.canHatch && !p.isLegendary && HasAnyUsableSprite(p) && (!incenseActive || p.genderVisualType != GenderVisualType.None));

        List<PokemonData> targetList =
            roll < 0.05f ? legendaryList : normalList;

        if (targetList == null || targetList.Count == 0)
        {
            Debug.LogError("부화 가능한 포켓몬 없음");
            return null;
        }

        return targetList[Random.Range(0, targetList.Count)];
    }

    private bool HasAnyUsableSprite(PokemonData data)
    {
        if (data == null)
        {
            return false;
        }

        if (data.genderVisualType == GenderVisualType.None)
        {
            return HasAnyUsableSpriteByGender(data, Gender.None);
        }

        if (HasAnyUsableSpriteByGender(data, Gender.Male))
        {
            return true;
        }

        if (HasAnyUsableSpriteByGender(data, Gender.Female))
        {
            return true;
        }

        return false;
    }

    private bool HasAnyUsableSpriteByGender(PokemonData data, Gender gender)
    {
        if (PokemonFormUtility.HasSprites(data, gender, false, 0))
        {
            return true;
        }

        for (int i = 1; i < 100; i++)
        {
            if (PokemonFormUtility.HasSprites(data, gender, false, i))
            {
                return true;
            }
        }

        return false;
    }

    private Gender RollGender(PokemonData data, Gender filter = Gender.None)
    {
        if (data.genderVisualType == GenderVisualType.None)
        {
            return Gender.None;
        }

        if (filter == Gender.Female)
        {
            return Gender.Female;
        }
        if (filter == Gender.Male)
        {
            return Gender.Male;
        }

        return Random.value < data.maleRatio ? Gender.Male : Gender.Female;
    }

    public static Sprite[] LoadSprites(PokemonInstance instance)
    {
        if (instance == null)
        {
            return null;
        }

        if (instance.data == null)
        {
            return null;
        }

        string path = PokemonFormUtility.GetLoadPath(
            instance.data,
            instance.gender,
            instance.isShiny,
            instance.formIndex
        );

        return Resources.LoadAll<Sprite>(path);
    }
}