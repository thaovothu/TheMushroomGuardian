using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseEnemy", menuName = "EntityData/BaseEnemy")]
public class BaseEnemySO : ScriptableObject, IEnemyRepository
{
    public EnemyVariant enemyVariants;
    public List<BaseEnemyData> enemyData;

    Dictionary<int, BaseEnemyData> _dataLookup;

    void Init()
    {
        if (_dataLookup != null) return;

        _dataLookup = new();
        foreach (var d in enemyData)
        {
            if (d == null) continue;
            if (!_dataLookup.TryAdd(d.levelEnemy, d));
        }
    }

    public BaseEnemyData GetEnemyData(int level)
    {
        Init();
        _dataLookup.TryGetValue(level, out var data);
        return data;
    }

    public EnemyVariant GetEnemyVariant(string enemyName)
    {
        return enemyVariants;
    }

    public EnemyVariant GetRandomEnemyVariant()
    {
        return enemyVariants;
    }
}