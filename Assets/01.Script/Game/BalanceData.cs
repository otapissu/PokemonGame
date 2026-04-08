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
    public int[] enhanceCosts =
    {
        500, 1000, 2000, 4000, 10000,
        15000, 25000, 50000, 100000, 200000,
        250000, 500000, 1000000, 1500000, 2000000
    };

    [Header("Sell Prices")]
    public int[] sellPrices =
    {
        0, 500, 1000, 2000, 6000,
        20000, 50000, 150000, 500000,
        1000000, 2000000, 5000000,
        10000000, 25000000, 50000000
    };
}