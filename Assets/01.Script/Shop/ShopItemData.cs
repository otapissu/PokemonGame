using UnityEngine;

public enum ShopItemType { General, Evolve }

[CreateAssetMenu(fileName = "ShopItem_", menuName = "Shop/Item Data")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea(2, 5)]
    public string description;
    public int price;
    public ShopItemType itemType;

    [Tooltip("Evolve 아이템인 경우 대응하는 진화 아이템 타입")]
    public EvolutionItemType evolutionItemType;

    // TODO: 아이템 사용 효과 (아이템마다 로직이 달라서 나중에 추가)
}
