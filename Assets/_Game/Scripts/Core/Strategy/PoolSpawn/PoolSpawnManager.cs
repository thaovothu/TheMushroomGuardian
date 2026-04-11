using System.Collections.Generic;
using UnityEngine;

public class PoolSpawnManager : MonoBehaviour
{
    [SerializeField] BaseEnemySO[] enemySO;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] int initialSize = 5;
    [SerializeField] float spawnInterval = 1f;
    [SerializeField] public int spawnPerTick = 1;

    public enum StrategyType { Linear, Random }
    public StrategyType strategy = StrategyType.Linear;

    Dictionary<GameObject, SimpleObjectPool> poolMap = new Dictionary<GameObject, SimpleObjectPool>();
    List<Transform> spawnPointList = new List<Transform>();

    float timer;
    int linearIndex = 0;
    List<Transform> randomPool;

    void Awake()
    {
        foreach (BaseEnemySO p in enemySO)
        {
            if (p == null || p.prefab == null) continue;
            if (!poolMap.ContainsKey(p.prefab)) poolMap[p.prefab] = new SimpleObjectPool(p.prefab, initialSize, this.transform);
        }

        if (spawnPoints != null) spawnPointList.AddRange(spawnPoints);
        timer = spawnInterval;
        if (strategy == StrategyType.Random) ResetRandomPool();
    }

    void Start()
    {
        SpawnEnemy(enemySO[0]);
    }

    private void SpawnEnemy(BaseEnemySO enemy)
    {
        for (int i = 0; i < 3; i++) SpawnOne(enemy);
    }

    void SpawnOne(BaseEnemySO enemy)
    {
        if (enemy == null || enemy.prefab == null) return;

        Transform spawnPoint = GetNextSpawnPoint();
        if (spawnPoint == null) return;

        var pool = poolMap[enemy.prefab];
        var go = pool.Get(spawnPoint);

        // optional: call OnSpawn interface or component
        // if (go.TryGetComponent(out IPoolSpawned spawned)) spawned.OnSpawned();
    }

    Transform GetNextSpawnPoint()
    {
        if (spawnPointList == null || spawnPointList.Count == 0) return null;
        switch (strategy)
        {
            case StrategyType.Linear:
                var t = spawnPointList[linearIndex];
                linearIndex = (linearIndex + 1) % spawnPointList.Count;
                return t;
            case StrategyType.Random:
                if (randomPool == null || randomPool.Count == 0) ResetRandomPool();
                var idx = Random.Range(0, randomPool.Count);
                var r = randomPool[idx];
                randomPool.RemoveAt(idx);
                return r;
            default:
                return spawnPointList[0];
        }
    }

    void ResetRandomPool()
    {
        randomPool = new List<Transform>(spawnPointList);
    }

    // release helper, can be called by pooled objects when collected
    public void ReleaseToPool(GameObject go)
    {
        var poolable = go.GetComponent<SimplePoolable>();
        if (poolable != null) poolable.ReturnToPool();
        else Destroy(go);
    }
}

public interface IPoolSpawned
{
    void OnSpawned();
}
