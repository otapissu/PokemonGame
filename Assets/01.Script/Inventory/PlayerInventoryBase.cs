using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryEntry
{
    public ShopItemData item;
    public int count;

    public InventoryEntry(ShopItemData item, int count)
    {
        this.item  = item;
        this.count = count;
    }
}

// JSON 직렬화용 — itemName으로 저장
[System.Serializable]
public class InventorySaveEntry
{
    public string itemName;
    public int count;
}

[System.Serializable]
public class InventorySaveData
{
    public List<InventorySaveEntry> entries = new();
}

// 아이템 가방 공통 베이스. GeneralInventory / EvolveInventory가 상속.
public abstract class PlayerInventoryBase : MonoBehaviour
{
    [Header("아이템 목록 (로드 시 itemName 매칭용)")]
    [SerializeField] private ShopItemData[] allItems;

    protected List<InventoryEntry> entries = new();

    public IReadOnlyList<InventoryEntry> Entries => entries;

    public event System.Action OnChanged;

    protected abstract string SaveKey { get; }

    protected virtual void Start()
    {
        Load();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void Reset()
    {
        entries.Clear();
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    // 같은 아이템이 있으면 count 누적, 없으면 새 항목 추가
    public void AddItem(ShopItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return;
        }

        InventoryEntry existing = entries.Find(e => e.item.itemName == item.itemName);
        if (existing != null)
        {
            existing.count += amount;
        }
        else
        {
            entries.Add(new InventoryEntry(item, amount));
        }

        Save();
        OnChanged?.Invoke();
    }

    public void RemoveItem(ShopItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return;
        }
        RemoveItem(item.itemName, amount);
    }

    public void RemoveItem(string itemName, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemName) || amount <= 0)
        {
            return;
        }

        InventoryEntry existing = entries.Find(e => e.item.itemName == itemName);
        if (existing == null)
        {
            return;
        }

        existing.count -= amount;
        if (existing.count <= 0)
        {
            entries.Remove(existing);
        }

        Save();
        OnChanged?.Invoke();
    }

    private void Save()
    {
        InventorySaveData data = new();
        foreach (InventoryEntry e in entries)
        {
            if (e.item == null)
            {
                continue;
            }
            data.entries.Add(new InventorySaveEntry { itemName = e.item.itemName, count = e.count });
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return;
        }
        if (allItems == null || allItems.Length == 0)
        {
            Debug.LogWarning($"[Inventory] {SaveKey} — allItems가 비어 있어 로드 불가");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

        entries.Clear();
        foreach (InventorySaveEntry saved in data.entries)
        {
            ShopItemData found = System.Array.Find(allItems, i => i.itemName == saved.itemName);
            if (found != null)
            {
                entries.Add(new InventoryEntry(found, saved.count));
            }
            else
            {
                Debug.LogWarning($"[Inventory] 저장된 아이템 '{saved.itemName}'을 찾을 수 없음");
            }
        }

        OnChanged?.Invoke();
    }
}
