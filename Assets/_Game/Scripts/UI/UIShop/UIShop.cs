using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI cửa hàng NPC — hiện danh sách item có thể mua.
/// Dùng ItemIconSO để lấy icon và ItemSO để lấy tên item theo itemId.
/// </summary>
public class UIShop : MonoBehaviour
{
    [System.Serializable]
    public struct ShopItemEntry
    {
        public int itemId;
        public int price;
        public int stock; // -1 = unlimited
    }

    [System.Serializable]
    public struct ShopConfig
    {
        public int npcId;
        public List<ShopItemEntry> items;
    }

    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI shopTitleText;

    [Header("Data")]
    [SerializeField] private ItemIconSO itemIconSO;
    [SerializeField] private ItemSO itemSO;

    [Header("Shop Configs — mỗi NPC 1 config")]
    [SerializeField] private List<ShopConfig> shopConfigs = new List<ShopConfig>();

    private int currentNpcId = -1;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvent.NPC.OnInteract += OnNPCInteract;
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }

    private void OnDisable()
    {
        GameEvent.NPC.OnInteract -= OnNPCInteract;
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseShop);
    }

    private void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    // ── Event Handler ─────────────────────────────────────────────────────────

    private void OnNPCInteract(int npcId, InteractableNPC.InteractionType type)
    {
        if (type != InteractableNPC.InteractionType.Shop) return;
        OpenShop(npcId);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void OpenShop(int npcId)
    {
        Debug.Log($"[UIShop] Attempting to open shop for NPC {npcId}...");
        currentNpcId = npcId;

        var config = GetShopConfig(npcId);
        if (config == null)
        {
            Debug.LogWarning($"[UIShop] Không tìm thấy ShopConfig cho NpcID={npcId}!");
            return;
        }

        if (shopTitleText != null)
            shopTitleText.text = "Cửa Hàng";

        PopulateShop(config.Value);

        if (shopPanel != null)
            shopPanel.SetActive(true);

        Debug.Log($"[UIShop] Opened shop for NPC {npcId}");
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        Debug.Log("[UIShop] Shop closed");
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void PopulateShop(ShopConfig config)
    {
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (var item in config.items)
        {
            if (shopItemPrefab == null) continue;

            var slot = Instantiate(shopItemPrefab, itemContainer);
            var slotUI = slot.GetComponent<UIShopItemSlot>();
            if (slotUI != null)
                slotUI.Setup(item.itemId, item.price, item.stock, OnBuyItem, itemIconSO, itemSO);
        }
    }

    private void OnBuyItem(int itemId, int price)
    {
        if (UIMoney.TotalCoins < price)
        {
            Debug.Log("[UIShop] Không đủ tiền!");
            return;
        }

        UIMoney.AddCoin(-price);
        InventorySystem.Instance?.AddItem(itemId, 1);
        GameEvent.Item.OnItemPickedUp?.Invoke(itemId, 1);

        Debug.Log($"[UIShop] Bought itemId={itemId} for {price} coins");
    }

    private ShopConfig? GetShopConfig(int npcId)
    {
        foreach (var config in shopConfigs)
        {
            if (config.npcId == npcId)
                return config;
        }
        return null;
    }
    public void Show()
    {
        gameObject.SetActive(true);
        OpenShop(currentNpcId);    
    }
}