using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Inventory UI chia 3 row cố định:
///   Row 1 — Vũ Khí:     Sword, Bow
///   Row 2 — Bình Thuốc: HealthPotion, ManaPotion, StrengthBuff, DefenseBuff
///   Row 3 — Nguyên Tố:  EarthCrystal, WindCrystal, WaterCrystal, FireCrystal
/// Subscribe event trong Awake/OnDestroy — không dùng CanvasGroup.
/// </summary>
public class UIInventory : MonoBehaviour
{
    [Header("Row containers")]
    [SerializeField] private Transform rowWeapon;
    [SerializeField] private Transform rowPotion;
    [SerializeField] private Transform rowElement;

    [Header("Slot Prefab")]
    [SerializeField] private InventorySlotUI slotPrefab;

    [Header("Detail Panel")]
    [SerializeField] private ItemDetailUI itemDetailUI;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    private static readonly ItemType[] WeaponTypes = { ItemType.Sword, ItemType.Bow };
    private static readonly ItemType[] PotionTypes = { ItemType.HealthPotion, ItemType.ManaPotion, ItemType.StrengthBuff, ItemType.DefenseBuff };
    private static readonly ItemType[] ElementTypes = { ItemType.EarthCrystal, ItemType.WindCrystal, ItemType.WaterCrystal, ItemType.FireCrystal };

    private Dictionary<ItemType, InventorySlotUI> slotByType = new Dictionary<ItemType, InventorySlotUI>();
    private bool isBuilt = false;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        GameEvent.Inventory.OnSlotChanged += OnInventoryChanged;
        GameEvent.Item.OnItemPickedUp += OnItemPickedUp;
    }

    private void Start()
    {
        BuildRows();
        Hide();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    private void OnDestroy()
    {
        GameEvent.Inventory.OnSlotChanged -= OnInventoryChanged;
        GameEvent.Item.OnItemPickedUp -= OnItemPickedUp;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Show()
    {
        gameObject.SetActive(true);
        RefreshAll();
    }

    public void Hide()
    {
        if (itemDetailUI != null) itemDetailUI.Hide();
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (gameObject.activeSelf) Hide();
        else Show();
    }

    public bool IsOpen => gameObject.activeSelf;

    // ── Build ─────────────────────────────────────────────────────────────────

    private void BuildRows()
    {
        if (isBuilt) return;

        BuildRow(rowWeapon, WeaponTypes);
        BuildRow(rowPotion, PotionTypes);
        BuildRow(rowElement, ElementTypes);

        isBuilt = true;
        Debug.Log($"[InventoryUI] Built rows — slotByType count={slotByType.Count}");
    }

    private void BuildRow(Transform parent, ItemType[] types)
    {
        if (parent == null || slotPrefab == null) return;

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
        // Chỉ refresh khi đang mở
        if (gameObject.activeSelf) RefreshAll();
    }

    private void OnItemPickedUp(int itemId, int amount)
    {
        // Chỉ refresh khi đang mở
        if (gameObject.activeSelf) RefreshAll();
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