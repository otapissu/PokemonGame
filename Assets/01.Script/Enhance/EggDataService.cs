using UnityEngine;

public static class EggDataService
{
    public static float GetSuccessRate(EggEnhanceController c, int level)
    {
        if (c == null)
        {
            return 0f;
        }

        if (c.balanceData == null)
        {
            Debug.LogError("balanceData가 초기화되지 않았습니다.");
            return 0f;
        }

        if (c.balanceData.successRates == null || c.balanceData.successRates.Length == 0)
        {
            Debug.LogError("successRates가 비어 있습니다.");
            return 0f;
        }

        int index = Mathf.Clamp(level - 1, 0, c.balanceData.successRates.Length - 1);
        return c.balanceData.successRates[index];
    }

    public static float GetDestroyRate(EggEnhanceController c, int level)
    {
        if (level < 6)
        {
            return 0f;
        }

        if (c == null)
        {
            return 0f;
        }

        if (c.balanceData == null)
        {
            Debug.LogError("balanceData가 초기화되지 않았습니다.");
            return 0f;
        }

        if (c.balanceData.destroyRates == null || c.balanceData.destroyRates.Length == 0)
        {
            Debug.LogError("destroyRates가 비어 있습니다.");
            return 0f;
        }

        int index = Mathf.Clamp(level - 6, 0, c.balanceData.destroyRates.Length - 1);
        return c.balanceData.destroyRates[index];
    }

    public static long GetEnhanceCost(EggEnhanceController c, int level)
    {
        if (c == null)
        {
            return 0L;
        }

        if (c.balanceData == null)
        {
            Debug.LogError("balanceData가 초기화되지 않았습니다.");
            return 0L;
        }

        if (c.balanceData.enhanceCosts == null || c.balanceData.enhanceCosts.Length == 0)
        {
            Debug.LogError("enhanceCosts가 비어 있습니다.");
            return 0L;
        }

        int index = Mathf.Clamp(level - 1, 0, c.balanceData.enhanceCosts.Length - 1);
        return c.balanceData.enhanceCosts[index];
    }

    public static long GetSellPrice(EggEnhanceController c)
    {
        if (c == null)
        {
            return 0L;
        }

        if (c.currentInstance == null)
        {
            return 0L;
        }

        if (c.currentInstance.data == null)
        {
            return 0L;
        }

        if (c.balanceData == null)
        {
            Debug.LogError("balanceData가 초기화되지 않았습니다.");
            return 0L;
        }

        if (c.balanceData.sellPrices == null || c.balanceData.sellPrices.Length == 0)
        {
            Debug.LogError("sellPrices가 비어 있습니다.");
            return 0L;
        }

        int level = Mathf.Clamp(c.currentInstance.enhanceLevel, 1, 15);
        long basePrice = c.balanceData.sellPrices[level - 1];

        float multiplier = 1f;

        if (c.currentInstance.data.isLegendary)
        {
            multiplier *= 1.5f;
        }

        if (c.currentInstance.isShiny)
        {
            multiplier *= 2f;
        }

        long finalPrice = (long)(basePrice * multiplier);

        if (HasMissedItemEvolution(c.currentInstance))
        {
            finalPrice /= 2;
        }

        return finalPrice;
    }

    private static bool HasMissedItemEvolution(PokemonInstance instance)
    {
        if (instance == null)
        {
            return false;
        }

        if (instance.data == null)
        {
            return false;
        }


        if (instance.data.evolutionOptions == null)
        {
            return false;
        }

        for (int i = 0; i < instance.data.evolutionOptions.Length; i++)
        {
            EvolutionOption option = instance.data.evolutionOptions[i];

            if (option == null)
            {
                continue;
            }

            if (option.targetData == null)
            {
                continue;
            }

            if (option.method != EvolutionMethod.Item)
            {
                continue;
            }

            if (!IsMatchingSourceForm(option, instance))
            {
                continue;
            }

            if (instance.enhanceLevel < option.requiredEnhanceLevel)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool IsMatchingSourceForm(EvolutionOption option, PokemonInstance instance)
    {
        if (option.requiredSourceFormIndex < 0)
        {
            return true;
        }

        return option.requiredSourceFormIndex == instance.formIndex;
    }

}
