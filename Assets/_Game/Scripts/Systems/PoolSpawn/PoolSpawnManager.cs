// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public class PoolSpawnManager : MonoBehaviour
// {
//     [SerializeField] BaseEnemySO[] enemySO;
//     [SerializeField] MapSO[] mapSO;
//     [SerializeField] Transform[] spawnPoints;
//     [SerializeField] int initialSize = 5;
//     [SerializeField] float spawnInterval = 1f;
//     [SerializeField] public int spawnPerTick = 1;
//     [SerializeField] int mapLevelToLoad = 1; 
//     public static Action<GameObject> OnRelease;

//     public enum StrategyType { Linear, Random }
//     public StrategyType strategy = StrategyType.Linear;

//     Dictionary<GameObject, SimpleObjectPool> poolMap = new Dictionary<GameObject, SimpleObjectPool>();
//     List<Transform> spawnPointList = new List<Transform>();

//     float timer;
//     int linearIndex = 0;
//     List<Transform> randomPool;
//     int plannedSpawnTotal = 0;
//     int actualSpawned = 0;

//     void Awake()
//     {
//         OnRelease -= ReleaseToPool;
//         OnRelease += ReleaseToPool;

//         foreach (BaseEnemySO p in enemySO)
//         {
//             if (p == null || p.enemyVariants[0].enemyPrefab == null) continue;
//             if (!poolMap.ContainsKey(p.enemyVariants[0].enemyPrefab)) poolMap[p.enemyVariants[0].enemyPrefab] = new SimpleObjectPool(p.enemyVariants[0].enemyPrefab, initialSize, this.transform);
//         }

//         if (spawnPoints != null) spawnPointList.AddRange(spawnPoints);
//         timer = spawnInterval;
//         if (strategy == StrategyType.Random) ResetRandomPool();
//     }

//     void Start()
//     {
//         LoadAndSpawnMap(mapLevelToLoad);
//     }

//     // Public: load a map by its levelMap value and spawn configured enemies
//     public void LoadAndSpawnMap(int levelMap)
//     {
//         var mapData = FindMapData(levelMap);
//         if (mapData == null)
//         {
//             //Debug.LogWarning("Map data not found for level " + levelMap);
//             return;
//         }

//         // Calculate planned total and print details for debugging
//         plannedSpawnTotal = 0;
//         actualSpawned = 0;
//         //Debug.Log($"[PoolSpawnManager] Loading map {levelMap} with {mapData.enemyConfigs.Count} enemy config(s)");
//         foreach (var cfg in mapData.enemyConfigs)
//         {
//             if (cfg == null)
//             {
//                 //Debug.LogWarning("[PoolSpawnManager] Found null MapEnemyConfig, skipping.");
//                 continue;
//             }
//             if (cfg.baseEnemySO == null)
//             {
//                 //Debug.LogWarning($"[PoolSpawnManager] MapEnemyConfig has null baseEnemySO, skipping one config (count={cfg.count}).");
//                 continue;
//             }
//             plannedSpawnTotal += cfg.count;
//             string levels = cfg.level == null ? "(no levels)" : string.Join(",", System.Array.ConvertAll(cfg.level, x => x.ToString()));
//             //Debug.Log($"[PoolSpawnManager] Config: base={cfg.baseEnemySO.name} count={cfg.count} levels={levels}");
//             SpawnFromConfig(cfg);
//         }
//         //Debug.Log($"[PoolSpawnManager] Planned total spawns: {plannedSpawnTotal}");
//         //Debug.Log($"[PoolSpawnManager] Actual spawned after load: {actualSpawned}");
//     }

//     // Spawn according to a single MapEnemyConfig: uses cfg.count and cfg.level[] per spawn
//     public void SpawnFromConfig(MapEnemyConfig cfg)
//     {
//         if (cfg == null)
//         {
//             //Debug.LogWarning("[PoolSpawnManager] SpawnFromConfig called with null cfg");
//             return;
//         }
//         if (cfg.baseEnemySO == null)
//         {
//             //Debug.LogWarning("[PoolSpawnManager] SpawnFromConfig cfg.baseEnemySO is null");
//             return;
//         }

