using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hiển thị chi tiết item được chọn.
/// SetActive(true) khi click slot, SetActive(false) khi đóng/clear.
/// </summary>
public class ItemDetailUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button useItemButton;
    [SerializeField] private Button deleteItemButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button closeButton;

    private ItemType currentItemType = ItemType.None;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (useItemButton != null)
            useItemButton.onClick.AddListener(OnUseItemClicked);
        if (deleteItemButton != null)
            deleteItemButton.onClick.AddListener(OnDeleteItemClicked);
        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipClicked);
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        gameObject.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void ShowItemDetailByType(ItemType type, int quantity)
    {
        if (InventorySystem.Instance == null) return;

        currentItemType = type;

        var icon = InventorySystem.Instance.GetItemIconByType(type);
        var data = InventorySystem.Instance.GetItemDataByType(type);

        if (itemImage != null) itemImage.sprite = icon;
        if (itemTypeText != null) itemTypeText.text = type.ToString();
        if (quantityText != null) quantityText.text = $"x{quantity}";

        if (data != null)
        {
            if (itemNameText != null) itemNameText.text = data.itemName;
            if (descriptionText != null) descriptionText.text = data.description;
            if (statsText != null) statsText.text = BuildStatsText(data);
        }

        bool isWeapon = type == ItemType.Sword || type == ItemType.Bow;
        bool isElement = type == ItemType.EarthCrystal || type == ItemType.WindCrystal
                      || type == ItemType.WaterCrystal || type == ItemType.FireCrystal;

        // Weapons: show Use (acts as Equip) but hide Delete.
        // Elements: hide both Use and Delete.
        // Potions/Buffs: show both Use and Delete.
        if (useItemButton != null)
            useItemButton.gameObject.SetActive(!isElement);

        if (deleteItemButton != null)
            deleteItemButton.gameObject.SetActive(!isElement && !isWeapon);

        // equipButton — optional dedicated equip button (set in Inspector)
        if (equipButton != null)
            equipButton.gameObject.SetActive(isWeapon);

        gameObject.SetActive(true);
    }

    public void ClearDetail()
    {
        currentItemType = ItemType.None;
        gameObject.SetActive(false);
    }

    public void Hide() => ClearDetail();

    // ── Internal ──────────────────────────────────────────────────────────────

    private string BuildStatsText(ItemData data)
    {
        return data.itemType switch
        {
            ItemType.HealthPotion =>
                data.isPercentage ? $"Hồi {data.healAmount}% HP" : $"Hồi {data.healAmount} HP",
            ItemType.ManaPotion =>
                data.isPercentage ? $"Hồi {data.healAmount}% Mana" : $"Hồi {data.healAmount} Mana",
            ItemType.StrengthBuff =>
                $"Tăng tấn công {data.buffValue}%\nThời gian: {data.buffDuration}s",
            ItemType.DefenseBuff =>
                $"Tăng phòng thủ {data.buffValue}%\nThời gian: {data.buffDuration}s",
            ItemType.Sword or ItemType.Bow =>
                $"Sát thương +{data.damageBonus}%",
            ItemType.EarthCrystal => "Nguyên tố: Đất",
            ItemType.WindCrystal => "Nguyên tố: Khí",
            ItemType.WaterCrystal => "Nguyên tố: Nước",
            ItemType.FireCrystal => "Nguyên tố: Lửa",
            _ => ""
        };
    }

    private bool IsUsable(ItemType type) =>
        type == ItemType.HealthPotion ||
        type == ItemType.ManaPotion ||
        type == ItemType.StrengthBuff ||
        type == ItemType.DefenseBuff;

    private void OnUseItemClicked()
    {
        if (currentItemType == ItemType.None) return;
        InventorySystem.Instance?.UseItem(currentItemType);
        Hide();
    }

    private void OnDeleteItemClicked()
    {
        if (currentItemType == ItemType.None) return;
        InventorySystem.Instance?.RemoveItem(currentItemType);
        Hide();
    }

    private void OnEquipClicked()
    {
        if (currentItemType == ItemType.Sword)
            EquipmentSystem.Instance?.EquipFromInventory(WeaponType.Sword);
        else if (currentItemType == ItemType.Bow)
            EquipmentSystem.Instance?.EquipFromInventory(WeaponType.Bow);
        Hide();
    }
}