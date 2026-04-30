using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UsageCondition
{
    RequiresEgg,       // 알 상태에서만 사용 (부화 전) RequiresHatched,   // 부화된 상태에서만 사용 RequiresDestroyed, // 파괴된 상태에서만 사용 }

public class GeneralBagSlotUI : MonoBehaviour
{
    [SerializeField] private string itemName;    // 고정 아이템 이름 (인스펙터 연결)
    [SerializeField] private TMP_Text     countText;   // "X999"
    [SerializeField] private GameObject   selectUI;    // 선택 시 표시

    [Header("사용 조건")]
    [SerializeField] private UsageCondition usageCondition;
    [Tooltip("같은 ID끼리는 동시에 큐에 담을 수 없음. 0 = 제한 없음")]
    [SerializeField] private int exclusiveGroupId;

    [Header("아이템 효과")]
    [Tooltip("체크 시 이로치 확률을 5%로 고정 (ShinyCharm 전용)")]
    [SerializeField] private bool isShinyCharm;
    [Tooltip("체크 시 부화 포켓몬을 암컷으로 고정 (핑크향로 전용)")]
    [SerializeField] private bool isPinkIncense;
    [Tooltip("체크 시 부화 포켓몬을 수컷으로 고정 (파랑향로 전용)")]
    [SerializeField] private bool isBlueIncense;

    public string          ItemName         => itemName;
    public int             ExclusiveGroupId => exclusiveGroupId;
    public UsageCondition  Condition        => usageCondition;
    public bool            IsShinyCharm     => isShinyCharm;
    public bool            IsPinkIncense    => isPinkIncense;
    public bool            IsBlueIncense    => isBlueIncense;

    private void Awake()
    {
        Button btn = GetComponentInChildren<Button>(true);
        if (btn != null)
        {
            btn.onClick.AddListener(OnClick);
        }
    }

    public void RefreshCount(int count)
    {
        if (countText != null)
        {
            countText.text = "X" + count;
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectUI != null)
        {
            selectUI.SetActive(selected);
        }
    }

    public bool CanUseNow()
    {
        EggEnhanceController c = EggEnhanceController.Instance;
        if (c == null)
        {
            return false;
        }

        switch (usageCondition)
        {
            case UsageCondition.RequiresHatched:   return c.currentInstance != null;
            case UsageCondition.RequiresEgg:        return c.currentInstance == null && !c.IsDestroyed;
            case UsageCondition.RequiresDestroyed:  return c.IsDestroyed;
            default: return false;
        }
    }

    private void OnClick()
    {
        if (GeneralBagPanelController.Instance != null)
        {
            GeneralBagPanelController.Instance.OnSlotClicked(this);
        }
    }
}
