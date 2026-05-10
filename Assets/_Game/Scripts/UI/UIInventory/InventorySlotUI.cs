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
    // [SerializeField] private TextMeshProUGUI itemNameText;    // Tên item
    [SerializeField] private TextMeshProUGUI quantityText;    // Số lượng
    [SerializeField] private Button slotButton;        // Button của slot
    [SerializeField] private Image slotBackground;     // Background highlight
    [SerializeField] private ItemDetailUI itemDetailUI; // Reference tới detail panel

    private int displaySlotIndex;      // Vị trí hiển thị (0-8)
    private int bagIndex = -1;         // Chỉ số túi
    private int actualSlotIndex = -1;  // Chỉ số slot thực tế trong túi
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
    /// Khởi tạo slot UI (gọi lần đầu từ InventoryUI)
    /// </summary>
    public void Initialize(int index, InventoryUI parentUI)
    {
        displaySlotIndex = index;
        inventoryUI = parentUI;
    }

    /// <summary>
    /// Thiết lập dữ liệu slot (túi và vị trí thực tế)
    /// </summary>
    public void SetSlotData(int newBagIndex, int newSlotIndex)
    {
        bagIndex = newBagIndex;
        actualSlotIndex = newSlotIndex;
        RefreshUI();
    }

    /// <summary>
    /// Cập nhật giao diện slot từ dữ liệu
    /// </summary>
    public void RefreshUI()
    {
        if (InventorySystem.Instance == null || bagIndex < 0 || actualSlotIndex < 0)
            return;

        slotData = InventorySystem.Instance.GetSlot(bagIndex, actualSlotIndex);

        if (slotData == null || slotData.IsEmpty)
        {
            // Slot rỗng
            if (itemIcon != null)
                itemIcon.sprite = null;
            // if (itemNameText != null)
            //     itemNameText.text = "Empty";
            if (quantityText != null)
                quantityText.text = "";
            if (slotBackground != null)
                slotBackground.color = emptyColor;
        }
        else
        {
            // Slot có item
            ItemData itemData = slotData.ItemData;
            
            // if (itemNameText != null)
            //     itemNameText.text = itemData.itemName;
            
            if (quantityText != null)
                quantityText.text = slotData.Quantity > 1 ? slotData.Quantity.ToString() : "";
            
            if (slotBackground != null)
                slotBackground.color = filledColor;

            // Thiết lập icon từ ItemIconSO
            if (itemIcon != null)
            {
                Sprite icon = InventorySystem.Instance.GetItemIcon(itemData.itemId);
                itemIcon.sprite = icon;
            }
        }
    }

    /// <summary>
    /// Khi click vào slot - truyền data tới ItemDetailUI
    /// </summary>
    private void OnSlotClicked()
    {
        if (slotData != null && !slotData.IsEmpty && itemDetailUI != null)
        {
            // Truyền dữ liệu sang ItemDetailUI
            itemDetailUI.ShowItemDetail(bagIndex, actualSlotIndex, slotData);
        }
        
        // Callback cho InventoryUI nếu cần
        if (inventoryUI != null)
        {
            inventoryUI.OnSlotClicked(displaySlotIndex);
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
        if (slotData == null || slotData.IsEmpty || bagIndex < 0 || actualSlotIndex < 0)
        {
            Debug.Log("[InventorySlotUI] Cannot use empty slot");
            return;
        }

        InventorySystem.Instance.UseItem(bagIndex, actualSlotIndex);
        RefreshUI();
    }

    /// <summary>
    /// Xoá item
    /// </summary>
    public void DeleteItem()
    {
        if (slotData == null || slotData.IsEmpty || bagIndex < 0 || actualSlotIndex < 0)
        {
            Debug.Log("[InventorySlotUI] Cannot delete empty slot");
            return;
        }

        InventorySystem.Instance.DeleteItem(bagIndex, actualSlotIndex);
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

    public int GetDisplaySlotIndex() => displaySlotIndex;
    public int GetBagIndex() => bagIndex;
    public int GetActualSlotIndex() => actualSlotIndex;
    public InventorySlot GetSlotData() => slotData;
}

