using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityData
{
    GameObject GetPrefab();
}
public class EntityData : ScriptableObject, IEntityData
{
    public GameObject prefab;

    public GameObject GetPrefab()
    {
        return prefab;
    }
}