//         for (int i = 0; i < cfg.count; i++)
//         {
//             int level = 1;
//             if (cfg.level != null && cfg.level.Length > 0)
//                 level = cfg.level[i % cfg.level.Length];

//             //Debug.Log($"[PoolSpawnManager] Spawning {cfg.baseEnemySO.name} instance #{i} level={level}");
//             SpawnOne(cfg.baseEnemySO, level);
//         }
//     }

//     MapData FindMapData(int levelMap)
//     {
//         if (mapSO == null) return null;
//         foreach (var ms in mapSO)
//         {
//             if (ms == null || ms.mapData == null) continue;
//             foreach (var md in ms.mapData)
//             {
//                 if (md != null && md.levelMap == levelMap) return md;
//             }
//         }
//         return null;
//     }

//     // private void SpawnOne(BaseEnemySO enemySOData, int level)
//     // {
//     //     if (enemySOData == null) return;

//     //     // pick a variant prefab
//     //     var variant = enemySOData.GetRandomEnemyVariant();
//     //     if (variant == null)
//     //     {
//     //         //Debug.LogWarning($"[PoolSpawnManager] No variants found in BaseEnemySO {enemySOData.name}");
//     //         return;
//     //     }
//     //     if (variant.enemyPrefab == null)
//     //     {
//     //         //Debug.LogWarning($"[PoolSpawnManager] Variant {variant.enemyName} in {enemySOData.name} has null prefab");
//     //         return;
//     //     }

//     //     // ensure pool exists for this prefab
//     //     if (!poolMap.ContainsKey(variant.enemyPrefab)) poolMap[variant.enemyPrefab] = new SimpleObjectPool(variant.enemyPrefab, initialSize, this.transform);

//     //     //Debug.Log($"[PoolSpawnManager] GetNextSpawnPoint: linearIndex={linearIndex} spawnPointCount={spawnPointList?.Count ?? 0}");
//     //     Transform spawnPoint = GetNextSpawnPoint();
//     //     if (spawnPoint == null)
//     //     {
//     //         //Debug.LogWarning($"[PoolSpawnManager] GetNextSpawnPoint returned null (linearIndex={linearIndex}) - skipping spawn of {variant.enemyPrefab.name}");
//     //         return;
//     //     }

//     //     var pool = poolMap[variant.enemyPrefab];
//     //     var go = pool.Get(spawnPoint);
//     //     actualSpawned++;
//     //     //Debug.Log($"[PoolSpawnManager] Spawned #{actualSpawned}: prefab={variant.enemyPrefab.name} at {spawnPoint.name} level={level}");

//     //     // initialize enemy with level data if has EnemyBase
//     //     var data = enemySOData.GetEnemyData(level);
//     //     if (go.TryGetComponent<EnemyBase>(out var enemyBase) && data != null)
//     //     {
//     //         enemyBase.Init(data);
//     //     }

//     //     // optional: notify spawned component
//     //     if (go.TryGetComponent(out IPoolSpawned spawned)) spawned.OnSpawned();
//     // }

//     private void SpawnOne(BaseEnemySO enemySOData, int level)
//     {
//         if (enemySOData == null) return;

//         var variant = enemySOData.GetRandomEnemyVariant();
//         if (variant?.enemyPrefab == null)
//         {
//             //Debug.LogWarning($"[PoolSpawnManager] No valid prefab in {enemySOData.name}");
//             return;
//         }

//         if (!poolMap.ContainsKey(variant.enemyPrefab))
//             poolMap[variant.enemyPrefab] = new SimpleObjectPool(variant.enemyPrefab, initialSize, transform);

//         Transform spawnPoint = GetNextSpawnPoint();
//         if (spawnPoint == null)
//         {
//             //Debug.LogWarning("[PoolSpawnManager] No spawn point available.");
//             return;
//         }

//         var go = poolMap[variant.enemyPrefab].Get(spawnPoint);
//         actualSpawned++;

//         // Lấy data trước, truyền thẳng vào EnemyController
//         var data = enemySOData.GetEnemyData(level);

//         if (go.TryGetComponent<EnemyController>(out var controller))
//         {
//             controller.OnSpawn(data); // ← một điểm duy nhất init mọi thứ
//         }
//         else
//         {
//             //Debug.LogWarning($"[PoolSpawnManager] {go.name} không có EnemyController.");
//         }
//     }

