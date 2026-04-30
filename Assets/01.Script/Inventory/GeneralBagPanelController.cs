using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralBagPanelController : MonoBehaviour
{
    public static GeneralBagPanelController Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject bagPanel;

    [Header("Slots")]
    [SerializeField] private GeneralBagSlotUI[] slots;

    [Header("복구 비용 UI")]
    [Tooltip("파괴 상태일 때만 활성화되는 오브젝트")]
    [SerializeField] private GameObject revivalCostUI;
    [Tooltip("'X10' 형식으로 필요 갯수를 표시할 TMP 텍스트")]
    [SerializeField] private TMPro.TMP_Text revivalCostText;

    [Header("경고 UI")]
    [Tooltip("경고 이미지 GameObject (CanvasGroup 자동 추가됨)")]
    [SerializeField] private GameObject warningUI;
    [SerializeField] private float warningFadeDuration = 0.3f;
    [SerializeField] private float warningHoldDuration = 1.0f;

    private CanvasGroup warningCanvasGroup;
    private TMPro.TMP_Text warningText;

    private readonly List<GeneralBagSlotUI> selectedSlots = new();

    private Coroutine warningCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (bagPanel != null)
        {
            bagPanel.SetActive(false);
        }

        if (revivalCostUI != null)
        {
            revivalCostUI.SetActive(false);
        }

        if (warningUI != null)
        {
            warningCanvasGroup = warningUI.GetComponent<CanvasGroup>();
            if (warningCanvasGroup == null)
            {
                warningCanvasGroup = warningUI.AddComponent<CanvasGroup>();
            }

            warningText = warningUI.GetComponentInChildren<TMPro.TMP_Text>();

            warningCanvasGroup.alpha = 0f;
            warningUI.SetActive(false);
        }
    }

    // Open / Close / Toggle

    public void ToggleBag()
    {
        if (bagPanel == null)
        {
            return;
        }

        if (bagPanel.activeSelf)
        {
            CloseBag();
        }
        else
        {
            OpenBag();
        }
    }

    public void OpenBag()
    {
        if (bagPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBagOpen();
        }

        bagPanel.SetActive(true);
        RefreshSlots();
    }

    public void CloseBag()
    {
        if (bagPanel == null)
        {
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClose();
        }

        ClearSelection();
        bagPanel.SetActive(false);
    }

    // Slot refresh

    private void RefreshSlots()
    {
        RefreshCounts();
        RefreshSelectionDisplay();
        RefreshRevivalCostUI();
    }

    private int GetCount(string itemName)
    {
        if (PlayerGeneralInventory.Instance == null)
        {
            return 0;
        }
        var entries = PlayerGeneralInventory.Instance.Entries;
        foreach (var e in entries)
        {
            if (e.item.itemName == itemName)
            {
                return e.count;
            }
        }
        return 0;
    }

    // 선택 조작

    public void OnSlotClicked(GeneralBagSlotUI slot)
    {
        if (slot == null)
        {
            return;
        }

        // 이미 선택됐으면 제거 (토글)
        if (selectedSlots.Contains(slot))
        {
            selectedSlots.Remove(slot);
            RefreshSelectionDisplay();
            return;
        }

        // 아이템 0개 체크
        if (GetCount(slot.ItemName) <= 0)
        {
            ShowWarning("사용할 수 있는 아이템이 없습니다.");
            return;
        }

        // 상태 조건 체크 (알/부화)
        if (!slot.CanUseNow())
        {
            ShowWarning("이용할 수 있는 상태가 아닙니다.");
            return;
        }

        // 배타 그룹: 같은 그룹이 이미 선택됐으면 그걸 빼고 새 걸 넣음
        if (slot.ExclusiveGroupId != 0)
        {
            for (int i = selectedSlots.Count - 1; i >= 0; i--)
            {
                if (selectedSlots[i].ExclusiveGroupId == slot.ExclusiveGroupId)
                {
                    selectedSlots.RemoveAt(i);
                }
            }
        }

        selectedSlots.Add(slot);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShopSelect();
        }

        RefreshSelectionDisplay();
    }

    public void ApplySelectedItems()
    {
        foreach (GeneralBagSlotUI slot in selectedSlots)
        {
            UseItem(slot.ItemName);
        }

        ClearSelection();
        RefreshCounts();
    }

    public Gender GetGenderFilter()
    {
        foreach (GeneralBagSlotUI slot in selectedSlots)
        {
            if (slot.IsPinkIncense)
            {
                return Gender.Female;
            }
            if (slot.IsBlueIncense)
            {
                return Gender.Male;
            }
        }
        return Gender.None;
    }

    public bool IsShinyCharmSelected()
    {
        foreach (GeneralBagSlotUI slot in selectedSlots)
        {
            if (slot.IsShinyCharm)
            {
                return true;
            }
        }
        return false;
    }

    public float GetSuccessRateBonus()
    {
        foreach (GeneralBagSlotUI slot in selectedSlots)
        {
            if (slot.Condition == UsageCondition.RequiresHatched)
            {
                return 0.05f;
            }
        }
        return 0f;
    }

    public void ConsumeEnhanceItems()
    {
        for (int i = selectedSlots.Count - 1; i >= 0; i--)
        {
            GeneralBagSlotUI slot = selectedSlots[i];
            if (slot.Condition != UsageCondition.RequiresHatched)
            {
                continue;
            }

            if (PlayerGeneralInventory.Instance != null)
            {
                PlayerGeneralInventory.Instance.RemoveItem(slot.ItemName, 1);
            }

            // 0개가 되면 자동 선택 해제
            if (GetCount(slot.ItemName) <= 0)
            {
                selectedSlots.RemoveAt(i);
            }
        }

        RefreshCounts();
        RefreshSelectionDisplay();
    }

    public bool HasRevivalItemSelected()
    {
        foreach (GeneralBagSlotUI slot in selectedSlots)
        {
            if (slot.Condition == UsageCondition.RequiresDestroyed)
            {
                return true;
            }
        }
        return false;
    }

    public string GetRevivalItemName()
    {
        foreach (GeneralBagSlotUI slot in selectedSlots)
        {
            if (slot.Condition == UsageCondition.RequiresDestroyed)
            {
                return slot.ItemName;
            }
        }
        return null;
    }

    public void RefreshCounts()
    {
        foreach (GeneralBagSlotUI slot in slots)
        {
            if (slot == null || string.IsNullOrEmpty(slot.ItemName))
            {
                continue;
            }
            slot.RefreshCount(GetCount(slot.ItemName));
        }
    }

    public void HideRevivalCostUI()
    {
        if (revivalCostUI != null)
        {
            revivalCostUI.SetActive(false);
        }
    }

    public void ShowRevivalCostUI()
    {
        RefreshRevivalCostUI();
    }

    public void ClearSelection()
    {
        selectedSlots.Clear();
        RefreshSelectionDisplay();
    }

    private void RefreshSelectionDisplay()
    {
        if (slots == null)
        {
            return;
        }

        foreach (GeneralBagSlotUI slot in slots)
        {
            if (slot == null)
            {
                continue;
            }
            slot.SetSelected(selectedSlots.Contains(slot));
        }

        // 강화확률 텍스트 즉시 갱신
        EggEnhanceController c = EggEnhanceController.Instance;
        if (c != null && c.currentInstance != null)
        {
            new EggUIService().UpdateAll(c);
        }

        // 강화 버튼 텍스트 갱신 ("강화하기" / "되살리기")
        if (c != null)
        {
            c.RefreshEnhanceButtonText();
        }

        // 복구 비용 UI 갱신
        RefreshRevivalCostUI();
    }

    private void RefreshRevivalCostUI()
    {
        EggEnhanceController c = EggEnhanceController.Instance;
        bool isDestroyed = c != null && c.IsDestroyed;

        if (revivalCostUI != null)
        {
            revivalCostUI.SetActive(isDestroyed);
        }

        if (!isDestroyed || revivalCostText == null)
        {
            return;
        }

        // 필요 갯수 계산
        int cost = 1;
        if (c.DestroyedSnapshot != null && c.balanceData != null && c.balanceData.reviveCosts != null)
        {
            int idx = c.DestroyedSnapshot.destroyedAtLevel - 6;
            if (idx >= 0 && idx < c.balanceData.reviveCosts.Length)
            {
                cost = c.balanceData.reviveCosts[idx];
            }
        }

        revivalCostText.text = "X" + cost;
    }

    // 경고 UI 페이드인/아웃
    private void ShowWarning(string message)
    {
        if (warningUI == null)
        {
            return;
        }

        if (warningText != null)
        {
            warningText.text = message;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayEnhanceFail();
        }

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }

        warningCoroutine = StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        warningUI.SetActive(true);

        // 페이드인
        float t = 0f;
        while (t < warningFadeDuration)
        {
            t += Time.deltaTime;
            warningCanvasGroup.alpha = Mathf.Clamp01(t / warningFadeDuration);
            yield return null;
        }
        warningCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(warningHoldDuration);

        // 페이드아웃
        t = 0f;
        while (t < warningFadeDuration)
        {
            t += Time.deltaTime;
            warningCanvasGroup.alpha = 1f - Mathf.Clamp01(t / warningFadeDuration);
            yield return null;
        }

        warningCanvasGroup.alpha = 0f;
        warningUI.SetActive(false);
    }

    // 아이템 사용
    private void UseItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            return;
        }

        // 아이템별 효과 구현
        Debug.Log($"[GeneralBag] 아이템 사용: {itemName}");

        if (PlayerGeneralInventory.Instance != null)
        {
            PlayerGeneralInventory.Instance.RemoveItem(itemName, 1);
        }
    }
}
