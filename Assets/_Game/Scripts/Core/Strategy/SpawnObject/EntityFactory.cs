using System.Collections.Generic;
using UnityEngine;

public class EntityFactory<T, TData> : IEntityFactory<T>
    where T : Entity
    where TData : IEntityData
{
    private TData[] data;
    private Dictionary<GameObject, ObjectPool> pools;

    public EntityFactory(TData[] data)
    {
        this.data = data;
        pools = new Dictionary<GameObject, ObjectPool>();
        foreach (var d in data)
        {
            if (d == null) continue;
            var prefab = d.GetPrefab();
            if (prefab == null) continue;
            if (!pools.ContainsKey(prefab)) pools[prefab] = new ObjectPool(prefab, 5);
        }
    }

    public T Create(Transform spawnPoint)
    {
        TData entityData = data[Random.Range(0, data.Length)];
        if (entityData == null) return null;
        var prefab = entityData.GetPrefab();
        if (prefab == null) return null;

        GameObject instance = pools[prefab].Get(spawnPoint);
        return instance.GetComponent<T>();
    }
}