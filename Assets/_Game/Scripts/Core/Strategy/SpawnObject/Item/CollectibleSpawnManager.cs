using UnityEngine;
public class CollectibleSpawnManager : EntitySpawnManager
{
    [SerializeField] CollectibleData[] collectibleData;
    [SerializeField] float spawnInterval = 1f;

    EntitySpawner<Collectible> spawner;

    CountdownTimer spawnTimer;
    int counter;

    protected override void Awake()
    {
        base.Awake();
        spawner = new EntitySpawner<Collectible>(new EntityFactory<Collectible, CollectibleData>(collectibleData),
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