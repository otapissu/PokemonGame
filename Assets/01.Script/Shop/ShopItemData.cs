using UnityEngine;

public enum ShopItemType { General, Evolve }

[System.Serializable]
public class ShopItemData
{
    public string itemName;
    public Sprite icon;
    [TextArea(2, 5)]
    public string description;
    public int price;
    public ShopItemType itemType;

    [Tooltip("Evolve 아이템인 경우 대응하는 진화 아이템 타입")]
    public EvolutionItemType evolutionItemType;
}
