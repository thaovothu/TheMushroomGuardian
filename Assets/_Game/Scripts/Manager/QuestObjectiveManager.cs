using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Quản lý các mục tiêu của quest (locations, NPCs, ...)
/// Mapping giữa objective name và in-game position
/// Tracking khi player đến gần mục tiêu
/// </summary>
public class QuestObjectiveManager : BaseSingleton<QuestObjectiveManager>
{
    [System.Serializable]
    public struct ObjectiveLocation
    {
        public string name;
        public Vector3 position;
        public float triggerRadius;
        public int mapId;
    }

    /// <summary>
    /// Mapping tên NPC → NpcID để tra cứu dialog.
    /// Khớp với tên trong color tag của InfoQuest.
    /// </summary>
    [System.Serializable]
    public struct NPCDialogEntry
    {
        public string npcName;   // "Tinh Linh Lumi", "Cổ Thụ Nấm"
        public int npcId;        // 1, 2, 3...
    }

    [Header("Objective Locations")]
    [SerializeField] private List<ObjectiveLocation> objectiveLocations = new List<ObjectiveLocation>();

    [Header("NPC Dialog Mapping — tên NPC → NpcID")]
    [SerializeField] private List<NPCDialogEntry> npcDialogEntries = new List<NPCDialogEntry>();

    private Dictionary<string, ObjectiveLocation> locationMap = new Dictionary<string, ObjectiveLocation>();
    private Dictionary<string, int> npcDialogMap = new Dictionary<string, int>();
    private ObjectiveLocation? currentObjective = null;

    // Quest/step đang chờ dialog hoàn thành
    private int pendingQuestId = -1;
    private int pendingStepId = -1;
    private int pendingMaxStepId = -1;

    protected override void Awake()
    {
        base.Awake();
        RebuildLocationMap();
        RebuildNPCDialogMap();
    }

    private void RebuildLocationMap()
    {
        locationMap.Clear();
        foreach (var location in objectiveLocations)
        {
            if (!string.IsNullOrEmpty(location.name))
            {
                locationMap[location.name] = location;
                Debug.Log($"[QuestObjectiveManager] Registered location: {location.name} at {location.position}");
            }
        }
    }

