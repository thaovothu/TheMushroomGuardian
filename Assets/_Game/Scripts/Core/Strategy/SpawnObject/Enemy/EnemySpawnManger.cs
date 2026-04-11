using System.Linq;
using UnityEngine;

public class EnemySpawnManger : EntitySpawnManager
{
    [SerializeField] BaseEnemySO[] enemyList;
    [SerializeField] float spawnInterval = 2f;

    EntitySpawner<Enemy> spawner;
    CountdownTimer spawnTimer;
    int counter;
    protected override void Awake()
    {
        base.Awake();
        spawner = new EntitySpawner<Enemy>(new EntityFactory<Enemy, BaseEnemySO>(enemyList),
            spawnPointStrategy);
        spawnTimer = new CountdownTimer(spawnInterval);
        spawnTimer.OnTimerStopped += () =>
        {
            if(counter++ >= spawnPoints.Length)
            {
                spawnTimer.Stop();
                return;
            }
            Spawn();
            spawnTimer.Start();
        };
    }
    void Start() => spawnTimer.Start();
    void Update() => spawnTimer.Tick(Time.deltaTime);
    public override void Spawn() => spawner.Spawn();
}
