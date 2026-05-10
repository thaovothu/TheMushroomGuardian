using System;
using System.Collections.Generic;
using UnityEngine;
public interface IPoolSpawned
{
    void OnSpawned();
}
public class PoolSpawnManager : BaseSingleton<PoolSpawnManager>
{
    [Header("Config")]
    [SerializeField] MapSO[] mapSO;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] GameObject DataEnemy;
    [SerializeField] int initialSize = 5;
    [SerializeField] int mapLevelToLoad = 1;

    [Header("Spawn Strategy")]
    public StrategyType strategy = StrategyType.Linear;
    public enum StrategyType { Linear, Random }

    public Action<GameObject> OnRelease;

    // ── Runtime ────────────────────────────────────
    // key = prefab, value = pool của prefab đó
    readonly Dictionary<GameObject, SimpleObjectPool> _poolMap = new();
    readonly List<Transform> _spawnPts = new();

    int _linearIndex;
    List<Transform> _randomPool;

    // ── Unity ──────────────────────────────────────
    void Awake()
    {
        OnRelease += ReleaseToPool;

        if (spawnPoints != null) _spawnPts.AddRange(spawnPoints);
        if (strategy == StrategyType.Random) ResetRandomPool();
    }

    void OnEnable()
    {
        // Chờ loading xong mới spawn quái
        UILoading.OnLoadingComplete += OnSceneLoadingComplete;
    }

    void OnDisable()
    {
        UILoading.OnLoadingComplete -= OnSceneLoadingComplete;
    }

    void OnDestroy()
    {
        OnRelease -= ReleaseToPool;
        UILoading.OnLoadingComplete -= OnSceneLoadingComplete;
    }

    // ── Event Handlers ─────────────────────────────
    void OnSceneLoadingComplete()
    {
        LoadMap(mapLevelToLoad);
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
                    // Tạo pool với parent là DataEnemy (hoặc PoolSpawnManager nếu DataEnemy chưa gán)
                    Transform poolParent = DataEnemy != null ? DataEnemy.transform : transform;
                    _poolMap[variant.enemyPrefab] =
                        new SimpleObjectPool(variant.enemyPrefab, initialSize, poolParent);

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
            blackboard.OnSpawn(data);
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