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
            pool.Enqueue(CreateInstance());
    }

    // Instantiate luôn inactive để NavMeshAgent không đăng ký tại vị trí sai
    private GameObject CreateInstance()
    {
        bool wasActive = prefab.activeSelf;
        prefab.SetActive(false);
        var go = GameObject.Instantiate(prefab, parent);
        prefab.SetActive(wasActive);
        var p = go.GetComponent<SimplePoolable>();
        if (p == null) p = go.AddComponent<SimplePoolable>();
        p.SetPool(this);
        return go;
    }

    public GameObject Get(Transform spawnPoint)
    {
        var go = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
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
