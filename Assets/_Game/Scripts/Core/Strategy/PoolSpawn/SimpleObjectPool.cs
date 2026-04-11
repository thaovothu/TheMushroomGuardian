using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPool
{
    GameObject prefab;
    Transform parent;
    Queue<GameObject> pool = new Queue<GameObject>();

    public SimpleObjectPool(GameObject prefab, int initialSize = 5, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < initialSize; i++)
        {
            var go = GameObject.Instantiate(prefab, parent);
            go.SetActive(false);
            var p = go.GetComponent<SimplePoolable>();
            if (p == null) p = go.AddComponent<SimplePoolable>();
            p.SetPool(this);
            pool.Enqueue(go);
        }
    }

    public GameObject Get(Transform spawnPoint)
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Dequeue();
        }
        else
        {
            go = GameObject.Instantiate(prefab, parent);
            var p = go.GetComponent<SimplePoolable>();
            if (p == null) p = go.AddComponent<SimplePoolable>();
            p.SetPool(this);
        }

        go.transform.position = spawnPoint.position;
        go.transform.rotation = spawnPoint.rotation;
        go.SetActive(true);
        return go;
    }

    public void Release(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }
}
