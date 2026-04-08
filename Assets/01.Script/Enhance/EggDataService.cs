using UnityEngine;

public static class EggDataService
{
    // 강화 성공 확률
    public static float GetSuccessRate(int level)
    {
        float[] rates = { 1f, 0.9f, 0.8f, 0.7f, 0.65f, 0.6f, 0.5f, 0.45f, 0.4f, 0.3f, 0.26f, 0.22f, 0.18f, 0.14f, 0.1f };
        return rates[level - 1];
    }

    // 강화 파괴 확률
    public static float GetDestroyRate(int level)
    {
        if (level < 6)
        {
            return 0f;
        }

        float[] rates = { 0.05f, 0.06f, 0.07f, 0.08f, 0.1f, 0.11f, 0.12f, 0.13f, 0.14f, 0.15f };
        return rates[level - 6];
    }

    // 강화 비용
    public static int GetEnhanceCost(int level)
    {
        int[] cost = { 500, 1000, 2000, 4000, 10000, 15000, 25000, 50000, 100000, 200000, 250000, 500000, 1000000, 1500000, 2000000 };
        return cost[level - 1];
    }

    // 판매 비용 조정
    public static int GetSellPrice(EggEnhanceController c)
    {
        int level = Mathf.Clamp(c.currentInstance.enhanceLevel, 1, 15);
        int basePrice = c.sellPrices[level - 1];

        float multiplier = 1f;

        if (c.currentInstance.data.isLegendary)
        {
            multiplier *= 1.5f;
        }

        if (c.currentInstance.isShiny)
        {
            multiplier *= 2f;
        }

        return Mathf.RoundToInt(basePrice * multiplier);
    }
}