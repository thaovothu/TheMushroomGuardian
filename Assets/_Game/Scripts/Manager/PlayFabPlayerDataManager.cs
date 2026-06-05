using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

/// <summary>
/// Lưu và load player data lên PlayFab UserData.
/// Auto-save khi quest step thay đổi.
/// Auto-load sau khi login thành công.
///
/// Keys được lưu trên PlayFab:
///   "QuestId"   — currentQuestId
///   "StepId"    — currentStepId
/// </summary>
public class PlayFabPlayerDataManager : BaseSingleton<PlayFabPlayerDataManager>
{
    private void OnEnable()
    {
        GameEvent.Auth.OnLoginSuccess       += OnLoggedIn;
        GameEvent.Quest.OnStepChanged       += OnStepChanged;
        GameEvent.Quest.OnQuestChanged      += OnQuestChanged;
    }

    private void OnDisable()
    {
        GameEvent.Auth.OnLoginSuccess       -= OnLoggedIn;
        GameEvent.Quest.OnStepChanged       -= OnStepChanged;
        GameEvent.Quest.OnQuestChanged      -= OnQuestChanged;
    }

    // ── Load ──────────────────────────────────────────────────────────────────

    private void OnLoggedIn(string playFabId)
    {
        LoadPlayerData();
    }

    public void LoadPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnLoadSuccess, OnError);
        Debug.Log("[PlayFabPlayerData] Loading player data...");
    }

    private void OnLoadSuccess(GetUserDataResult result)
    {
        if (result.Data == null || result.Data.Count == 0)
        {
            Debug.Log("[PlayFabPlayerData] No saved data found — new player.");
            return;
        }

        int questId = GetInt(result.Data, "QuestId", 1);
        int stepId  = GetInt(result.Data, "StepId",  1);

        Debug.Log($"[PlayFabPlayerData] Loaded — Quest: {questId}, Step: {stepId}");

        QuestProgressManager.Instance?.LoadFromCloud(questId, stepId);
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    private void OnStepChanged(int questId, int stepId)
    {
        SavePlayerData(questId, stepId);
    }

    private void OnQuestChanged(int questId)
    {
        SavePlayerData(questId, 1);
    }

    public void SavePlayerData(int questId, int stepId)
    {
        if (!PlayFabManager.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[PlayFabPlayerData] Not logged in — skip save.");
            return;
        }

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "QuestId", questId.ToString() },
                { "StepId",  stepId.ToString()  }
            }
        };

        PlayFabClientAPI.UpdateUserData(request,
            _ => Debug.Log($"[PlayFabPlayerData] Saved — Quest: {questId}, Step: {stepId}"),
            OnError);
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
