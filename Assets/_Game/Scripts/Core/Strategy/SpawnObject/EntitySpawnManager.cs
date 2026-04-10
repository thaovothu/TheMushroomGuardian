using UnityEngine;

public abstract class EntitySpawnManager : MonoBehaviour
{
    [SerializeField] protected SpawnPointStrategy spawnPointStrategyType = SpawnPointStrategy.Linear;
    [SerializeField] protected Transform[] spawnPoints;
    protected ISpawnPointStrategy spawnPointStrategy;
    protected enum SpawnPointStrategy
    {
        Linear,
        Random
    }
    protected virtual void Awake()
    {
        switch (spawnPointStrategyType)
        {
            case SpawnPointStrategy.Linear:
                spawnPointStrategy = new LinearSpawnPointStrategy(spawnPoints);
                break;
            case SpawnPointStrategy.Random:
                spawnPointStrategy = new RandomSpawnPointStrategy(spawnPoints);
                break;
        }
    }
    public abstract void Spawn();
}
