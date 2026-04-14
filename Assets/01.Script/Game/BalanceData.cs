using UnityEngine;

[CreateAssetMenu(fileName = "BalanceData", menuName = "Game/Egg Balance Data")]
public class EggBalanceData : ScriptableObject
{
    [Header("Enhance Success Rates")]
    public float[] successRates =
    {
        1f, 0.9f, 0.8f, 0.7f, 0.65f,
        0.6f, 0.5f, 0.45f, 0.4f, 0.3f,
        0.26f, 0.22f, 0.18f, 0.14f, 0.1f
    };

    [Header("Destroy Rates (6~15)")]
    public float[] destroyRates =
    {
        0.05f, 0.06f, 0.07f, 0.08f, 0.1f,
        0.11f, 0.12f, 0.13f, 0.14f, 0.15f
    };

    [Header("Enhance Costs")]
    public long[] enhanceCosts =
    {
        500L, 1000L, 2000L, 4000L, 10000L,
        15000L, 25000L, 50000L, 100000L, 200000L,
        250000L, 500000L, 1000000L, 1500000L, 2000000L
    };

    [Header("Revival Costs (index 0 = 6강 파괴, index 1 = 7강 파괴 ...)")]
    public int[] reviveCosts = { 1, 2, 4, 6, 9, 12, 16, 20, 25, 30 };

    [Header("Sell Prices")]
    public long[] sellPrices =
    {
        0L, 500L, 1000L, 2000L, 6000L,
        20000L, 50000L, 150000L, 500000L,
        1000000L, 2000000L, 5000000L,
        10000000L, 25000000L, 50000000L
    };
}