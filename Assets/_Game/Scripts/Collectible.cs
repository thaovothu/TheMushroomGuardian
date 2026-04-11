using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : Entity
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Call this to return the collectible to the pool instead of destroying it.
    public void ReturnToPool()
    {
        var poolable = GetComponent<Poolable>();
        if (poolable != null)
        {
            poolable.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
