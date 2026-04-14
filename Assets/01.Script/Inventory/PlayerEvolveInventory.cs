// 진화 아이템 가방
public class PlayerEvolveInventory : PlayerInventoryBase
{
    public static PlayerEvolveInventory Instance { get; private set; }

    protected override string SaveKey => "EVOLVE_INVENTORY";

    private void Awake()
    {
        Instance = this;
    }

    // EggDataService에서 진화 가능 여부 확인 시 사용
    public int GetCount(EvolutionItemType type)
    {
        foreach (InventoryEntry e in Entries)
        {
            if (e.item != null && e.item.evolutionItemType == type)
                return e.count;
        }
        return 0;
    }

    public bool HasItem(EvolutionItemType type)
    {
        return GetCount(type) > 0;
    }

    public void ConsumeItem(EvolutionItemType type)
    {
        InventoryEntry entry = null;
        foreach (InventoryEntry e in Entries)
        {
            if (e.item != null && e.item.evolutionItemType == type)
            {
                entry = e;
                break;
            }
        }

        if (entry != null)
            RemoveItem(entry.item);
    }
}