    private void RebuildNPCDialogMap()
    {
        npcDialogMap.Clear();
        foreach (var entry in npcDialogEntries)
        {
            if (!string.IsNullOrEmpty(entry.npcName))
                npcDialogMap[entry.npcName] = entry.npcId;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void SetObjective(int questId, string locationName)
    {
        Debug.Log($"[QuestObjectiveManager] Attempting to set objective: '{locationName}' from Quest {questId}");

        if (locationMap.ContainsKey(locationName))
        {
            currentObjective = locationMap[locationName];
            Debug.Log($"[QuestObjectiveManager] ✅ Objective set: {locationName} at {currentObjective.Value.position}");
            GameEvent.Quest.OnObjectiveSet?.Invoke(currentObjective.Value);
        }
        else
        {
            Debug.LogWarning($"[QuestObjectiveManager] ❌ Location '{locationName}' not found!");
            currentObjective = null;
        }
    }

    public void SetObjective(string locationName) => SetObjective(-1, locationName);

    public void ClearObjective()
    {
        currentObjective = null;
        Debug.Log("[QuestObjectiveManager] Objective cleared");
    }

    public ObjectiveLocation? GetCurrentObjective() => currentObjective;

    public void CheckObjectiveProximity(Vector3 playerPosition)
    {
        if (!currentObjective.HasValue) return;

        float distance = Vector3.Distance(playerPosition, currentObjective.Value.position);

        if (distance <= currentObjective.Value.triggerRadius)
        {
            Debug.Log($"[QuestObjectiveManager] Objective reached: {currentObjective.Value.name}");
            GameEvent.Quest.OnObjectiveReached?.Invoke(currentObjective.Value);
            ClearObjective();
        }
    }

    public Vector3? GetLocationPosition(string locationName)
    {
        if (locationMap.ContainsKey(locationName))
            return locationMap[locationName].position;
        return null;
    }

    public void DebugPrintAllLocations()
    {
        Debug.Log($"[QuestObjectiveManager] Total locations: {locationMap.Count}");
        foreach (var loc in locationMap)
            Debug.Log($"  {loc.Key}: {loc.Value.position} (Radius: {loc.Value.triggerRadius}, Map: {loc.Value.mapId})");
    }

    // ── Dialog callback ───────────────────────────────────────────────────────

    /// <summary>
    /// Gọi bởi UIDialog khi dialog kết thúc.
    /// Nếu đang có pending step → complete step đó.
    /// </summary>
    public void OnDialogFinished()
    {
        if (pendingQuestId == -1) return;

        Debug.Log($"[QuestObjectiveManager] Dialog finished → completing Quest {pendingQuestId} Step {pendingStepId}");
        QuestProgressManager.Instance.CompleteCurrentStep(pendingQuestId, pendingStepId, pendingMaxStepId);

        pendingQuestId = -1;
        pendingStepId = -1;
        pendingMaxStepId = -1;
    }

    // ── Auto-waypoint ─────────────────────────────────────────────────────────

    private void Start()
    {
        GameEvent.Quest.OnStepChanged += OnQuestStepChanged;
        GameEvent.Quest.OnQuestChanged += OnQuestChanged;
        GameEvent.Quest.OnDataLoaded += OnQuestDataLoaded;
        GameEvent.Quest.OnObjectiveReached += OnObjectiveReached;

        if (QuestDataManager.Instance != null && QuestDataManager.Instance.IsDataLoaded)
            TryAutoSetWaypointForCurrentStep();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEvent.Quest.OnStepChanged -= OnQuestStepChanged;
        GameEvent.Quest.OnQuestChanged -= OnQuestChanged;
        GameEvent.Quest.OnDataLoaded -= OnQuestDataLoaded;
        GameEvent.Quest.OnObjectiveReached -= OnObjectiveReached;
    }

    private void OnQuestStepChanged(int questId, int stepId) => TryAutoSetWaypoint(questId, stepId);
    private void OnQuestChanged(int newQuestId) => TryAutoSetWaypoint(newQuestId, 1);
    private void OnQuestDataLoaded(List<QuestData> _) => TryAutoSetWaypointForCurrentStep();

    private void TryAutoSetWaypointForCurrentStep()
    {
        if (QuestProgressManager.Instance == null) return;
        int questId = QuestProgressManager.Instance.GetCurrentQuestId();
        int stepId = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
        TryAutoSetWaypoint(questId, stepId);
    }

    private void TryAutoSetWaypoint(int questId, int stepId)
    {
        if (QuestDataManager.Instance == null) return;
        var questData = QuestDataManager.Instance.GetQuestStep(questId, stepId);
        if (questData == null) return;

        if (!string.IsNullOrEmpty(questData.spawnGroupId)) return;

        string locationName = ExtractLocationFromInfo(questData.infoQuest);
        if (string.IsNullOrEmpty(locationName)) return;

        Debug.Log($"[QuestObjectiveManager] Auto-waypoint for Quest {questId} Step {stepId}: '{locationName}'");
        SetObjective(questId, locationName);
    }

    private static string ExtractLocationFromInfo(string infoQuest)
    {
        if (string.IsNullOrEmpty(infoQuest)) return null;
        var match = Regex.Match(infoQuest, @"<color=[^>]*>([^<]*)</color>");
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    /// <summary>
    /// Khi player đến gần objective:
    /// - Nếu objective là NPC có dialog → mở dialog, complete step sau khi dialog xong
    /// - Nếu không có dialog → complete step ngay
    /// </summary>
    private void OnObjectiveReached(ObjectiveLocation objective)
    {
        if (QuestProgressManager.Instance == null || QuestDataManager.Instance == null) return;

        int questId = QuestProgressManager.Instance.GetCurrentQuestId();
        int stepId = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
        int maxStepId = QuestDataManager.Instance.GetMaxStepId(questId);

        // Kiểm tra objective có phải NPC có dialog không
        if (npcDialogMap.TryGetValue(objective.name, out int npcId))
        {
            // Lưu pending step — sẽ complete sau khi dialog xong
            pendingQuestId = questId;
            pendingStepId = stepId;
            pendingMaxStepId = maxStepId;

            Debug.Log($"[QuestObjectiveManager] NPC dialog → opening NpcID={npcId}, pending Quest {questId} Step {stepId}");

            // Mở dialog
            var uiDialog = FindObjectOfType<UIDialog>(true);
            if (uiDialog != null)
                uiDialog.PlayDialog(npcId);
            else
                Debug.LogWarning("[QuestObjectiveManager] UIDialog không tìm thấy trên scene!");
        }
        else
        {
            // Không có dialog → complete thẳng
            Debug.Log($"[QuestObjectiveManager] No dialog → completing Quest {questId} Step {stepId}");
            QuestProgressManager.Instance.CompleteCurrentStep(questId, stepId, maxStepId);
        }
    }
}