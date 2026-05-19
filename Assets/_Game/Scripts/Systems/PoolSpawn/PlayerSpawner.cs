using UnityEngine;
using System;

/// <summary>
/// Spawn player dynamically khi scene load xong.
/// Listen vào UILoading.OnLoadingComplete event
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject content;

    private GameObject spawnedPlayer;
    
    // Event trigger khi player được spawn xong
    public static Action<GameObject> OnPlayerSpawned;

    void OnEnable()
    {
        Debug.Log("[PlayerSpawner] OnEnable - subscribing to UILoading.OnLoadingComplete");
        UILoading.OnLoadingComplete += SpawnPlayer;
    }

    void OnDisable()
    {
        Debug.Log("[PlayerSpawner] OnDisable - unsubscribing from UILoading.OnLoadingComplete");
        UILoading.OnLoadingComplete -= SpawnPlayer;
    }

    void SpawnPlayer()
    {
        Debug.Log("[PlayerSpawner] SpawnPlayer called!");
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Player prefab not assigned!");
            return;
        }
        Debug.Log("Chaydaynef");

        // Lấy vị trí spawn: dùng playerSpawnPoint nếu có, không thì dùng vị trí của PlayerSpawner
        Transform spawnTransform = playerSpawnPoint != null ? playerSpawnPoint : transform;
        Vector3 spawnPos = spawnTransform.position;
        Quaternion spawnRot = spawnTransform.rotation;

        // Spawn player thành child của PlayerSpawner
        spawnedPlayer = Instantiate(playerPrefab, spawnPos, spawnRot, content.transform);
        
        // Tag player để các system khác có thể find
        spawnedPlayer.tag = "Player";
        
        Debug.Log($"[PlayerSpawner] Player spawned at {spawnPos} with tag 'Player'");
        
        // Trigger event để các system khác biết player đã spawn
        OnPlayerSpawned?.Invoke(spawnedPlayer);
    }

    /// <summary>
    /// Get spawned player instance
    /// </summary>
    public GameObject GetSpawnedPlayer()
    {
        return spawnedPlayer;
    }
}
