using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BaseEnemy", menuName = "EntityData/BaseEnemy")]
public class BaseEnemySO : EntityData
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
            enemyVariantLookup.Add(enemy.enemyName, enemy);
        }
        enemyDataLookup = new Dictionary<int, BaseEnemyData>();
        foreach (var enemy in enemyData)
        {
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