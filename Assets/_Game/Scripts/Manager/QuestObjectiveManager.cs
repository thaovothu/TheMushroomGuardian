using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

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

    [System.Serializable]
    public struct NPCDialogEntry
    {
        public string npcName;
        public int npcId;
    }

    [Header("Objective Locations")]
    [SerializeField] private List<ObjectiveLocation> objectiveLocations = new List<ObjectiveLocation>();

    [Header("NPC Dialog Mapping")]
    [SerializeField] private List<NPCDialogEntry> npcDialogEntries = new List<NPCDialogEntry>();

    [Header("NPC Unlock Mapping")]
    [SerializeField] private List<string> npcUnlockNames = new List<string>();

    [Header("Puzzle Locations — đến nơi nhưng KHÔNG complete, chờ puzzle xử lý")]
    [SerializeField] private List<string> puzzleLocationNames = new List<string>();

    private HashSet<string> puzzleLocationSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, ObjectiveLocation> locationMap = new Dictionary<string, ObjectiveLocation>();
    private Dictionary<string, int> npcDialogMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> npcUnlockSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private ObjectiveLocation? currentObjective = null;
    private Dictionary<string, QuestNPCMover> npcMoverMap = new Dictionary<string, QuestNPCMover>();

    public void RegisterNPCMover(string name, QuestNPCMover mover) => npcMoverMap[name] = mover;
    public void UnregisterNPCMover(string name) => npcMoverMap.Remove(name);

    private int pendingQuestId = -1;
    private int pendingStepId = -1;
    private int pendingMaxStepId = -1;

    protected override void Awake()
    {
        base.Awake();
        RebuildLocationMap();
        RebuildNPCMaps();
    }

    private void RebuildLocationMap()
    {
        locationMap.Clear();
        foreach (var location in objectiveLocations)
            if (!string.IsNullOrEmpty(location.name))
            {
                locationMap[location.name] = location;
                Debug.Log($"[QuestObjectiveManager] Registered location: {location.name} at {location.position}");
            }
    }

    private void RebuildNPCMaps()
    {
        npcDialogMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in npcDialogEntries)
            if (!string.IsNullOrEmpty(entry.npcName))
                npcDialogMap[entry.npcName] = entry.npcId;

        npcUnlockSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in npcUnlockNames)
            if (!string.IsNullOrEmpty(name))
                npcUnlockSet.Add(name);

        puzzleLocationSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in puzzleLocationNames)
            if (!string.IsNullOrEmpty(name))
                puzzleLocationSet.Add(name);
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

    public void SetObjectiveDirectly(ObjectiveLocation objective)
    {
        currentObjective = objective;
        GameEvent.Quest.OnObjectiveSet?.Invoke(objective);
        Debug.Log($"[QuestObjectiveManager] Direct objective set: {objective.name} at {objective.position}");
    }

    public void ClearObjective()
    {
        currentObjective = null;
        GameEvent.Quest.OnObjectiveReached?.Invoke(default);
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
            var reached = currentObjective.Value;
            currentObjective = null;
            GameEvent.Quest.OnObjectiveReached?.Invoke(reached);
        }
    }

    public Vector3? GetLocationPosition(string locationName)
    {
        if (locationMap.ContainsKey(locationName))
            return locationMap[locationName].position;
        return null;
    }

    // ── Dialog callback ───────────────────────────────────────────────────────

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

    // ── Objective reached ─────────────────────────────────────────────────────

    private void OnObjectiveReached(ObjectiveLocation objective)
    {
        if (QuestProgressManager.Instance == null || QuestDataManager.Instance == null) return;

        // Bỏ qua event rỗng từ ClearObjective
        if (string.IsNullOrEmpty(objective.name)) return;

        Debug.Log($"[QuestObjectiveManager] objective.name='{objective.name}'");
        Debug.Log($"[QuestObjectiveManager] npcDialogMap keys: {string.Join(", ", npcDialogMap.Keys)}");
        Debug.Log($"[QuestObjectiveManager] npcUnlockSet: {string.Join(", ", npcUnlockSet)}");

        int questId = QuestProgressManager.Instance.GetCurrentQuestId();
        int stepId = QuestProgressManager.Instance.GetActiveStepForQuest(questId);
        int maxStepId = QuestDataManager.Instance.GetMaxStepId(questId);

        // 1. NPC Unlock
        if (npcUnlockSet.Contains(objective.name))
        {
            Debug.Log($"[QuestObjectiveManager] NPC Unlock '{objective.name}' — chờ player interact");
            return;
        }

        // 2. NPC Dialog
        if (npcDialogMap.TryGetValue(objective.name, out int npcId))
        {
            pendingQuestId = questId;
            pendingStepId = stepId;
            pendingMaxStepId = maxStepId;

            if (npcMoverMap.TryGetValue(objective.name, out var mover))
                mover.MoveToTarget();

            Debug.Log($"[QuestObjectiveManager] NPC dialog → NpcID={npcId}, pending Quest {questId} Step {stepId}");

            var uiDialog = FindObjectOfType<UIDialog>(true);
            if (uiDialog != null)
                uiDialog.PlayDialog(npcId);
            else
                Debug.LogWarning("[QuestObjectiveManager] UIDialog không tìm thấy!");
            return;
        }

        // 3. Puzzle Location
        if (puzzleLocationSet.Contains(objective.name))
        {
            Debug.Log($"[QuestObjectiveManager] Puzzle location '{objective.name}' — chờ puzzle complete");
            return;
        }

        // 4. Không có gì đặc biệt → complete thẳng
        Debug.Log($"[QuestObjectiveManager] No special action → completing Quest {questId} Step {stepId}");
        QuestProgressManager.Instance.CompleteCurrentStep(questId, stepId, maxStepId);
    }
}