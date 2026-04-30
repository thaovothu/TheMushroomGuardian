using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerLevelData
{
    [SerializeField] public int level;
    [SerializeField] public int hp;
    [SerializeField] public int damage;
    [SerializeField] public int exp;

    public PlayerLevelData(int level, int hp, int damage, int exp)
    {
        this.level = level;
        this.hp = hp;
        this.damage = damage;
        this.exp = exp;
    }
}

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "EntityData/Player")]
public class PlayerSO : ScriptableObject
{
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private int baseMana = 150;
    [SerializeField] private List<PlayerLevelData> levelData = new();

    private Dictionary<int, PlayerLevelData> levelLookup;
    private bool isInitialized = false;

    private void Init()
    {
        if (isInitialized) return;

        levelLookup = new Dictionary<int, PlayerLevelData>();

        if (levelData == null || levelData.Count == 0)
        {
            Debug.LogWarning($"PlayerSO '{name}': No level data configured!");
            return;
        }

        foreach (var data in levelData)
        {
            if (data == null)
            {
                Debug.LogWarning($"PlayerSO '{name}': Null level data found, skipping.");
                continue;
            }

            if (levelLookup.ContainsKey(data.level))
            {
                Debug.LogWarning($"PlayerSO '{name}': Duplicate level '{data.level}', skipping.");
                continue;
            }

            levelLookup.Add(data.level, data);
        }

        isInitialized = true;
    }

    public PlayerLevelData GetLevelData(int level)
    {
        Init();

        if (levelLookup.TryGetValue(level, out PlayerLevelData data))
        {
            return data;
        }

        Debug.LogWarning($"PlayerSO '{name}': Level {level} not found!");
        return null;
    }

    public int GetHP(int level)
    {
        var data = GetLevelData(level);
        return data?.hp ?? 0;
    }

    public int GetDamage(int level)
    {
        var data = GetLevelData(level);
        return data?.damage ?? 0;
    }

    public int GetEXPRequirement(int level)
    {
        var data = GetLevelData(level);
        return data?.exp ?? 0;
    }

    public int GetMaxLevel() => maxLevel;
    public int GetBaseMana() => baseMana;

    public bool IsValidLevel(int level) => level >= 1 && level <= maxLevel;
}