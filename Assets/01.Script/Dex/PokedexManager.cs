using UnityEngine;
using System.Collections.Generic;

public class PokedexManager : MonoBehaviour
{
    public GameObject dexSlotPrefab;
    public Transform slotContainer;

    public int maxDexCount = 151;

    private int currentPage = 0;
    private int itemsPerPage = 25;

    private readonly List<DexSlotUI> slotPool = new();
    private bool isInitialized = false;

    private readonly Dictionary<int, Sprite[]> spriteCache = new();

    private float animTimer = 0f;
    private float animInterval = 0.4f;
    private int animFrame = 0;

    private void OnEnable()
    {
        animTimer = 0f;
    }

    private void Update()
    {
        animTimer += Time.deltaTime;

        if (animTimer < animInterval)
        {
            return;
        }

        animTimer -= animInterval;
        animFrame++;

        for (int i = 0; i < slotPool.Count; i++)
        {
            DexSlotUI slot = slotPool[i];

            if (slot.gameObject.activeSelf)
            {
                slot.UpdateFrame(animFrame);
            }
        }
    }

    private void InitializeSlots()
    {
        if (isInitialized)
        {
            return;
        }

        for (int i = 0; i < itemsPerPage; i++)
        {
            GameObject slot = Instantiate(dexSlotPrefab, slotContainer);
            slotPool.Add(slot.GetComponent<DexSlotUI>());
        }

        isInitialized = true;
    }

    private Sprite[] GetCachedSprites(int id)
    {
        if (spriteCache.TryGetValue(id, out Sprite[] cached))
        {
            return cached;
        }

        Sprite[] loaded = Resources.LoadAll<Sprite>("Icon/" + id.ToString("D3"));
        spriteCache[id] = loaded;
        return loaded;
    }

    public void GenerateCurrentPage()
    {
        InitializeSlots();

        int startIndex = currentPage * itemsPerPage;

        for (int i = 0; i < itemsPerPage; i++)
        {
            int id = startIndex + i + 1;
            DexSlotUI slot = slotPool[i];

            if (id > maxDexCount)
            {
                slot.gameObject.SetActive(false);
            }
            else
            {
                slot.gameObject.SetActive(true);
                slot.Setup(id, GetCachedSprites(id));
            }
        }
    }

    public void NextPage()
    {
        int totalPages = Mathf.CeilToInt((float)maxDexCount / itemsPerPage);

        currentPage++;
        if (currentPage >= totalPages)
        {
            currentPage = 0;
        }

        GenerateCurrentPage();
    }

    public void PrevPage()
    {
        int totalPages = Mathf.CeilToInt((float)maxDexCount / itemsPerPage);

        currentPage--;
        if (currentPage < 0)
        {
            currentPage = totalPages - 1;
        }

        GenerateCurrentPage();
    }
}
