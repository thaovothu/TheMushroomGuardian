using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private Button removeButton;
    [SerializeField] private Button closeButton;

    private ItemType currentItemType = ItemType.None;

    private void Start()
    {
        if (useItemButton != null)
            useItemButton.onClick.AddListener(OnUseClicked);
        if (removeButton != null)
            removeButton.onClick.AddListener(OnRemoveClicked);
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        gameObject.SetActive(false);
    }

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

        bool isElement = type == ItemType.EarthCrystal || type == ItemType.WindCrystal
                      || type == ItemType.WaterCrystal || type == ItemType.FireCrystal;

        // Elements have no actions; weapons and potions both get Dùng + Cởi.
        if (useItemButton != null)
            useItemButton.gameObject.SetActive(!isElement);
        if (removeButton != null)
            removeButton.gameObject.SetActive(!isElement);

        gameObject.SetActive(true);
    }

    public void ClearDetail()
    {
        Debug.Log("xoaduoc");
        currentItemType = ItemType.None;
        gameObject.SetActive(false);
    }

    public void Hide() => ClearDetail();

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

    // "Dùng" — equip weapon or consume potion/buff
    private void OnUseClicked()
    {
        Debug.Log("Vaodayne");
        if (currentItemType == ItemType.None) return;

        if (currentItemType == ItemType.Sword)
            EquipmentSystem.Instance?.EquipFromInventory(WeaponType.Sword);
        else if (currentItemType == ItemType.Bow)
            EquipmentSystem.Instance?.EquipFromInventory(WeaponType.Bow);
        else
            InventorySystem.Instance?.UseItem(currentItemType);

        Hide();
    }

    // "Cởi" — unequip weapon (set to None) or delete potion/buff
    private void OnRemoveClicked()
    {
        if (currentItemType == ItemType.None) return;

        if (currentItemType == ItemType.Sword || currentItemType == ItemType.Bow)
            EquipmentSystem.Instance?.EquipFromInventory(WeaponType.None);
        else
            InventorySystem.Instance?.RemoveItem(currentItemType);

        Hide();
    }
}