using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entry cho 1 item icon
/// </summary>
[System.Serializable]
public class ItemIconEntry
{
    public int itemId;
    public Sprite icon;
}

/// <summary>
/// Lưu trữ tất cả icon 2D của item
/// Sử dụng itemId để lấy icon
/// </summary>
[CreateAssetMenu(fileName = "ItemIconSO", menuName = "Item/ItemIconSO")]
public class ItemIconSO : ScriptableObject
{
    [SerializeField] private List<ItemIconEntry> itemIcons = new();
    [SerializeField] private Sprite defaultIcon;  // Icon mặc định khi không tìm thấy

    /// <summary>
    /// Lấy icon theo itemId
    /// </summary>
    public Sprite GetIcon(int itemId)
    {
        foreach (var entry in itemIcons)
        {
            if (entry.itemId == itemId && entry.icon != null)
            {
                return entry.icon;
            }
        }

        Debug.LogWarning($"[ItemIconSO] Icon không tìm thấy cho item ID: {itemId}");
        return defaultIcon;
    }

    /// <summary>
    /// Kiểm tra icon có tồn tại không
    /// </summary>
    public bool HasIcon(int itemId)
    {
        foreach (var entry in itemIcons)
        {
            if (entry.itemId == itemId && entry.icon != null)
            {
                return true;
            }
        }
        return false;
    }
}
