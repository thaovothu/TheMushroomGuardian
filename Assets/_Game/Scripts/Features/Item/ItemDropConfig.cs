using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cấu hình drop item cho enemy
/// </summary>
[System.Serializable]
public class ItemDropChance
{
    public ItemType itemType;
    [Range(0f, 100f)] public float dropChance = 100f;  // Tỷ lệ drop
    public int itemId;                                   // Item ID cụ thể
    [Tooltip("Giá trị drop (số lượng coin hoặc exp)")]
    public int dropAmount = 1;
}

[System.Serializable]
public class ItemPickupPrefabEntry
{
    public ItemType itemType;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "ItemDropConfig", menuName = "EntityData/ItemDropConfig")]
public class ItemDropConfig : ScriptableObject
{
    [Header("Always drop")]
    [SerializeField] private ItemDropChance coinDrop = new() 
    { 
        itemType = ItemType.Coin, 
        dropChance = 80f,
        itemId = 8,
        dropAmount = 10
    };
    
    [SerializeField] private ItemDropChance expGemDrop = new() 
    { 
        itemType = ItemType.EXPGem, 
        dropChance = 50f,
        itemId = 7,
        dropAmount = 1
    };

    [Header("Boss only")]
    [SerializeField] private ItemDropChance earthCrystalDrop = new() 
    { 
        itemType = ItemType.EarthCrystal, 
        dropChance = 5f,
        itemId = 9,
        dropAmount = 1
    };

    [SerializeField] private ItemDropChance windCrystalDrop = new() 
    { 
        itemType = ItemType.WindCrystal, 
        dropChance = 5f,
        itemId = 10,
        dropAmount = 1
    };

    [SerializeField] private ItemDropChance waterCrystalDrop = new() 
    { 
        itemType = ItemType.WaterCrystal, 
        dropChance = 5f,
        itemId = 11,
        dropAmount = 1
    };

    [SerializeField] private ItemDropChance fireCrystalDrop = new() 
    { 
        itemType = ItemType.FireCrystal, 
        dropChance = 5f,
        itemId = 12,
        dropAmount = 1
    };

    [Header("Buff")]
    [SerializeField] private ItemDropChance strengthBuffDrop = new() 
    { 
        itemType = ItemType.StrengthBuff, 
        dropChance = 3f,
        itemId = 3,
        dropAmount = 1
    };

    [SerializeField] private ItemDropChance defenseBuffDrop = new() 
    { 
        itemType = ItemType.DefenseBuff, 
        dropChance = 3f,
        itemId = 4,
        dropAmount = 1
    };

    [Header("Spawning")]
    [SerializeField] private List<ItemPickupPrefabEntry> itemPickupPrefabs = new();
    [SerializeField] private GameObject fallbackItemPickupPrefab;
    [SerializeField] public float dropForce = 5f;

    public GameObject GetPickupPrefab(ItemType itemType)
    {
        foreach (var entry in itemPickupPrefabs)
        {
            if (entry != null && entry.itemType == itemType && entry.prefab != null)
            {
                return entry.prefab;
            }
        }

        return fallbackItemPickupPrefab;
    }

    /// <summary>
    /// Lấy main drop items (Coin, EXPGem)
    /// </summary>
    public List<ItemDropChance> GetMainDrops()
    {
        return new List<ItemDropChance> 
        { 
            coinDrop, 
            expGemDrop 
        };
    }

    /// <summary>
    /// Lấy tất cả drop items
    /// </summary>
    public List<ItemDropChance> GetAllDrops()
    {
        return new List<ItemDropChance> 
        { 
            coinDrop, 
            expGemDrop, 
            earthCrystalDrop,
            windCrystalDrop,
            waterCrystalDrop,
            fireCrystalDrop,
            strengthBuffDrop,
            defenseBuffDrop
        };
    }

    /// <summary>
    /// Lấy boss-only drops
    /// </summary>
    public List<ItemDropChance> GetBossDrops()
    {
        return new List<ItemDropChance> 
        { 
            earthCrystalDrop,
            windCrystalDrop,
            waterCrystalDrop,
            fireCrystalDrop,
            strengthBuffDrop,
            defenseBuffDrop
        };
    }

    /// <summary>
    /// Check drop theo tỷ lệ
    /// </summary>
    public bool CheckDropChance(ItemDropChance drop)
    {
        return Random.value * 100f <= drop.dropChance;
    }
}
