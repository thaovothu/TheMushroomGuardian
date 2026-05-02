using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý item drop logic
/// </summary>
public class ItemDropManager : MonoBehaviour
{
    [SerializeField] private ItemDropConfig dropConfig;
    [SerializeField] private ItemSO itemSO;
    
    private static ItemDropManager _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawn item drop khi enemy chết (logic cơ bản: Coin hoặc EXPGem)
    /// </summary>
    public void DropItemsOnEnemyDeath(Vector3 dropPosition, bool isBoss = false)
    {
        if (dropConfig == null || itemSO == null) return;

        // Lấy main drops (Coin, EXPGem)
        var drops = dropConfig.GetMainDrops();

        foreach (var drop in drops)
        {
            // Check tỷ lệ drop
            if (dropConfig.CheckDropChance(drop))
            {
                SpawnItem(drop, dropPosition);
            }
        }
    }

    /// <summary>
    /// Spawn item drop khi giết boss (bao gồm item hiếm)
    /// </summary>
    public void DropItemsOnBossDeath(Vector3 dropPosition)
    {
        if (dropConfig == null || itemSO == null) return;

        // Drop main items
        DropItemsOnEnemyDeath(dropPosition, false);

        // Thêm boss-only drops
        List<ItemDropChance> bossDrops = dropConfig.GetBossDrops();
        foreach (var drop in bossDrops)
        {
            if (dropConfig.CheckDropChance(drop))
            {
                SpawnItem(drop, dropPosition);
            }
        }
    }

    /// <summary>
    /// Spawn một item
    /// </summary>
    private void SpawnItem(ItemDropChance drop, Vector3 spawnPos)
    {
        var pickupPrefab = dropConfig.GetPickupPrefab(drop.itemType);
        if (pickupPrefab == null)
        {
            Debug.LogWarning($"[ItemDropManager] Chưa set prefab cho item type {drop.itemType}!");
            return;
        }

        var itemObj = Instantiate(pickupPrefab, spawnPos, Quaternion.identity);
        
        if (itemObj.TryGetComponent<ItemPickup>(out var pickup))
        {
            pickup.Initialize(drop.itemId, drop.dropAmount);
            
            // Thêm force để item bay ra
            if (itemObj.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 randomDir = Random.insideUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y); // Hướng lên trên
                rb.velocity = randomDir.normalized * dropConfig.dropForce;
            }
        }
        else
        {
            Debug.LogWarning("[ItemDropManager] Item prefab không có ItemPickup component!");
        }
    }

    public static ItemDropManager Instance => _instance;
}
