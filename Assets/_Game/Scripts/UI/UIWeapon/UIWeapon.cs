using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiển thị vũ khí đang mang bằng 1 Image duy nhất.
/// Gắn script này vào GameObject UIWeapon trong UICanvas.
/// Trong Inspector cần set:
///   - weaponImage: Image component hiển thị icon
///   - noWeaponSprite: icon mặc định khi không mang vũ khí (có thể để null)
/// Icon kiếm/cung lấy tự động từ InventorySystem (ItemIconSO).
/// </summary>
public class UIWeapon : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private Sprite noWeaponSprite;

    private void OnEnable()
    {
        GameEvent.Equipment.OnWeaponChanged += RefreshIcon;
        RefreshIcon(EquipmentSystem.Instance?.GetCurrentWeaponType() ?? WeaponType.None);
    }

    private void OnDisable()
    {
        GameEvent.Equipment.OnWeaponChanged -= RefreshIcon;
    }

    private void RefreshIcon(WeaponType type)
    {
        if (weaponImage == null) return;

        Sprite icon = null;
        if (type == WeaponType.Sword)
            icon = InventorySystem.Instance?.GetItemIconByType(ItemType.Sword);
        else if (type == WeaponType.Bow)
            icon = InventorySystem.Instance?.GetItemIconByType(ItemType.Bow);

        if (icon != null)
        {
            weaponImage.sprite = icon;
            weaponImage.enabled = true;
        }
        else if (noWeaponSprite != null)
        {
            weaponImage.sprite = noWeaponSprite;
            weaponImage.enabled = true;
        }
        else
        {
            weaponImage.sprite = null;
            weaponImage.enabled = false;
        }
    }
}
