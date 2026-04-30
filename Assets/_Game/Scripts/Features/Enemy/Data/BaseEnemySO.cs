// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// [CreateAssetMenu(fileName = "BaseEnemy", menuName = "EntityData/BaseEnemy")]
// public class BaseEnemySO : ScriptableObject
// {
//     public List<EnemyVariant> enemyVariants;
//     public List<BaseEnemyData> enemyData;

//     private Dictionary<string, EnemyVariant> enemyVariantLookup;
//     private Dictionary<int, BaseEnemyData> enemyDataLookup;

//     private void Init()
//     {
//         if (enemyDataLookup != null) return;

//         enemyVariantLookup = new Dictionary<string, EnemyVariant>();
//         foreach (var enemy in enemyVariants)
//         {
//             if (enemy == null)
//             {
//                 Debug.LogWarning("BaseEnemySO: null EnemyVariant found, skipping.");
//                 continue;
//             }
//             if (string.IsNullOrEmpty(enemy.enemyName))
//             {
//                 Debug.LogWarning($"BaseEnemySO {name}: EnemyVariant with empty name, skipping prefab {enemy.enemyPrefab?.name}");
//                 continue;
//             }
//             if (enemyVariantLookup.ContainsKey(enemy.enemyName))
//             {
//                 Debug.LogWarning($"BaseEnemySO {name}: Duplicate EnemyVariant name '{enemy.enemyName}', skipping duplicate.");
//                 continue;
//             }
//             enemyVariantLookup.Add(enemy.enemyName, enemy);
//         }
//         enemyDataLookup = new Dictionary<int, BaseEnemyData>();
//         foreach (var enemy in enemyData)
//         {
//             if (enemy == null)
//             {
//                 Debug.LogWarning("BaseEnemySO: null BaseEnemyData found, skipping.");
//                 continue;
//             }
//             if (enemyDataLookup.ContainsKey(enemy.levelEnemy))
//             {
//                 Debug.LogWarning($"BaseEnemySO {name}: Duplicate BaseEnemyData level '{enemy.levelEnemy}', skipping duplicate.");
//                 continue;
//             }
//             enemyDataLookup.Add(enemy.levelEnemy, enemy);
//         }
//     }

//     public EnemyVariant GetEnemyVariant(string enemyName)
//     {
//         Init();
//         enemyVariantLookup.TryGetValue(enemyName, out EnemyVariant variant);
//         return variant;
//     }
//     public EnemyVariant GetRandomEnemyVariant()
//     {
//         Init();
//         if (enemyVariants.Count == 0) return null;

//         return enemyVariants[Random.Range(0, enemyVariants.Count)];
//     }

//     public BaseEnemyData GetEnemyData(int level)
//     {
//         Init();
//         if (enemyDataLookup.TryGetValue(level, out BaseEnemyData data))
//         {
//             return data;
//         }

//         Debug.Log("DataEnemy Empty");
//         return default;
//     }

// }

// // [System.Serializable]
// // public class EnemyVariant
// // {
// //     public string enemyName;
// //     public GameObject enemyPrefab;
// // }

// // [System.Serializable]
// // public class BaseEnemyData
// // {
// //     public int levelEnemy;
// //     public float moveSpeed;
// //     public float hp;
// //     public int damage;
// //     public float recover;

// // }

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseEnemy", menuName = "EntityData/BaseEnemy")]
public class BaseEnemySO : ScriptableObject, IEnemyRepository
{
    public List<EnemyVariant> enemyVariants;
    public List<BaseEnemyData> enemyData;

    Dictionary<string, EnemyVariant> _variantLookup;
    Dictionary<int, BaseEnemyData> _dataLookup;

    void Init()
    {
        if (_dataLookup != null) return;

        _variantLookup = new();
        foreach (var e in enemyVariants)
        {
            if (e == null || string.IsNullOrEmpty(e.enemyName)) continue;
            if (!_variantLookup.TryAdd(e.enemyName, e))
                Debug.LogWarning($"[BaseEnemySO] Duplicate variant '{e.enemyName}'");
        }

        _dataLookup = new();
        foreach (var d in enemyData)
        {
            if (d == null) continue;
            if (!_dataLookup.TryAdd(d.levelEnemy, d))
                Debug.LogWarning($"[BaseEnemySO] Duplicate level '{d.levelEnemy}'");
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
        Init();
        _variantLookup.TryGetValue(enemyName, out var variant);
        return variant;
    }

    public EnemyVariant GetRandomEnemyVariant()
    {
        Init();
        if (enemyVariants.Count == 0) return null;
        return enemyVariants[Random.Range(0, enemyVariants.Count)];
    }
}