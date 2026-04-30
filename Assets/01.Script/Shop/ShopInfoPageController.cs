using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ShopInfoPageController : MonoBehaviour
{
    public static ShopInfoPageController Instance { get; private set; }

    [Header("Info Page UI")]
    [SerializeField] private GameObject infoPage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text ownedCountText;

    [Header("Amount Buttons")]
    [SerializeField] private Button plusOneButton;
    [SerializeField] private Button plusTenButton;
    [SerializeField] private Button minusOneButton;
    [SerializeField] private Button minusTenButton;

    [Header("Buy Button")]
    [SerializeField] private Button buyButton;

    public bool IsInfoOpen => infoPage != null && infoPage.activeSelf;

    private ShopItemData currentItem;
    private int currentAmount;

    private const int MaxAmount = 999;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (infoPage != null)
        {
            infoPage.SetActive(false);
        }

        if (plusOneButton != null)
        {
            plusOneButton.onClick.AddListener(() => ChangeAmount(1));
        }
        if (plusTenButton != null)
        {
            plusTenButton.onClick.AddListener(() => ChangeAmount(10));
        }
        if (minusOneButton != null)
        {
            minusOneButton.onClick.AddListener(() => ChangeAmount(-1));
        }
        if (minusTenButton != null)
        {
            minusTenButton.onClick.AddListener(() => ChangeAmount(-10));
        }
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyClick);
        }

        if (PlayerGeneralInventory.Instance != null)
        {
            PlayerGeneralInventory.Instance.OnChanged += OnInventoryChanged;
        }
        if (PlayerEvolveInventory.Instance != null)
        {
            PlayerEvolveInventory.Instance.OnChanged += OnInventoryChanged;
        }
    }

    private void OnDestroy()
    {
        if (PlayerGeneralInventory.Instance != null)
        {
            PlayerGeneralInventory.Instance.OnChanged -= OnInventoryChanged;
        }
        if (PlayerEvolveInventory.Instance != null)
        {
            PlayerEvolveInventory.Instance.OnChanged -= OnInventoryChanged;
        }
    }

    private void OnInventoryChanged()
    {
        if (infoPage != null && infoPage.activeSelf)
        {
            UpdateOwnedCountText();
        }
    }


    // ShopItemButton에서 호출 — 같은 아이템 누르면 토글, 다른 아이템 누르면 전환
    public void ToggleInfo(ShopItemData data)
    {
        if (data == null) { return; }

        bool isSameItem = infoPage != null && infoPage.activeSelf && currentItem?.itemName == data.itemName;

        if (isSameItem)
        {
            HideInfo();
            return;
        }

        ShowInfo(data);
    }

    public void HideInfo()
    {
        if (infoPage != null)
        {
            infoPage.SetActive(false);
        }

        currentItem = null;
        ResetAmount();
    }

    private void ShowInfo(ShopItemData data)
    {
        currentItem = data;
        ResetAmount();

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
        }
        if (nameText != null)
        {
            nameText.text = data.itemName;
        }
        if (infoText != null)
        {
            infoText.text = data.description;
        }
        if (priceText != null)
        {
            priceText.text = data.price.ToString("N0") + "원";
        }

        UpdateOwnedCountText();

        if (infoPage != null)
        {
            infoPage.SetActive(true);
        }
    }

    private void ChangeAmount(int delta)
    {
        if (delta < 0 && currentAmount == 0)
        {
            currentAmount = MaxAmount;
        }
        else if (delta > 0 && currentAmount == MaxAmount)
        {
            currentAmount = 1;
        }
        else
        {
            currentAmount = Mathf.Clamp(currentAmount + delta, 0, MaxAmount);
        }

        UpdateAmountText();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShopSelect();
        }
    }

    private void OnBuyClick()
    {
        if (currentItem == null || currentAmount == 0)
        {
            Debug.LogWarning($"[Shop] 구매 취소 — item:{currentItem}, amount:{currentAmount}");
            return;
        }

        long totalCost = (long)currentItem.price * currentAmount;

        EggEnhanceController enhancer = EggEnhanceController.Instance;
        if (enhancer == null)
        {
            Debug.LogWarning("[Shop] EggEnhanceController.Instance가 null");
            return;
        }
        if (enhancer.gold < totalCost)
        {
            Debug.LogWarning($"[Shop] 골드 부족 — 필요:{totalCost}, 보유:{enhancer.gold}");
            return;
        }

        enhancer.gold -= totalCost;
        enhancer.RefreshGoldUI();

        if (currentItem.itemType == ShopItemType.General)
        {
            if (PlayerGeneralInventory.Instance != null)
            {
                PlayerGeneralInventory.Instance.AddItem(currentItem, currentAmount);
            }
            else                 Debug.LogWarning("[Shop] PlayerGeneralInventory.Instance가 null");
        }
        else
        {
            if (PlayerEvolveInventory.Instance != null)
            {
                PlayerEvolveInventory.Instance.AddItem(currentItem, currentAmount);
            }
            else                 Debug.LogWarning("[Shop] PlayerEvolveInventory.Instance가 null");
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShopBuy();
        }

        LogInventory();
        ResetAmount();
    }

    private void LogInventory()
    {
        if (currentItem.itemType == ShopItemType.General)
        {
            if (PlayerGeneralInventory.Instance == null) { Debug.LogWarning("[Shop] GeneralInventory null"); return; }
            var entries = PlayerGeneralInventory.Instance.Entries;
            System.Text.StringBuilder sb = new($"[GeneralInventory] 총 {entries.Count}종 | ");
            foreach (var e in entries)
            {
                sb.Append(e.item.itemName).Append(" x").Append(e.count).Append(", ");
            }
            Debug.Log(sb.ToString());
        }
        else
        {
            if (PlayerEvolveInventory.Instance == null) { Debug.LogWarning("[Shop] EvolveInventory null"); return; }
            var entries = PlayerEvolveInventory.Instance.Entries;
            System.Text.StringBuilder sb = new($"[EvolveInventory] 총 {entries.Count}종 | ");
            foreach (var e in entries)
            {
                sb.Append(e.item.itemName).Append(" x").Append(e.count).Append(", ");
            }
            Debug.Log(sb.ToString());
        }
    }

    private void UpdateOwnedCountText()
    {
        if (ownedCountText == null || currentItem == null)
        {
            return;
        }

        int count = 0;
        if (currentItem.itemType == ShopItemType.General)
        {
            if (PlayerGeneralInventory.Instance != null)
            {
                foreach (var e in PlayerGeneralInventory.Instance.Entries)
                {
                    if (e.item != null && e.item.itemName == currentItem.itemName)
                    {
                        count = e.count;
                        break;
                    }
                }
            }
        }
        else
        {
            if (PlayerEvolveInventory.Instance != null)
            {
                foreach (var e in PlayerEvolveInventory.Instance.Entries)
                {
                    if (e.item != null && e.item.itemName == currentItem.itemName)
                    {
                        count = e.count;
                        break;
                    }
                }
            }
        }

        ownedCountText.text = ($"{count}");
    }

    private void ResetAmount()
    {
        currentAmount = 1;
        UpdateAmountText();
    }

    private void UpdateAmountText()
    {
        if (amountText != null)
        {
            amountText.text = currentAmount.ToString();
        }
    }
}
