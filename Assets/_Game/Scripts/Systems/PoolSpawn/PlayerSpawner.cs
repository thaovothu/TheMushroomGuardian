using UnityEngine;

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
    
    void OnEnable()
    {
        Debug.Log("[PlayerSpawner] OnEnable - subscribing to UILoading.OnLoadingComplete");
        UILoading.OnLoadingComplete += SpawnPlayer;
        Debug.Log("[PlayerSpawner] ✅ Subscribed to UILoading.OnLoadingComplete");
    }

    void OnDisable()
    {
        Debug.Log("[PlayerSpawner] OnDisable - unsubscribing from UILoading.OnLoadingComplete");
        UILoading.OnLoadingComplete -= SpawnPlayer;
    }

    void SpawnPlayer()
    {
        if (spawnedPlayer != null)
        {
            Debug.LogWarning("[PlayerSpawner] Player already spawned — skipping duplicate spawn");
            return;
        }

        Debug.Log("[PlayerSpawner] SpawnPlayer called!");
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Player prefab not assigned!");
            return;
        }

        // Lấy vị trí spawn: dùng playerSpawnPoint nếu có, không thì dùng vị trí của PlayerSpawner
        Transform spawnTransform = playerSpawnPoint != null ? playerSpawnPoint : transform;
        Vector3 spawnPos = spawnTransform.position;
        Quaternion spawnRot = spawnTransform.rotation;

        // Spawn player thành child của PlayerSpawner
        spawnedPlayer = Instantiate(playerPrefab, spawnPos, spawnRot, content.transform);
        
        // Tag player để các system khác có thể find
        spawnedPlayer.tag = "Player";
        
        Debug.Log($"[PlayerSpawner] Player spawned at {spawnPos} with tag 'Player'");
        
        GameEvent.Player.OnSpawned?.Invoke(spawnedPlayer);
        Debug.Log($"[PlayerSpawner] Player spawned: {spawnedPlayer.name}");
    }

    /// <summary>
    /// Get spawned player instance
    /// </summary>
    public GameObject GetSpawnedPlayer()
    {
        return spawnedPlayer;
    }
}
