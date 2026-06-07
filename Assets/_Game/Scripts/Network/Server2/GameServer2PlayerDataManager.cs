using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Lưu và load player data lên Server2 (ASP.NET Core + PostgreSQL).
/// Mirrors PlayFabPlayerDataManager — cùng logic, khác transport (HTTP thay PlayFab SDK).
///
/// Fields lưu:
///   questId, stepId, coins, inventoryJson, skillsCsv
/// </summary>
public class GameServer2PlayerDataManager : BaseSingleton<GameServer2PlayerDataManager>
{
    [System.Serializable]
    private class InventorySave
    {
        public List<int> itemTypes  = new List<int>();
        public List<int> quantities = new List<int>();
    }

    [System.Serializable]
    private class PlayerDataPayload
    {
        public int    questId;
        public int    stepId;
        public int    coins;
        public string inventoryJson;
        public string skillsCsv;
    }

    private int                   _pendingQuestId       = -1;
    private int                   _pendingStepId        = -1;
    private bool                  _playerSpawned        = false;
    private bool                  _hasInventoryData     = false;
    private string                _pendingInventoryJson = null;
    private int                   _pendingCoins         = -1;
    private string                _pendingSkillsRaw     = null;
    private PlayerSkillController _skillController      = null;

    private string BaseUrl => ServerConfig.Instance.Server2BaseUrl;
    private string Token   => GameServer2Manager.Instance.Token;

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

    private void OnApplicationQuit() => SaveAll();

    // ── Load ──────────────────────────────────────────────────────────────────

    private void OnLoggedIn(string _)
    {
        if (!ServerConfig.Instance.IsServer2Active) return;
        StartCoroutine(LoadCoroutine());
    }

    public void LoadPlayerData() => StartCoroutine(LoadCoroutine());

    private IEnumerator LoadCoroutine()
    {
        using var req = MakeGet($"{BaseUrl}/api/player/data", Token);
        Debug.Log("[Server2PlayerData] Loading player data...");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[Server2PlayerData] Load failed: {req.downloadHandler.text}");
            yield break;
        }

        var data = JsonUtility.FromJson<PlayerDataPayload>(req.downloadHandler.text);
        if (data == null) yield break;

        Debug.Log($"[Server2PlayerData] Loaded — Quest: {data.questId}, Step: {data.stepId}");

        if (!string.IsNullOrEmpty(data.inventoryJson) && data.inventoryJson != "{}")
        {
            _pendingInventoryJson = data.inventoryJson;
            _hasInventoryData     = true;
        }

        if (data.coins > 0)                     _pendingCoins    = data.coins;
        if (!string.IsNullOrEmpty(data.skillsCsv)) _pendingSkillsRaw = data.skillsCsv;

        if (data.questId == 1 && data.stepId == 1) yield break;

        _pendingQuestId = data.questId;
        _pendingStepId  = data.stepId;

        if (_playerSpawned) ApplyCloudProgress();
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
            Debug.Log($"[Server2PlayerData] Coins restored: {_pendingCoins}");
            _pendingCoins = -1;
        }

        if (_pendingSkillsRaw != null)
        {
            RestoreSkills(_pendingSkillsRaw);
            _pendingSkillsRaw = null;
        }

        if (_pendingQuestId > 0) ApplyCloudProgress();
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
            Debug.LogError("[Server2PlayerData] UIDebugPanel.Instance null — cannot apply cloud progress.");
            return;
        }

        Debug.Log($"[Server2PlayerData] Applying cloud progress → Quest {questId} Step {stepId}");
        panel.JumpToQuestStep(questId, stepId, grantRewards: !_hasInventoryData);
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    private void OnStepChanged(int questId, int stepId) => SaveAll(questId, stepId);
    private void OnQuestChanged(int questId)            => SaveAll(questId, 1);

    public void SaveAll(int questId = -1, int stepId = -1)
    {
        if (!ServerConfig.Instance.IsServer2Active) return;
        if (!GameServer2Manager.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[Server2PlayerData] Not logged in — skip save.");
            return;
        }

        if (questId < 0 && QuestProgressManager.Instance != null)
        {
            questId = QuestProgressManager.Instance.GetCurrentQuestId();
            stepId  = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
        }

        StartCoroutine(SaveCoroutine(questId, stepId));
    }

    private IEnumerator SaveCoroutine(int questId, int stepId)
    {
        var payload = new PlayerDataPayload
        {
            questId      = questId,
            stepId       = stepId,
            coins        = UIMoney.TotalCoins,
            inventoryJson = SerializeInventory(),
            skillsCsv    = SerializeSkills()
        };

        string json = JsonUtility.ToJson(payload);
        using var req = MakePut($"{BaseUrl}/api/player/data", json, Token);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            Debug.Log($"[Server2PlayerData] Saved — Quest:{questId} Step:{stepId} Coins:{UIMoney.TotalCoins}");
        else
            Debug.LogError($"[Server2PlayerData] Save failed: {req.downloadHandler.text}");
    }

    // ── Inventory serialize (giống PlayFabPlayerDataManager) ─────────────────

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
            if (qty > 0) InventorySystem.Instance.AddItem(save.itemTypes[i], qty);
        }

        Debug.Log($"[Server2PlayerData] Inventory restored: {save.itemTypes.Count} item type(s)");
    }

    // ── Skill serialize (giống PlayFabPlayerDataManager) ─────────────────────

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
        Debug.Log($"[Server2PlayerData] Skills restored: {raw}");
    }

    // ── HTTP helpers ──────────────────────────────────────────────────────────

    private static UnityWebRequest MakeGet(string url, string token)
    {
        var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        return req;
    }

    private static UnityWebRequest MakePut(string url, string json, string token)
    {
        var req = new UnityWebRequest(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        return req;
    }
}
