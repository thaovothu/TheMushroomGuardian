using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Inventory UI chia 3 row cố định:
///   Row 1 — Vũ Khí:     Sword, Bow
///   Row 2 — Bình Thuốc: HealthPotion, ManaPotion, StrengthBuff, DefenseBuff
///   Row 3 — Nguyên Tố:  EarthCrystal, WindCrystal, WaterCrystal, FireCrystal
/// Mỗi slot gắn cố định với 1 ItemType — hiện số lượng khi có item.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Row containers")]
    [SerializeField] private Transform rowWeapon;    // Parent chứa slots vũ khí
    [SerializeField] private Transform rowPotion;    // Parent chứa slots bình
    [SerializeField] private Transform rowElement;   // Parent chứa slots nguyên tố

    [Header("Slot Prefab")]
    [SerializeField] private InventorySlotUI slotPrefab;

    [Header("Detail Panel")]
    [SerializeField] private ItemDetailUI itemDetailUI;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    // Item types cho mỗi row — thứ tự = thứ tự slot trong row
    private static readonly ItemType[] WeaponTypes = { ItemType.Sword, ItemType.Bow };
    private static readonly ItemType[] PotionTypes = { ItemType.HealthPotion, ItemType.ManaPotion, ItemType.StrengthBuff, ItemType.DefenseBuff };
    private static readonly ItemType[] ElementTypes = { ItemType.EarthCrystal, ItemType.WindCrystal, ItemType.WaterCrystal, ItemType.FireCrystal };

    // Lookup: ItemType → slot UI
    private Dictionary<ItemType, InventorySlotUI> slotByType = new Dictionary<ItemType, InventorySlotUI>();

    private CanvasGroup canvasGroup;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        BuildRows();
        Hide();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    private void OnEnable()
    {
        GameEvent.Inventory.OnSlotChanged += OnInventoryChanged;
        GameEvent.Item.OnItemPickedUp += OnItemPickedUp;
    }

    private void OnDisable()
    {
        GameEvent.Inventory.OnSlotChanged -= OnInventoryChanged;
        GameEvent.Item.OnItemPickedUp -= OnItemPickedUp;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        RefreshAll();
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        if (itemDetailUI != null) itemDetailUI.Hide();
    }

    public void Toggle()
    {
        if (canvasGroup.alpha > 0) Hide();
        else Show();
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    private void BuildRows()
    {
        BuildRow(rowWeapon, WeaponTypes);
        BuildRow(rowPotion, PotionTypes);
        BuildRow(rowElement, ElementTypes);
    }

    private void BuildRow(Transform parent, ItemType[] types)
    {
        if (parent == null || slotPrefab == null) return;

        // Xóa slot cũ
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (var type in types)
        {
            var slot = Instantiate(slotPrefab, parent);
            slot.InitializeByType(type, this);
            slotByType[type] = slot;
        }
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        foreach (var pair in slotByType)
            pair.Value.RefreshByType(pair.Key);
    }

    private void OnInventoryChanged(int bagIndex, int slotIndex)
    {
        RefreshAll();
    }

    private void OnItemPickedUp(int itemId, int amount)
    {
        RefreshAll();
    }

    public void OnSlotClicked(ItemType type)
    {
        if (itemDetailUI == null) return;

        var quantity = InventorySystem.Instance?.GetItemQuantity(type) ?? 0;
        if (quantity > 0)
            itemDetailUI.ShowItemDetailByType(type, quantity);
        else
            itemDetailUI.ClearDetail();
    }
}