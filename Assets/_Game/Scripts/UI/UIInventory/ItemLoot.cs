using UnityEngine;

/// <summary>
/// Xử lý việc nhặt item từ quái vật hoặc vật thể
/// Có thể được gắn vào prefab quái vật
/// </summary>
public class ItemLoot : MonoBehaviour
{
    [SerializeField] private int[] droppedItemIds;        // Danh sách ID item có thể rơi
    [SerializeField] private bool randomItem = false;     // Chỉ rơi 1 item ngẫu nhiên hay tất cả

    /// <summary>
    /// Tạo loot từ quái vật
    /// Được gọi khi quái vật chết
    /// </summary>
    public void DropLoot()
    {
        if (droppedItemIds == null || droppedItemIds.Length == 0)
        {
            Debug.Log("[ItemLoot] No items to drop");
            return;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogError("[ItemLoot] InventorySystem not found!");
            return;
        }

        if (randomItem)
        {
            // Chỉ rơi 1 item ngẫu nhiên
            int randomIndex = Random.Range(0, droppedItemIds.Length);
            AddItemToInventory(droppedItemIds[randomIndex]);
        }
        else
        {
            // Rơi tất cả items
            foreach (int itemId in droppedItemIds)
            {
                AddItemToInventory(itemId);
            }
        }
    }

    /// <summary>
    /// Thêm item vào inventory
    /// </summary>
    private void AddItemToInventory(int itemId)
    {
        if (InventorySystem.Instance.AddItem(itemId, 1))
        {
            Debug.Log($"[ItemLoot] Item {itemId} added to inventory");
        }
        else
        {
            Debug.LogWarning($"[ItemLoot] Failed to add item {itemId} - inventory full");
        }
    }
}
