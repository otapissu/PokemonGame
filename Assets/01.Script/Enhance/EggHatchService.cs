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

            yield return c.StartCoroutine(EggShake(c));

            PokemonData selected = GetRandomHatchPokemon(c);

            if (selected == null)
            {
                Debug.LogError("selected null → 부화 중단");
                yield break;
            }

            bool isShiny = Random.value < c.shinyChance;
            Gender gender = RollGender(selected);
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

            c.messageText.text = selected.pokemonName + " 등장!";

            if (isShiny == true)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayShinyAppear();
                }
            }

            new EggUIService().UpdateAll(c);

            yield return c.StartCoroutine(PopEffect(c));

            StartAnimationLoop(c);
        }
        finally
        {
            c.EndProcessing();
        }
    }

    public IEnumerator EggShake(EggEnhanceController c)
    {
        RectTransform rt = c.eggImage.rectTransform;
        Vector2 originalPos = rt.anchoredPosition;

        float duration = 0.5f;
        float timer = 0f;
        float strength = 12f;

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

        float duration = 0.25f;
        float timer = 0f;

        Vector3 baseScale = Vector3.one;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float scale = Mathf.Lerp(0.6f, 1.2f, t);
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

        int index = 0;

        while (true)
        {
            c.pokemonImage.sprite = frames[index];
            ApplyAutoScale(c, frames[index]);

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

        float targetSize = 500f;
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

        float roll = Random.value;

        List<PokemonData> legendaryList =
            c.allPokemons.FindAll(p => p.canHatch && p.isLegendary && HasAnyUsableSprite(p));

        List<PokemonData> normalList =
            c.allPokemons.FindAll(p => p.canHatch && !p.isLegendary && HasAnyUsableSprite(p));

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

    private Gender RollGender(PokemonData data)
    {
        if (data.genderVisualType == GenderVisualType.None)
        {
            return Gender.None;
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