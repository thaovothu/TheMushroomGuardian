using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Giao diện chính của Inventory
/// Hỗ trợ 3 túi, mỗi túi hiển thị 9 slots, có phân trang
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup slotsGrid;       // Grid chứa slots
    [SerializeField] private InventorySlotUI slotPrefab;      // Prefab của 1 slot
    [SerializeField] private ItemDetailUI itemDetailUI;       // Panel hiển thị chi tiết item
    [SerializeField] private Button closeButton;              // Nút đóng
    
    // Túi - toggle bằng button
    [SerializeField] private Toggle bag1Button;               // Nút túi 1
    [SerializeField] private Toggle bag2Button;               // Nút túi 2
    [SerializeField] private Toggle bag3Button;               // Nút túi 3
    
    // Phân trang
    [SerializeField] private Button prevPageButton;           // Nút trang trước
    [SerializeField] private Button nextPageButton;           // Nút trang sau
    [SerializeField] private TextMeshProUGUI pageText;         // Hiển thị trang hiện tại

    private const int SLOTS_PER_PAGE = 9;
    private List<InventorySlotUI> slotUIs = new();
    private InventorySlotUI selectedSlot;
    private CanvasGroup canvasGroup;
    
    private int currentBagIndex = 0;
    private int currentPage = 0;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

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

    void OnEnable()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);

        // Setup bag toggles
        if (bag1Button != null)
            bag1Button.onValueChanged.AddListener(OnBag1Toggled);
        if (bag2Button != null)
            bag2Button.onValueChanged.AddListener(OnBag2Toggled);
        if (bag3Button != null)
            bag3Button.onValueChanged.AddListener(OnBag3Toggled);

        // Setup pagination buttons
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(PreviousPage);
        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);
    }
    void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);

        // Remove listeners from bag toggles
        if (bag1Button != null)
            bag1Button.onValueChanged.RemoveListener(OnBag1Toggled);
        if (bag2Button != null)
            bag2Button.onValueChanged.RemoveListener(OnBag2Toggled);
        if (bag3Button != null)
            bag3Button.onValueChanged.RemoveListener(OnBag3Toggled);

        // Remove listeners from pagination buttons
        if (prevPageButton != null)
            prevPageButton.onClick.RemoveListener(PreviousPage);
        if (nextPageButton != null)
            nextPageButton.onClick.RemoveListener(NextPage);
    }

    private void OnBag1Toggled(bool value) { if (value) SelectBag(0); }
    private void OnBag2Toggled(bool value) { if (value) SelectBag(1); }
    private void OnBag3Toggled(bool value) { if (value) SelectBag(2); }
    private void Start()
    {
        InitializeSlots();
        UpdateBagButtons();
    }

    /// <summary>
    /// Khởi tạo 9 slots UI
    /// </summary>
    private void InitializeSlots()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("[InventoryUI] Slot prefab not assigned!");
            return;
        }

        // Xoá slots cũ nếu có
        foreach (Transform child in slotsGrid.transform)
        {
            Destroy(child.gameObject);
        }
        slotUIs.Clear();

        // Tạo 9 slots
        for (int i = 0; i < SLOTS_PER_PAGE; i++)
        {
            InventorySlotUI slotUI = Instantiate(slotPrefab, slotsGrid.transform);
            slotUI.Initialize(i, this);
            slotUIs.Add(slotUI);
        }

        Debug.Log("[InventoryUI] Initialized 9 inventory slots");
    }

    /// <summary>
    /// Chọn túi
    /// </summary>
    public void SelectBag(int bagIndex)
    {
        if (bagIndex < 0 || bagIndex >= InventorySystem.Instance.GetBagCount())
        {
            Debug.LogError($"[InventoryUI] Invalid bag index: {bagIndex}");
            return;
        }

        currentBagIndex = bagIndex;
        currentPage = 0;
        selectedSlot = null;
        if (itemDetailUI != null)
            itemDetailUI.ClearDetail();
        UpdateBagButtons();
        RefreshCurrentPage();
        Debug.Log($"[InventoryUI] Selected bag {bagIndex}");
    }

    /// <summary>
    /// Cập nhật trạng thái các toggle túi
    /// </summary>
    private void UpdateBagButtons()
    {
        if (bag1Button != null)
            bag1Button.isOn = currentBagIndex == 0;
        if (bag2Button != null)
            bag2Button.isOn = currentBagIndex == 1;
        if (bag3Button != null)
            bag3Button.isOn = currentBagIndex == 2;
    }

    /// <summary>
    /// Trang trước
    /// </summary>
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            selectedSlot = null;
            if (itemDetailUI != null)
                itemDetailUI.ClearDetail();
            RefreshCurrentPage();
        }
    }

    /// <summary>
    /// Trang sau
    /// </summary>
    public void NextPage()
    {
        InventoryBag currentBag = InventorySystem.Instance.GetBag(currentBagIndex);
        int maxPages = Mathf.CeilToInt((float)currentBag.SlotCount / SLOTS_PER_PAGE);
        
        if (currentPage < maxPages - 1)
        {
            currentPage++;
            selectedSlot = null;
            if (itemDetailUI != null)
                itemDetailUI.ClearDetail();
            RefreshCurrentPage();
        }
    }

    /// <summary>
    /// Cập nhật các slots hiện tại dựa vào trang
    /// </summary>
    private void RefreshCurrentPage()
    {
        InventoryBag currentBag = InventorySystem.Instance.GetBag(currentBagIndex);
        int startIndex = currentPage * SLOTS_PER_PAGE;

        // Cập nhật các slots hiển thị
        for (int i = 0; i < SLOTS_PER_PAGE; i++)
        {
            int actualSlotIndex = startIndex + i;
            if (actualSlotIndex < currentBag.SlotCount)
            {
                slotUIs[i].SetSlotData(currentBagIndex, actualSlotIndex);
                slotUIs[i].RefreshUI();
                slotUIs[i].gameObject.SetActive(true);
            }
            else
            {
                slotUIs[i].gameObject.SetActive(false);
            }
        }

        // Cập nhật nút phân trang
        UpdatePaginationButtons();
        UpdatePageText();
    }

    /// <summary>
    /// Cập nhật trạng thái nút phân trang
    /// </summary>
    private void UpdatePaginationButtons()
    {
        InventoryBag currentBag = InventorySystem.Instance.GetBag(currentBagIndex);
        int maxPages = Mathf.CeilToInt((float)currentBag.SlotCount / SLOTS_PER_PAGE);

        if (prevPageButton != null)
            prevPageButton.interactable = currentPage > 0;
        if (nextPageButton != null)
            nextPageButton.interactable = currentPage < maxPages - 1;
    }

    /// <summary>
    /// Cập nhật hiển thị số trang
    /// </summary>
    private void UpdatePageText()
    {
        InventoryBag currentBag = InventorySystem.Instance.GetBag(currentBagIndex);
        int maxPages = Mathf.CeilToInt((float)currentBag.SlotCount / SLOTS_PER_PAGE);
        
        if (pageText != null)
        {
            pageText.text = $"Page {currentPage + 1}/{maxPages}";
        }
    }

    /// <summary>
    /// Khi click vào 1 slot
    /// </summary>
    public void OnSlotClicked(int displaySlotIndex)
    {
        // Bỏ highlight slot cũ
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(false);
        }

        // Highlight slot mới
        selectedSlot = slotUIs[displaySlotIndex];
        selectedSlot.SetSelected(true);
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
    private void OnInventorySlotChanged(int bagIndex, int slotIndex)
    {
        // Chỉ cập nhật nếu là túi hiện tại đang hiển thị
        if (bagIndex == currentBagIndex)
        {
            RefreshCurrentPage();
        }
    }

    /// <summary>
    /// Hiển thị inventory
    /// </summary>
    public void Show()
    {
        // Show được quản lý thông qua UIManager.ShowUI()
        gameObject.SetActive(true);
        // UIManager.Instance.ShowUI("UIInventory");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Refresh trang hiện tại
        RefreshCurrentPage();
        Debug.Log("[InventoryUI] Inventory shown");
    }

    /// <summary>
    /// Ẩn inventory
    /// </summary>
    public void Hide()
    {
        // UIManager.Instance.HideUI("UIInventory");
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        selectedSlot = null;
        if (itemDetailUI != null)
            itemDetailUI.Hide();
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

