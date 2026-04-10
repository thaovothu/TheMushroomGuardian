using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearSpawnPointStrategy : ISpawnPointStrategy
{
    int index = 0;
    Transform[] spawnPoints;
    public LinearSpawnPointStrategy(Transform[] spawnPoints)
    {
        this.spawnPoints = spawnPoints;
    }
    public Transform NextSpawnPoint()
    {
        Transform result = spawnPoints[index];
        index = (index + 1) % spawnPoints.Length;
        return result;
    }
}
