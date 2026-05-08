using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Giao diện cho 1 slot trong inventory
/// Hiển thị icon, tên, số lượng của item
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;           // Icon của item
    [SerializeField] private TextMeshProUGUI itemNameText;    // Tên item
    [SerializeField] private TextMeshProUGUI quantityText;    // Số lượng
    [SerializeField] private Button slotButton;        // Button của slot
    [SerializeField] private Image slotBackground;     // Background highlight

    private int slotIndex;
    private InventorySlot slotData;
    private InventoryUI inventoryUI;

    private Color emptyColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    private Color filledColor = Color.white;

    private void Awake()
    {
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    /// <summary>
    /// Khởi tạo slot UI
    /// </summary>
    public void Initialize(int index, InventoryUI parentUI)
    {
        slotIndex = index;
        inventoryUI = parentUI;
        RefreshUI();
    }

    /// <summary>
    /// Cập nhật giao diện slot từ dữ liệu
    /// </summary>
    public void RefreshUI()
    {
        if (InventorySystem.Instance == null)
            return;

        slotData = InventorySystem.Instance.GetSlot(slotIndex);

        if (slotData == null || slotData.IsEmpty)
        {
            // Slot rỗng
            if (itemIcon != null)
                itemIcon.sprite = null;
            if (itemNameText != null)
                itemNameText.text = "Empty";
            if (quantityText != null)
                quantityText.text = "";
            if (slotBackground != null)
                slotBackground.color = emptyColor;
        }
        else
        {
            // Slot có item
            ItemData itemData = slotData.ItemData;
            
            if (itemNameText != null)
                itemNameText.text = itemData.itemName;
            
            if (quantityText != null)
                quantityText.text = slotData.Quantity > 1 ? slotData.Quantity.ToString() : "";
            
            if (slotBackground != null)
                slotBackground.color = filledColor;

            // TODO: Thiết lập icon từ Resources
            // if (itemIcon != null && !string.IsNullOrEmpty(itemData.iconPath))
            // {
            //     itemIcon.sprite = Resources.Load<Sprite>(itemData.iconPath);
            // }
        }
    }

    /// <summary>
    /// Khi click vào slot
    /// </summary>
    private void OnSlotClicked()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnSlotClicked(slotIndex);
        }
    }

    /// <summary>
    /// Hiển thị chi tiết item
    /// </summary>
    public void ShowDetails()
    {
        if (slotData == null || slotData.IsEmpty)
        {
            Debug.Log("[InventorySlotUI] Slot is empty");
            return;
        }

        ItemData itemData = slotData.ItemData;
        Debug.Log($"[InventorySlotUI] Item: {itemData.itemName}, Type: {itemData.itemType}, Description: {itemData.description}");
    }

    /// <summary>
    /// Sử dụng item
    /// </summary>
    public void UseItem()
    {
        if (slotData == null || slotData.IsEmpty)
        {
            Debug.Log("[InventorySlotUI] Cannot use empty slot");
            return;
        }

        InventorySystem.Instance.UseItem(slotIndex);
        RefreshUI();
    }

    /// <summary>
    /// Xoá item
    /// </summary>
    public void DeleteItem()
    {
        if (slotData == null || slotData.IsEmpty)
        {
            Debug.Log("[InventorySlotUI] Cannot delete empty slot");
            return;
        }

        InventorySystem.Instance.DeleteItem(slotIndex);
        RefreshUI();
    }

    /// <summary>
    /// Đặt slot được chọn
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (slotBackground != null)
        {
            if (isSelected)
            {
                slotBackground.color = new Color(1, 1, 0, 1); // Vàng để highlight
            }
            else
            {
                slotBackground.color = slotData != null && !slotData.IsEmpty ? filledColor : emptyColor;
            }
        }
    }

    public int GetSlotIndex() => slotIndex;
    public InventorySlot GetSlotData() => slotData;
}
