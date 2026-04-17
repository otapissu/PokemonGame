using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvolveBagSlotUI : MonoBehaviour
{
    [SerializeField] private Image      iconImage;
    [SerializeField] private TMP_Text   amountText;
    [SerializeField] private GameObject selectUI;

    public string            ItemName          { get; private set; }
    public Sprite            Icon              { get; private set; }
    public EvolutionItemType EvolutionItemType { get; private set; }

    private void Awake()
    {
        Button btn = GetComponentInChildren<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClick);
    }

    public void Setup(ShopItemData data, int count)
    {
        ItemName          = data.itemName;
        Icon              = data.icon;
        EvolutionItemType = data.evolutionItemType;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (selectUI  != null) selectUI.SetActive(false);

        UpdateCount(count);
    }

    public void UpdateCount(int count)
    {
        if (amountText != null)
            amountText.text = "X" + count;
    }

    public void SetSelected(bool selected)
    {
        if (selectUI != null)
            selectUI.SetActive(selected);
    }

    private void OnClick()
    {
        EvolveBagPanelController.Instance?.OnSlotClicked(this);
    }
}