//     Transform GetNextSpawnPoint()
//     {
//         if (spawnPointList == null || spawnPointList.Count == 0) return null;
//         switch (strategy)
//         {
//             case StrategyType.Linear:
//                 var t = spawnPointList[linearIndex];
//                 linearIndex = (linearIndex + 1) % spawnPointList.Count;
//                 return t;
//             case StrategyType.Random:
//                 if (randomPool == null || randomPool.Count == 0) ResetRandomPool();
//                 var idx = UnityEngine.Random.Range(0, randomPool.Count);
//                 var r = randomPool[idx];
//                 randomPool.RemoveAt(idx);
//                 return r;
//             default:
//                 return spawnPointList[0];
//         }
//     }

//     void ResetRandomPool()
//     {
//         randomPool = new List<Transform>(spawnPointList);
//     }

//     // release helper, can be called by pooled objects when collected
//     public void ReleaseToPool(GameObject go)
//     {
//         //Debug.Log($"[PoolSpawnManager] ReleaseToPool called for {go.name}");
//         var poolable = go.GetComponent<SimplePoolable>();
//         if (poolable != null) poolable.ReturnToPool();
//         else Destroy(go);
//     }
// }

// public interface IPoolSpawned
// {
//     void OnSpawned();
// }


using System;
using System.Collections.Generic;
using UnityEngine;
public interface IPoolSpawned
{
    void OnSpawned();
}
public class PoolSpawnManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] MapSO[] mapSO;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] int initialSize = 5;
    [SerializeField] int mapLevelToLoad = 1;

    [Header("Spawn Strategy")]
    public StrategyType strategy = StrategyType.Linear;
    public enum StrategyType { Linear, Random }

    public static Action<GameObject> OnRelease;

    // ── Runtime ────────────────────────────────────
    // key = prefab, value = pool của prefab đó
    readonly Dictionary<GameObject, SimpleObjectPool> _poolMap = new();
    readonly List<Transform> _spawnPts = new();

    int _linearIndex;
    List<Transform> _randomPool;

    // ── Unity ──────────────────────────────────────
    void Awake()
    {
        OnRelease -= ReleaseToPool;
        OnRelease += ReleaseToPool;

        if (spawnPoints != null) _spawnPts.AddRange(spawnPoints);
        if (strategy == StrategyType.Random) ResetRandomPool();
    }

    void Start()
    {
        LoadMap(mapLevelToLoad);
    }

    void OnDestroy()
    {
        OnRelease -= ReleaseToPool;
    }

    // ── Public API ─────────────────────────────────
    public void LoadMap(int levelMap)
    {
        var mapData = FindMapData(levelMap);
        if (mapData == null)
        {
            //Debug.LogWarning($"[PoolSpawnManager] Không tìm thấy map level {levelMap}.");
            return;
        }

        // Bước 1: warm-up pool cho đúng những prefab map này cần
        WarmUpPools(mapData);

        // Bước 2: spawn theo config
        int planned = 0, actual = 0;
        foreach (var cfg in mapData.enemyConfigs)
        {
            if (!ValidateConfig(cfg)) continue;
            planned += cfg.count;
            actual += SpawnFromConfig(cfg);
        }

        //Debug.Log($"[PoolSpawnManager] Map {levelMap} — planned:{planned} actual:{actual}");
    }

    // ── Private: map loading ───────────────────────
    MapData FindMapData(int levelMap)
    {
        if (mapSO == null) return null;
        foreach (var ms in mapSO)
        {
            if (ms?.mapData == null) continue;
            foreach (var md in ms.mapData)
                if (md?.levelMap == levelMap) return md;
        }
        return null;
    }

    // Tạo pool trước cho mọi prefab xuất hiện trong map
    // → tránh Instantiate giữa chừng khi đang spawn hàng loạt
    void WarmUpPools(MapData mapData)
    {
        foreach (var cfg in mapData.enemyConfigs)
        {
            if (!ValidateConfig(cfg)) continue;
            foreach (var variant in cfg.baseEnemySO.enemyVariants)
            {
                if (variant?.enemyPrefab == null) continue;
                if (!_poolMap.ContainsKey(variant.enemyPrefab))
                {
                    _poolMap[variant.enemyPrefab] =
                        new SimpleObjectPool(variant.enemyPrefab, initialSize, transform);

                    //Debug.Log($"[PoolSpawnManager] Pool created: {variant.enemyPrefab.name} x{initialSize}");
                }
            }
        }
    }

    int SpawnFromConfig(MapEnemyConfig cfg)
    {
        int spawned = 0;
        for (int i = 0; i < cfg.count; i++)
        {
            int level = (cfg.level != null && cfg.level.Length > 0)
                ? cfg.level[i % cfg.level.Length]
                : 1;

            if (SpawnOne(cfg.baseEnemySO, level)) spawned++;
        }
        return spawned;
    }

    bool SpawnOne(BaseEnemySO enemySOData, int level)
    {
        var variant = enemySOData.GetRandomEnemyVariant();
        if (variant?.enemyPrefab == null)
        {
            //Debug.LogWarning($"[PoolSpawnManager] Không có prefab hợp lệ trong {enemySOData.name}");
            return false;
        }

        var spawnPoint = GetNextSpawnPoint();
        if (spawnPoint == null)
        {
            //Debug.LogWarning("[PoolSpawnManager] Không có spawn point.");
            return false;
        }

        var go = _poolMap[variant.enemyPrefab].Get(spawnPoint);
        var data = enemySOData.GetEnemyData(level);

        // Try EnemyController first
        if (go.TryGetComponent<EnemyController>(out var enemyController))
        {
            enemyController.OnSpawn(data);
            //Debug.Log($"[PoolSpawnManager] ✓ Spawned ENEMY: {go.name} with level {level}");
            return true;
        }

        // Try BossBlackboard (new approach - no separate BossController needed)
        if (go.TryGetComponent<BossBlackboard>(out var blackboard))
        {
            // Convert BaseEnemyData to BossData
            // var bossData = new BossData
            // {
            //     hp = (int)data.hp,
            //     damage = (int)data.damage,
            //     moveSpeed = data.moveSpeed,
            //     element = data.element  // Get element from BaseEnemyData
            // };
            blackboard.OnSpawn(data);
            // //Debug.Log($"[PoolSpawnManager] ✓ Spawned BOSS: {go.name} with level {level}, hp={bossData.hp}, dmg={bossData.damage}, elem={bossData.element}");
            return true;
        }

        //Debug.LogWarning($"[PoolSpawnManager] ✗ {go.name} thiếu EnemyController hoặc BossBlackboard!");
        return false;
    }

    // ── Private: spawn point ───────────────────────
    Transform GetNextSpawnPoint()
    {
        if (_spawnPts == null || _spawnPts.Count == 0) return null;

        switch (strategy)
        {
            case StrategyType.Random:
                if (_randomPool == null || _randomPool.Count == 0) ResetRandomPool();
                int idx = UnityEngine.Random.Range(0, _randomPool.Count);
                var r = _randomPool[idx];
                _randomPool.RemoveAt(idx);
                return r;

            default: // Linear
                var t = _spawnPts[_linearIndex];
                _linearIndex = (_linearIndex + 1) % _spawnPts.Count;
                return t;
        }
    }

    void ResetRandomPool() => _randomPool = new List<Transform>(_spawnPts);

    // ── Private: release ───────────────────────────
    void ReleaseToPool(GameObject go)
    {
        var poolable = go.GetComponent<SimplePoolable>();
        if (poolable != null) poolable.ReturnToPool();
        else Destroy(go);
    }

    // ── Private: validation ────────────────────────
    static bool ValidateConfig(MapEnemyConfig cfg)
    {
        if (cfg == null) { 
            //Debug.LogWarning("[PoolSpawnManager] Null config."); return false; 
            }
        if (cfg.baseEnemySO == null) { 
            //Debug.LogWarning("[PoolSpawnManager] baseEnemySO null."); return false; 
            }
        if (cfg.count <= 0) { 
            //Debug.LogWarning($"[PoolSpawnManager] count={cfg.count} không hợp lệ."); return false;
             }
        return true;
    }

}