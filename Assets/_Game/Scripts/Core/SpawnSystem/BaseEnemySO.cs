using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BaseEnemy", menuName = "EntityData/BaseEnemy")]
public class BaseEnemySO : ScriptableObject
{
    public List<EnemyVariant> enemyVariants;
    public List<BaseEnemyData> enemyData;

    private Dictionary<string, EnemyVariant> enemyVariantLookup;
    private Dictionary<int, BaseEnemyData> enemyDataLookup;

    private void Init()
    {
        if (enemyDataLookup != null) return;

        enemyVariantLookup = new Dictionary<string, EnemyVariant>();
        foreach (var enemy in enemyVariants)
        {
            if (enemy == null)
            {
                Debug.LogWarning("BaseEnemySO: null EnemyVariant found, skipping.");
                continue;
            }
            if (string.IsNullOrEmpty(enemy.enemyName))
            {
                Debug.LogWarning($"BaseEnemySO {name}: EnemyVariant with empty name, skipping prefab {enemy.enemyPrefab?.name}");
                continue;
            }
            if (enemyVariantLookup.ContainsKey(enemy.enemyName))
            {
                Debug.LogWarning($"BaseEnemySO {name}: Duplicate EnemyVariant name '{enemy.enemyName}', skipping duplicate.");
                continue;
            }
            enemyVariantLookup.Add(enemy.enemyName, enemy);
        }
        enemyDataLookup = new Dictionary<int, BaseEnemyData>();
        foreach (var enemy in enemyData)
        {
            if (enemy == null)
            {
                Debug.LogWarning("BaseEnemySO: null BaseEnemyData found, skipping.");
                continue;
            }
            if (enemyDataLookup.ContainsKey(enemy.levelEnemy))
            {
                Debug.LogWarning($"BaseEnemySO {name}: Duplicate BaseEnemyData level '{enemy.levelEnemy}', skipping duplicate.");
                continue;
            }
            enemyDataLookup.Add(enemy.levelEnemy, enemy);
        }
    }

    public EnemyVariant GetEnemyVariant(string enemyName)
    {
        Init();
        enemyVariantLookup.TryGetValue(enemyName, out EnemyVariant variant);
        return variant;
    }
    public EnemyVariant GetRandomEnemyVariant()
    {
        Init();
        if (enemyVariants.Count == 0) return null;

        return enemyVariants[Random.Range(0, enemyVariants.Count)];
    }

    public BaseEnemyData GetEnemyData(int level)
    {
        Init();
        if (enemyDataLookup.TryGetValue(level, out BaseEnemyData data))
        {
            return data;
        }

        Debug.Log("DataEnemy Empty");
        return default;
    }

}

[System.Serializable]
public class EnemyVariant
{
    public string enemyName;
    public GameObject enemyPrefab;
}

[System.Serializable]
public class BaseEnemyData
{
    public int levelEnemy;
    public float moveSpeed;
    public float hp;
    public int damage;
    public float recover;

}