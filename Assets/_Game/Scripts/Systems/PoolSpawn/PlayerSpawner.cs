using UnityEngine;

public class PlayerSpawner : BaseSingleton<PlayerSpawner>
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject content;

    private GameObject spawnedPlayer;
    private string lastScene = "";

    public GameObject GetSpawnedPlayer() => spawnedPlayer;

    public void RegisterSpawnPoint(Transform point)
    {
        if (point == null)
        {
            Debug.LogWarning("[PlayerSpawner] spawnPoint là null!");
            return;
        }

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (spawnedPlayer == null)
        {
            // Chưa có player → spawn mới
            lastScene = currentScene;
            SpawnPlayer(point);
        }
        else if (currentScene != lastScene)
        {
            // Chuyển sang scene mới → teleport
            lastScene = currentScene;
            TeleportPlayer(spawnedPlayer, point.position, point.rotation);
            GameEvent.Player.OnSpawned?.Invoke(spawnedPlayer);
        }
        else
        {
            // Cùng scene reload → bỏ qua
            Debug.Log($"[PlayerSpawner] Same scene reload — skipping teleport");
        }
    }

    private void SpawnPlayer(Transform spawnPoint)
    {
        Vector3 spawnPos = spawnPoint.position;
        Quaternion spawnRot = spawnPoint.rotation;

        // Player đã tồn tại trong scene (DontDestroyOnLoad) → teleport
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

    private void TeleportPlayer(GameObject player, Vector3 position, Quaternion rotation)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Transform parent = player.transform.parent;
        player.transform.SetParent(null);
        player.transform.position = position;
        player.transform.rotation = rotation;
        player.transform.SetParent(parent);

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            rb.rotation = rotation;
        }

        if (cc != null) cc.enabled = true;

        Debug.Log($"[PlayerSpawner] TeleportPlayer → {position}");
    }
}