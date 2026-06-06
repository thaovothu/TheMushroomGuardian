using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lưu checkpoint khi hoàn thành mỗi step (sau khi rewards đã vào inventory).
/// Khi player chết ở step tiếp theo: teleport về vị trí đã lưu và hoàn nguyên inventory,
/// giữ lại những item hệ thống đã cấp (đã lưu trong checkpoint trước đó).
/// </summary>
public class CheckpointManager : BaseSingleton<CheckpointManager>
{
    [SerializeField] private float respawnDelay = 2f;

    private Vector3 _savedPosition;
    private Dictionary<ItemType, (ItemData data, int quantity)> _savedInventory;
    private int _savedCoins;
    private bool _hasCheckpoint;

    private void OnEnable()
    {
        GameEvent.Combat.OnDeath += OnEntityDeath;
    }

    private void OnDisable()
    {
        GameEvent.Combat.OnDeath -= OnEntityDeath;
    }

    /// <summary>
    /// Lưu vị trí player và snapshot inventory hiện tại.
    /// Gọi bởi QuestRewardManager SAU KHI đã cấp phát toàn bộ rewards của step.
    /// </summary>
    public void SaveCheckpoint()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[CheckpointManager] Player not found — checkpoint not saved");
            return;
        }

        _savedPosition = player.transform.position;
        _savedInventory = InventorySystem.Instance?.GetInventorySnapshot();
        _savedCoins = UIMoney.TotalCoins;
        _hasCheckpoint = true;

        Debug.Log($"[CheckpointManager] Checkpoint saved at {_savedPosition} | {_savedInventory?.Count ?? 0} item type(s) | {_savedCoins} coins");
    }

    private void OnEntityDeath(HealthSystem hs)
    {
        if (!hs.CompareTag("Player")) return;
        StartCoroutine(RespawnCoroutine(hs));
    }

    private IEnumerator RespawnCoroutine(HealthSystem hs)
    {
        yield return new WaitForSeconds(respawnDelay);

        if (hs == null) yield break;

        var player = hs.gameObject;

        // Dừng mọi velocity trước khi teleport để tránh drift
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;

        if (_hasCheckpoint)
        {
            player.transform.position = _savedPosition;

            // Hoàn nguyên inventory — vật phẩm nhặt ở step hiện tại biến mất,
            // quest rewards ở step trước vẫn còn (đã lưu trong snapshot).
            if (_savedInventory != null)
                InventorySystem.Instance?.RestoreInventory(_savedInventory);

            UIMoney.RestoreCoins(_savedCoins);
        }

        // Reset HP về full
        hs.Init(hs.MaxHealth);

        // Despawn enemies còn sống và spawn lại toàn bộ step
        if (QuestProgressManager.Instance != null)
        {
            int questId = QuestProgressManager.Instance.GetCurrentQuestId();
            int stepId = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
            QuestSpawnManager.Instance?.ResetStepForRespawn(questId, stepId);
        }

        GameEvent.Player.OnRespawn?.Invoke();
        Debug.Log($"[CheckpointManager] Player respawned at {player.transform.position}");
    }
}
