using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolveBagPanelController : MonoBehaviour
{
    public static EvolveBagPanelController Instance { get; private set; }

    [Header("슬라이드 대상")]
    [Tooltip("슬라이드할 패널 RectTransform")]
    [SerializeField] private RectTransform panelRect;
    [Tooltip("패널과 함께 움직일 가방 아이콘 RectTransform")]
    [SerializeField] private RectTransform bagIconRect;

    [Header("슬롯")]
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject slotPrefab;

    [Header("선택 아이템")]
    [SerializeField] private Image selectedItemImage;

    private readonly Dictionary<string, EvolveBagSlotUI> spawnedSlots = new();
    private EvolveBagSlotUI selectedSlot;

    public EvolutionItemType SelectedItemType =>
        selectedSlot != null ? selectedSlot.EvolutionItemType : EvolutionItemType.None;

    [Header("슬라이드 설정")]
    [Tooltip("오른쪽으로 숨길 거리 (패널 너비와 맞추세요)")]
    [SerializeField] private float slideDistance = 400f;
    [SerializeField] private float slideDuration = 0.25f;

    private bool isOpen = false;
    private Coroutine slideCoroutine;

    // Inspector 기준 위치 (= 닫힌 위치)
    private float panelClosedX;
    private float bagIconClosedX;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        panelClosedX   = panelRect   != null ? panelRect.anchoredPosition.x   : 0f;
        bagIconClosedX = bagIconRect != null ? bagIconRect.anchoredPosition.x : 0f;

        if (PlayerEvolveInventory.Instance != null)
            PlayerEvolveInventory.Instance.OnChanged += RefreshSlots;

        // Awake에서 Load 완료됐으므로 Start 시점엔 데이터 보장
        RefreshSlots();
    }

    private void OnDestroy()
    {
        if (PlayerEvolveInventory.Instance != null)
            PlayerEvolveInventory.Instance.OnChanged -= RefreshSlots;
    }

    // ─────────────────────────────────────────
    // 선택 처리
    // ─────────────────────────────────────────

    public void OnSlotClicked(EvolveBagSlotUI slot)
    {
        if (selectedSlot == slot)
        {
            // 같은 슬롯 다시 누르면 선택 해제
            selectedSlot.SetSelected(false);
            selectedSlot = null;
            if (selectedItemImage != null) selectedItemImage.gameObject.SetActive(false);
            return;
        }

        if (selectedSlot != null) selectedSlot.SetSelected(false);

        selectedSlot = slot;
        selectedSlot.SetSelected(true);

        if (selectedItemImage != null)
        {
            selectedItemImage.sprite = slot.Icon;
            selectedItemImage.gameObject.SetActive(true);
        }
    }

    public void ClearSelection()
    {
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
            selectedSlot = null;
        }

        if (selectedItemImage != null)
            selectedItemImage.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────
    // 슬롯 관리
    // ─────────────────────────────────────────

    private void RefreshSlots()
    {
        if (slotPrefab == null || content == null) return;
        if (PlayerEvolveInventory.Instance == null) return;

        var entries = PlayerEvolveInventory.Instance.Entries;

        // 새 아이템 추가 or 기존 아이템 카운트 갱신
        foreach (InventoryEntry entry in entries)
        {
            if (entry.item == null) continue;
            string name = entry.item.itemName;

            if (spawnedSlots.TryGetValue(name, out EvolveBagSlotUI existing))
            {
                existing.UpdateCount(entry.count);
            }
            else
            {
                GameObject obj = Instantiate(slotPrefab, content);
                EvolveBagSlotUI slot = obj.GetComponent<EvolveBagSlotUI>();
                if (slot != null)
                {
                    slot.Setup(entry.item, entry.count);
                    spawnedSlots[name] = slot;
                }
            }
        }

        // 인벤토리에서 사라진 아이템 슬롯 파괴
        var toRemove = new List<string>();
        foreach (var kv in spawnedSlots)
        {
            bool stillExists = false;
            foreach (InventoryEntry entry in entries)
            {
                if (entry.item != null && entry.item.itemName == kv.Key)
                {
                    stillExists = true;
                    break;
                }
            }
            if (!stillExists) toRemove.Add(kv.Key);
        }

        foreach (string key in toRemove)
        {
            if (spawnedSlots[key] != null)
                Destroy(spawnedSlots[key].gameObject);
            spawnedSlots.Remove(key);
        }
    }

    // ─────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────

    public void Toggle()
    {
        if (isOpen) Close();
        else        Open();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBagOpen();

        StartSlide(panelClosedX - slideDistance, bagIconClosedX - slideDistance);
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayButtonClose();

        // 둘 다 원래 위치로 복귀
        StartSlide(panelClosedX, bagIconClosedX);
    }

    // ─────────────────────────────────────────
    // 슬라이드 애니메이션
    // ─────────────────────────────────────────

    private void StartSlide(float targetPanelX, float targetIconX)
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideRoutine(targetPanelX, targetIconX));
    }

    private IEnumerator SlideRoutine(float targetPanelX, float targetIconX)
    {
        float startPanelX = panelRect != null   ? panelRect.anchoredPosition.x   : 0f;
        float startIconX  = bagIconRect != null ? bagIconRect.anchoredPosition.x : 0f;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);

            SetX(Mathf.Lerp(startPanelX, targetPanelX, t),
                 Mathf.Lerp(startIconX,  targetIconX,  t));

            yield return null;
        }

        SetX(targetPanelX, targetIconX);
    }

    private void SetX(float panelX, float iconX)
    {
        if (panelRect != null)
        {
            Vector2 pos = panelRect.anchoredPosition;
            pos.x = panelX;
            panelRect.anchoredPosition = pos;
        }

        if (bagIconRect != null)
        {
            Vector2 pos = bagIconRect.anchoredPosition;
            pos.x = iconX;
            bagIconRect.anchoredPosition = pos;
        }
    }
}
