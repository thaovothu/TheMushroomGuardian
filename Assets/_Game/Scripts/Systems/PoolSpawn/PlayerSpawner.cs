using UnityEngine;

/// <summary>
/// Singleton nằm trong DataGame (DontDestroyOnLoad).
/// Nhận spawn point từ SceneRegistry — spawn hoặc teleport player ngay khi nhận được point.
/// </summary>
public class PlayerSpawner : BaseSingleton<PlayerSpawner>
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject content;

    private GameObject spawnedPlayer;

    public GameObject GetSpawnedPlayer() => spawnedPlayer;

    public void RegisterSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("[PlayerSpawner] spawnPoint là null!");
            return;
        }

        Debug.Log($"[PlayerSpawner] Spawn point registered: {spawnPoint.position}");
        SpawnPlayer(spawnPoint);
    }

    private void SpawnPlayer(Transform spawnPoint)
    {
        Vector3 spawnPos = spawnPoint.position;
        Quaternion spawnRot = spawnPoint.rotation;

        // Player đã tồn tại → teleport
        var existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            TeleportPlayer(existingPlayer, spawnPos, spawnRot);
            spawnedPlayer = existingPlayer;
            Debug.Log($"[PlayerSpawner] Player teleported to {spawnPos}");
            GameEvent.Player.OnSpawned?.Invoke(spawnedPlayer);
            return;
        }

        // Chưa có player → spawn mới
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] playerPrefab chưa được gán!");
            return;
        }

        Transform parentTransform = content != null ? content.transform : null;
        spawnedPlayer = Instantiate(playerPrefab, parentTransform);
        spawnedPlayer.tag = "Player";
        TeleportPlayer(spawnedPlayer, spawnPos, spawnRot);

        Debug.Log($"[PlayerSpawner] Player spawned at {spawnedPlayer.transform.position}");
        GameEvent.Player.OnSpawned?.Invoke(spawnedPlayer);
    }

    /// <summary>
    /// Teleport player đến vị trí mới — xử lý cả Rigidbody lẫn CharacterController.
    /// </summary>
    private void TeleportPlayer(GameObject player, Vector3 position, Quaternion rotation)
    {
        // Xử lý CharacterController trước (phải disable mới set position được)
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Set transform
        Transform parent = player.transform.parent;
        player.transform.SetParent(null);
        player.transform.position = position;
        player.transform.rotation = rotation;
        player.transform.SetParent(parent);

        // Reset Rigidbody velocity + set position qua rb.position để physics sync
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            rb.rotation = rotation;
        }

        // Enable lại CharacterController
        if (cc != null) cc.enabled = true;

        Debug.Log($"[PlayerSpawner] TeleportPlayer → {position}");
    }
}