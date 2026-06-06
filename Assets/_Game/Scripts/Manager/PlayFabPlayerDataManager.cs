using System.Collections.Generic;
using System.Text;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

/// <summary>
/// Lưu và load player data lên PlayFab UserData.
///
/// Keys:
///   "QuestId"   — currentQuestId
///   "StepId"    — currentStepId
///   "Inventory" — JSON danh sách item (itemTypeId:quantity)
///   "Coins"     — số coin hiện tại
/// </summary>
public class PlayFabPlayerDataManager : BaseSingleton<PlayFabPlayerDataManager>
{
    [System.Serializable]
    private class InventorySave
    {
        public List<int> itemTypes  = new List<int>();
        public List<int> quantities = new List<int>();
    }

    private int                   _pendingQuestId       = -1;
    private int                   _pendingStepId        = -1;
    private bool                  _playerSpawned        = false;
    private bool                  _hasInventoryData     = false;
    private string                _pendingInventoryJson = null;
    private int                   _pendingCoins         = -1;
    private string                _pendingSkillsRaw     = null;
    private PlayerSkillController _skillController      = null;

    // ── Events ────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvent.Auth.OnLoginSuccess  += OnLoggedIn;
        GameEvent.Quest.OnStepChanged  += OnStepChanged;
        GameEvent.Quest.OnQuestChanged += OnQuestChanged;
        GameEvent.Player.OnSpawned     += OnPlayerSpawned;
    }

    private void OnDisable()
    {
        GameEvent.Auth.OnLoginSuccess  -= OnLoggedIn;
        GameEvent.Quest.OnStepChanged  -= OnStepChanged;
        GameEvent.Quest.OnQuestChanged -= OnQuestChanged;
        GameEvent.Player.OnSpawned     -= OnPlayerSpawned;
    }

    private void OnApplicationQuit()
    {
        SaveAll();
    }

    // ── Load ──────────────────────────────────────────────────────────────────

    private void OnLoggedIn(string _) => LoadPlayerData();

    public void LoadPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnLoadSuccess, OnError);
        Debug.Log("[PlayFabPlayerData] Loading player data...");
    }

    private void OnLoadSuccess(GetUserDataResult result)
    {
        if (result.Data == null || result.Data.Count == 0)
        {
            Debug.Log("[PlayFabPlayerData] No saved data — new player.");
            return;
        }

        int questId = GetInt(result.Data, "QuestId", 1);
        int stepId  = GetInt(result.Data, "StepId",  1);
        Debug.Log($"[PlayFabPlayerData] Loaded — Quest: {questId}, Step: {stepId}");

        // Cache inventory/coins/skills — các system chưa sẵn sàng ở giai đoạn UIAuth.
        // Sẽ apply trong OnPlayerSpawned sau khi scene load xong.
        if (result.Data.TryGetValue("Inventory", out var invRecord))
        {
            _pendingInventoryJson = invRecord.Value;
            _hasInventoryData     = true;
        }

        if (result.Data.TryGetValue("Coins", out var coinsRecord)
            && int.TryParse(coinsRecord.Value, out int coins))
        {
            _pendingCoins = coins;
        }

        if (result.Data.TryGetValue("Skills", out var skillsRecord))
            _pendingSkillsRaw = skillsRecord.Value;

        if (questId == 1 && stepId == 1) return;

        _pendingQuestId = questId;
        _pendingStepId  = stepId;

        if (_playerSpawned)
            ApplyCloudProgress();
    }

    private void OnPlayerSpawned(GameObject player)
    {
        _playerSpawned   = true;
        _skillController = player != null ? player.GetComponent<PlayerSkillController>() : null;

        if (_pendingInventoryJson != null)
        {
            RestoreInventory(_pendingInventoryJson);
            _pendingInventoryJson = null;
        }

        if (_pendingCoins >= 0)
        {
            UIMoney.RestoreCoins(_pendingCoins);
            Debug.Log($"[PlayFabPlayerData] Coins restored: {_pendingCoins}");
            _pendingCoins = -1;
        }

        if (_pendingSkillsRaw != null)
        {
            RestoreSkills(_pendingSkillsRaw);
            _pendingSkillsRaw = null;
        }

        if (_pendingQuestId > 0)
            ApplyCloudProgress();
    }

    private void ApplyCloudProgress()
    {
        int questId = _pendingQuestId;
        int stepId  = _pendingStepId;
        _pendingQuestId = -1;
        _pendingStepId  = -1;

        var panel = UIDebugPanel.Instance;
        if (panel == null)
        {
            Debug.LogError("[PlayFabPlayerData] UIDebugPanel.Instance null — cannot apply cloud progress.");
            return;
        }

        Debug.Log($"[PlayFabPlayerData] Applying cloud progress → Quest {questId} Step {stepId}");
        // Nếu đã có inventory từ cloud → không grant lại reward (tránh duplicate item/coin)
        panel.JumpToQuestStep(questId, stepId, grantRewards: !_hasInventoryData);
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    private void OnStepChanged(int questId, int stepId) => SaveAll(questId, stepId);
    private void OnQuestChanged(int questId)            => SaveAll(questId, 1);

    /// <summary>Save toàn bộ: quest + inventory + coins.</summary>
    public void SaveAll(int questId = -1, int stepId = -1)
    {
        if (!PlayFabManager.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[PlayFabPlayerData] Not logged in — skip save.");
            return;
        }

        if (questId < 0 && QuestProgressManager.Instance != null)
        {
            questId = QuestProgressManager.Instance.GetCurrentQuestId();
            stepId  = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
        }

        var data = new Dictionary<string, string>
        {
            { "QuestId",   questId.ToString() },
            { "StepId",    stepId.ToString()  },
            { "Inventory", SerializeInventory() },
            { "Coins",     UIMoney.TotalCoins.ToString() },
            { "Skills",    SerializeSkills() }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest { Data = data },
            _ => Debug.Log($"[PlayFabPlayerData] Saved — Quest:{questId} Step:{stepId} Coins:{UIMoney.TotalCoins}"),
            OnError);
    }

    // ── Inventory serialize ───────────────────────────────────────────────────

    private static string SerializeInventory()
    {
        var save = new InventorySave();
        if (InventorySystem.Instance == null) return JsonUtility.ToJson(save);

        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            if (type == ItemType.None) continue;
            int qty = InventorySystem.Instance.GetItemQuantity(type);
            if (qty <= 0) continue;
            save.itemTypes.Add((int)type);
            save.quantities.Add(qty);
        }

        return JsonUtility.ToJson(save);
    }

    private static void RestoreInventory(string json)
    {
        if (string.IsNullOrEmpty(json) || InventorySystem.Instance == null) return;

        var save = JsonUtility.FromJson<InventorySave>(json);
        if (save == null) return;

        InventorySystem.Instance.ClearInventory();

        for (int i = 0; i < save.itemTypes.Count; i++)
        {
            int qty = save.quantities[i];
            if (qty > 0)
                InventorySystem.Instance.AddItem(save.itemTypes[i], qty);
        }

        Debug.Log($"[PlayFabPlayerData] Inventory restored: {save.itemTypes.Count} item type(s)");
    }

    // ── Skill serialize ───────────────────────────────────────────────────────

    private string SerializeSkills()
    {
        if (_skillController == null) return "";
        var elements = _skillController.GetUnlockedElements();
        if (elements.Count == 0) return "";

        var parts = new StringBuilder();
        foreach (var e in elements)
        {
            if (parts.Length > 0) parts.Append(',');
            parts.Append((int)e);
        }
        return parts.ToString();
    }

    private static void RestoreSkills(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return;
        foreach (var token in raw.Split(','))
        {
            if (int.TryParse(token.Trim(), out int val))
                GameEvent.Player.OnSkillUnlocked?.Invoke((ElementType)val);
        }
        Debug.Log($"[PlayFabPlayerData] Skills restored: {raw}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int GetInt(Dictionary<string, UserDataRecord> data, string key, int defaultValue)
    {
        if (data.TryGetValue(key, out var record) && int.TryParse(record.Value, out int value))
            return value;
        return defaultValue;
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError($"[PlayFabPlayerData] Error: {error.GenerateErrorReport()}");
    }
}
