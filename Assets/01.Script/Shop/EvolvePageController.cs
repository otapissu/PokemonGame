using UnityEngine;

public class EvolvePageController : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ShopItemData[] evolveItems;

    [Header("Scroll View")]
    [Tooltip("ScrollView 안 Content RectTransform")]
    [SerializeField] private RectTransform content;

    [Header("Prefab")]
    [SerializeField] private GameObject itemButtonPrefab;

    [Header("Layout Scaler (선택)")]
    [Tooltip("버튼 생성 후 스케일 갱신용 — 없으면 무시")]
    [SerializeField] private ShopLayoutScaler layoutScaler;

    private void Start()
    {
        SpawnButtons();
    }

    private void SpawnButtons()
    {
        if (itemButtonPrefab == null || content == null) return;

        // 씬에서 미리 만들어둔 자식 제거
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        foreach (ShopItemData data in evolveItems)
        {
            if (data == null) continue;

            GameObject obj = Instantiate(itemButtonPrefab, content);
            if (obj.TryGetComponent(out ShopItemButton btn))
                btn.SetData(data);
        }

        // 버튼 생성 후 스케일 즉시 갱신
        if (layoutScaler != null)
            layoutScaler.RefreshScale();
    }
}
