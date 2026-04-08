using System.Collections.Generic;
using UnityEngine;

public static class PokemonFormUtility
{
    public static bool HasAdditionalForms(PokemonData data, Gender gender, bool isShiny)
    {
        if (data == null)
        {
            return false;
        }

        for (int i = 1; i < 100; i++)
        {
            if (HasSprites(data, gender, isShiny, i))
            {
                return true;
            }
        }

        return false;
    }

    public static int GetRandomFormIndex(PokemonData data, Gender gender, bool isShiny)
    {
        List<int> availableForms = GetAvailableFormIndices(data, gender, isShiny);

        if (availableForms.Count == 0)
        {
            return 0;
        }

        int randomIndex = Random.Range(0, availableForms.Count);
        return availableForms[randomIndex];
    }

    public static List<int> GetAvailableFormIndices(PokemonData data, Gender gender, bool isShiny)
    {
        List<int> result = new List<int>();

        if (data == null)
        {
            return result;
        }

        if (HasSprites(data, gender, isShiny, 0))
        {
            result.Add(0);
        }

        for (int i = 1; i < 100; i++)
        {
            if (HasSprites(data, gender, isShiny, i))
            {
                result.Add(i);
            }
            else
            {
                break;
            }
        }

        return result;
    }

    public static bool HasSprites(PokemonData data, Gender gender, bool isShiny, int formIndex)
    {
        string primaryPath = BuildSpritePath(data, gender, isShiny, formIndex);
        Sprite[] primarySprites = Resources.LoadAll<Sprite>(primaryPath);

        if (primarySprites != null && primarySprites.Length > 0)
        {
            return true;
        }

        string fallbackPath = BuildUngenderedFallbackPath(data, isShiny, formIndex);

        if (string.IsNullOrEmpty(fallbackPath))
        {
            return false;
        }

        Sprite[] fallbackSprites = Resources.LoadAll<Sprite>(fallbackPath);
        return fallbackSprites != null && fallbackSprites.Length > 0;
    }

    public static string BuildSpritePath(PokemonData data, Gender gender, bool isShiny, int formIndex)
    {
        string id = data.id.ToString("D3");
        string rarityPart = isShiny ? "shiny" : "normal";
        string path = "Pokemon/" + id + "_" + rarityPart;

        if (data.genderVisualType == GenderVisualType.DifferentVisual)
        {
            if (gender == Gender.Male)
            {
                path += "_m";
            }
            else if (gender == Gender.Female)
            {
                path += "_f";
            }
        }

        if (formIndex > 0)
        {
            path += "_" + formIndex;
        }

        return path;
    }

    public static string GetLoadPath(PokemonData data, Gender gender, bool isShiny, int formIndex)
    {
        string primaryPath = BuildSpritePath(data, gender, isShiny, formIndex);
        Sprite[] primarySprites = Resources.LoadAll<Sprite>(primaryPath);

        if (primarySprites != null && primarySprites.Length > 0)
        {
            return primaryPath;
        }

        string fallbackPath = BuildUngenderedFallbackPath(data, isShiny, formIndex);

        if (!string.IsNullOrEmpty(fallbackPath))
        {
            Sprite[] fallbackSprites = Resources.LoadAll<Sprite>(fallbackPath);

            if (fallbackSprites != null && fallbackSprites.Length > 0)
            {
                return fallbackPath;
            }
        }

        return primaryPath;
    }

    private static string BuildUngenderedFallbackPath(PokemonData data, bool isShiny, int formIndex)
    {
        if (data == null)
        {
            return string.Empty;
        }

        if (data.genderVisualType != GenderVisualType.DifferentVisual)
        {
            return string.Empty;
        }

        string id = data.id.ToString("D3");
        string rarityPart = isShiny ? "shiny" : "normal";
        string path = "Pokemon/" + id + "_" + rarityPart;

        if (formIndex > 0)
        {
            path += "_" + formIndex;
        }

        return path;
    }
}