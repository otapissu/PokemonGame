using UnityEngine;

public class PokedexManager : MonoBehaviour
{
    public GameObject dexSlotPrefab;
    public Transform slotContainer;

    public int maxDexCount = 151;   // Inspector에서 늘리면 됨

    private int currentPage = 0;
    private int itemsPerPage = 25;

    public void GenerateCurrentPage()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        int startIndex = currentPage * itemsPerPage;

        for (int i = 0; i < itemsPerPage; i++)
        {
            int id = startIndex + i + 1;

            if (id > maxDexCount)
                break;

            GameObject slot = Instantiate(dexSlotPrefab, slotContainer);
            slot.GetComponent<DexSlotUI>().Setup(id);
        }
    }

    public void NextPage()
    {
        int totalPages = Mathf.CeilToInt((float)maxDexCount / itemsPerPage);

        currentPage++;
        if (currentPage >= totalPages)
            currentPage = 0;

        GenerateCurrentPage();
    }

    public void PrevPage()
    {
        int totalPages = Mathf.CeilToInt((float)maxDexCount / itemsPerPage);

        currentPage--;
        if (currentPage < 0)
            currentPage = totalPages - 1;

        GenerateCurrentPage();
    }
}