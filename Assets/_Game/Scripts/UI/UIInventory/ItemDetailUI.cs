using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hiển thị chi tiết của item được chọn
/// Tên, hình ảnh, thuộc tính, số lượng
/// </summary>
public class ItemDetailUI : MonoBehaviour
{
    [SerializeField] private Image itemImage;           // Hình ảnh item
    [SerializeField] private TextMeshProUGUI itemNameText;    // Tên item
    [SerializeField] private TextMeshProUGUI itemTypeText;    // Loại item
    [SerializeField] private TextMeshProUGUI quantityText;    // Số lượng
    [SerializeField] private TextMeshProUGUI descriptionText; // Mô tả
    
    [SerializeField] private Button useItemButton;     // Nút sử dụng
    [SerializeField] private Button deleteItemButton;  // Nút xoá

    private int currentBagIndex = -1;
    private int currentSlotIndex = -1;
    private InventorySlot currentSlotData;

    private void Start()
    {
        // Setup buttons
        if (useItemButton != null)
            useItemButton.onClick.AddListener(OnUseItemClicked);
        if (deleteItemButton != null)
            deleteItemButton.onClick.AddListener(OnDeleteItemClicked);

        // Clear detail on start
        ClearDetail();
    }

    /// <summary>
    /// Hiển thị chi tiết của item
    /// Gọi từ InventorySlotUI khi click vào slot
    /// </summary>
    public void ShowItemDetail(int bagIndex, int slotIndex, InventorySlot slotData)
    {
        if (slotData == null || slotData.IsEmpty)
        {
            ClearDetail();
            return;
        }

        currentBagIndex = bagIndex;
        currentSlotIndex = slotIndex;
        currentSlotData = slotData;

        ItemData itemData = slotData.ItemData;

        // Cập nhật các text fields
        if (itemNameText != null)
            itemNameText.text = itemData.itemName;

        if (itemTypeText != null)
            itemTypeText.text = $"Type: {itemData.itemType}";

        if (quantityText != null)
            quantityText.text = $"Quantity: {slotData.Quantity}";

        if (descriptionText != null)
            descriptionText.text = itemData.description;

        // TODO: Load image từ Resources
        // if (itemImage != null && !string.IsNullOrEmpty(itemData.iconPath))
        // {
        //     itemImage.sprite = Resources.Load<Sprite>(itemData.iconPath);
        // }

        // Enable buttons
        if (useItemButton != null)
            useItemButton.interactable = true;
        if (deleteItemButton != null)
            deleteItemButton.interactable = true;

        Debug.Log($"[ItemDetailUI] Showing detail for item: {itemData.itemName}");
    }

    /// <summary>
    /// Xoá chi tiết hiển thị
    /// </summary>
    public void ClearDetail()
    {
        currentBagIndex = -1;
        currentSlotIndex = -1;
        currentSlotData = null;

        if (itemNameText != null)
            itemNameText.text = "No Item Selected";

        if (itemTypeText != null)
            itemTypeText.text = "";

        if (quantityText != null)
            quantityText.text = "";

        if (descriptionText != null)
            descriptionText.text = "Select an item to view details";

        if (itemImage != null)
            itemImage.sprite = null;

        if (useItemButton != null)
            useItemButton.interactable = false;
        if (deleteItemButton != null)
            deleteItemButton.interactable = false;
    }

    /// <summary>
    /// Nút sử dụng item
    /// </summary>
    private void OnUseItemClicked()
    {
        if (currentBagIndex >= 0 && currentSlotIndex >= 0)
        {
            InventorySystem.Instance.UseItem(currentBagIndex, currentSlotIndex);
            ClearDetail();
        }
    }

    /// <summary>
    /// Nút xoá item
    /// </summary>
    private void OnDeleteItemClicked()
    {
        if (currentBagIndex >= 0 && currentSlotIndex >= 0)
        {
            InventorySystem.Instance.DeleteItem(currentBagIndex, currentSlotIndex);
            ClearDetail();
        }
    }

    /// <summary>
    /// Ẩn detail panel
    /// </summary>
    public void Hide()
    {
        ClearDetail();
    }
}
