using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Giao diện chính của Inventory
/// Hiển thị 20 slots và các nút hành động
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup slotsGrid;       // Grid chứa slots
    [SerializeField] private InventorySlotUI slotPrefab;      // Prefab của 1 slot
    [SerializeField] private TextMeshProUGUI itemDetailsText;  // Hiển thị chi tiết item
    [SerializeField] private Button useItemButton;            // Nút sử dụng
    [SerializeField] private Button deleteItemButton;         // Nút xoá
    [SerializeField] private Button closeButton;              // Nút đóng

    private List<InventorySlotUI> slotUIs = new();
    private InventorySlotUI selectedSlot;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Setup buttons
        if (useItemButton != null)
            useItemButton.onClick.AddListener(OnUseItemClicked);
        if (deleteItemButton != null)
            deleteItemButton.onClick.AddListener(OnDeleteItemClicked);
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        // Subscribe to inventory changes
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnSlotChanged += OnInventorySlotChanged;
        }
    }

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnSlotChanged -= OnInventorySlotChanged;
        }
    }

    private void Start()
    {
        InitializeSlots();
        Hide();
    }

    /// <summary>
    /// Khởi tạo tất cả 20 slots
    /// </summary>
    private void InitializeSlots()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("[InventoryUI] Slot prefab not assigned!");
            return;
        }

        const int MAX_SLOTS = 20;

        // Xoá slots cũ nếu có
        foreach (Transform child in slotsGrid.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIs.Clear();

        // Tạo 20 slots
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            InventorySlotUI slotUI = Instantiate(slotPrefab, slotsGrid.transform);
            slotUI.Initialize(i, this);
            slotUIs.Add(slotUI);
        }

        Debug.Log("[InventoryUI] Initialized 20 inventory slots");
    }

    /// <summary>
    /// Khi click vào 1 slot
    /// </summary>
    public void OnSlotClicked(int slotIndex)
    {
        // Bỏ highlight slot cũ
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }

        // Highlight slot mới
        selectedSlot = slotUIs[slotIndex];
        selectedSlot.SetSelected(true);

        // Cập nhật chi tiết
        UpdateItemDetails();
    }

    /// <summary>
    /// Cập nhật chi tiết item
    /// </summary>
    private void UpdateItemDetails()
    {
        if (selectedSlot == null || itemDetailsText == null)
            return;

        InventorySlot slotData = selectedSlot.GetSlotData();

        if (slotData == null || slotData.IsEmpty)
        {
            itemDetailsText.text = "No item selected";
            useItemButton.interactable = false;
            deleteItemButton.interactable = false;
            return;
        }

        ItemData itemData = slotData.ItemData;
        string details = $"<b>{itemData.itemName}</b>\n";
        details += $"Type: {itemData.itemType}\n";
        details += $"Quantity: {slotData.Quantity}\n";
        details += $"\n{itemData.description}";

        itemDetailsText.text = details;
        useItemButton.interactable = true;
        deleteItemButton.interactable = true;
    }

    /// <summary>
    /// Nút sử dụng item
    /// </summary>
    private void OnUseItemClicked()
    {
        if (selectedSlot != null)
        {
            selectedSlot.UseItem();
            UpdateItemDetails();
        }
    }

    /// <summary>
    /// Nút xoá item
    /// </summary>
    private void OnDeleteItemClicked()
    {
        if (selectedSlot != null)
        {
            selectedSlot.DeleteItem();
            UpdateItemDetails();
        }
    }

    /// <summary>
    /// Nút đóng inventory
    /// </summary>
    private void OnCloseClicked()
    {
        Hide();
    }

    /// <summary>
    /// Callback khi inventory thay đổi
    /// </summary>
    private void OnInventorySlotChanged(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotUIs.Count)
        {
            slotUIs[slotIndex].RefreshUI();

            // Nếu slot được chọn thay đổi, update chi tiết
            if (selectedSlot != null && selectedSlot.GetSlotIndex() == slotIndex)
            {
                UpdateItemDetails();
            }
        }
    }

    /// <summary>
    /// Hiển thị inventory
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Refresh tất cả slots
        foreach (var slotUI in slotUIs)
        {
            slotUI.RefreshUI();
        }

        Debug.Log("[InventoryUI] Inventory shown");
    }

    /// <summary>
    /// Ẩn inventory
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        selectedSlot = null;
        Debug.Log("[InventoryUI] Inventory hidden");
    }

    /// <summary>
    /// Toggle hiển thị inventory
    /// </summary>
    public void Toggle()
    {
        if (gameObject.activeSelf)
            Hide();
        else
            Show();
    }

    /// <summary>
    /// Lấy trạng thái inventory
    /// </summary>
    public bool IsOpen => gameObject.activeSelf;
}
