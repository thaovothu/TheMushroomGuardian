using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnGroupEntry
{
    public string spawnGroupId;
    public Transform[] spawnPoints;
}

public class SceneRegistry : MonoBehaviour
{
    public enum RegistryType { PlayerSpawnPoint, QuestSpawnGroup }

    [SerializeField] private RegistryType registryType;

    [Header("PlayerSpawnPoint")]
    [SerializeField] private Transform spawnPoint;

    [Header("QuestSpawnGroup")]
    [SerializeField] private List<SpawnGroupEntry> spawnGroups; // ← nhiều group

    private void Start()
    {
        Register();
    }

    // private void Register()
    // {
    //     Transform points = spawnPoint != null ? spawnPoint : transform;
    //     Debug.Log($"[SceneRegistry] Registering PlayerSpawnPoint: {points.position} (scene: {gameObject.scene.name})");
    //     PlayerSpawner.Instance?.RegisterSpawnPoint(points);
    //     switch (registryType)
    //     {
    //         case RegistryType.PlayerSpawnPoint:
    //             Transform point = spawnPoint != null ? spawnPoint : transform;
    //             PlayerSpawner.Instance?.RegisterSpawnPoint(point);
    //             break;

    //         case RegistryType.QuestSpawnGroup:
    //             foreach (var group in spawnGroups)
    //                 QuestSpawnManager.Instance?.RegisterSceneSpawnPoints(group.spawnGroupId, group.spawnPoints);
    //             break;
    //     }
    // }
    private void Register()
    {
        switch (registryType)
        {
            case RegistryType.PlayerSpawnPoint:
                Transform point = spawnPoint != null ? spawnPoint : transform;
                Debug.Log($"[SceneRegistry] PlayerSpawnPoint: {point.position}");
                PlayerSpawner.Instance?.RegisterSpawnPoint(point);
                break;

            case RegistryType.QuestSpawnGroup:
                foreach (var group in spawnGroups)
                    QuestSpawnManager.Instance?.RegisterSceneSpawnPoints(group.spawnGroupId, group.spawnPoints);
                break;
        }
    }
}