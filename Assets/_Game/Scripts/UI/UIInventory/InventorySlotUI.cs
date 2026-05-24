using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1 slot trong InventoryUI — gắn cố định với 1 ItemType.
/// Hiện icon + số lượng, ẩn icon khi chưa có item.
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button slotButton;
    [SerializeField] private Image slotBackground;

    private ItemType itemType;
    private UIInventory inventoryUI;

    private Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    private Color filledColor = Color.white;

    private void Awake()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(OnClicked);
    }

    // ── API ───────────────────────────────────────────────────────────────────

    public void InitializeByType(ItemType type, UIInventory parent)
    {
        itemType = type;
        inventoryUI = parent;
        RefreshByType(type);
    }

    public void RefreshByType(ItemType type)
    {
        if (InventorySystem.Instance == null) return;

        int qty = InventorySystem.Instance.GetItemQuantity(type);
        var icon = InventorySystem.Instance.GetItemIconByType(type);

        Debug.Log($"[InventorySlotUI] RefreshByType {type} → qty={qty}, icon={icon?.name ?? "NULL"}");

        if (qty <= 0)
        {
            if (itemIcon != null) { itemIcon.sprite = null; itemIcon.enabled = false; }
            if (quantityText != null) quantityText.text = "";
            if (slotBackground != null) slotBackground.color = emptyColor;
        }
        else
        {
            if (itemIcon != null)
            {
                itemIcon.enabled = true;
                itemIcon.sprite = icon;
            }
            if (quantityText != null)
                quantityText.text = qty > 1 ? qty.ToString() : "";
            if (slotBackground != null)
                slotBackground.color = filledColor;
        }
    }

    private void OnClicked()
    {
        inventoryUI?.OnSlotClicked(itemType);
    }

    // Legacy support — giữ để không break code cũ
    public void Initialize(int index, UIInventory parent) { }
    public void SetSlotData(int bag, int slot) { }
    public void SetSelected(bool selected) { }
}