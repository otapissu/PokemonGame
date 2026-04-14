using UnityEngine;

// 일반 아이템 가방
public class PlayerGeneralInventory : PlayerInventoryBase
{
    public static PlayerGeneralInventory Instance { get; private set; }

    protected override string SaveKey => "GENERAL_INVENTORY";

    private void Awake()
    {
        Instance = this;
    }
}
