using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Đếm số lượng item nhặt được theo quest step.
/// Lắng nghe GameEvent.Item.OnItemPickedUp — khi đủ số lượng → CompleteStep.
///
/// Setup:
///   1. Thêm QuestCollectConfig vào QuestSpawnConfig hoặc ScriptableObject riêng
///   2. Điền questId, stepId, itemId cần thu thập, requiredAmount
/// </summary>
public class QuestCollectTracker : BaseSingleton<QuestCollectTracker>
{
    [System.Serializable]
    public struct CollectObjective
    {
        public int questId;
        public int stepId;
        public int itemId;       // ItemID cần thu thập (map với ItemType)
        public int required;     // Số lượng cần thu thập
    }

    [SerializeField] private List<CollectObjective> objectives = new List<CollectObjective>();

    // Runtime counter: key = "questId:stepId" → số đã nhặt
    private Dictionary<string, int> collectedCount = new Dictionary<string, int>();

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvent.Quest.OnStepChanged += OnStepChanged;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnStepChanged -= OnStepChanged;
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void OnItemPickedUp(int itemId, int amount)
    {
        if (QuestProgressManager.Instance == null) return;

        int questId = QuestProgressManager.Instance.GetCurrentQuestId();
        int stepId = QuestProgressManager.Instance.GetActiveStepForQuest(questId);

        // Tìm objective phù hợp
        var objective = FindObjective(questId, stepId, itemId);
        if (objective == null) return;

        string key = GetKey(questId, stepId);
        if (!collectedCount.ContainsKey(key))
            collectedCount[key] = 0;

        collectedCount[key] += amount;
        int current = collectedCount[key];

        Debug.Log($"[QuestCollectTracker] Quest {questId} Step {stepId} — item {itemId}: {current}/{objective.Value.required}");

        if (current >= objective.Value.required)
        {
            int maxStepId = QuestDataManager.Instance.GetMaxStepId(questId);
            Debug.Log($"[QuestCollectTracker] Collected enough! Completing Quest {questId} Step {stepId}");
            QuestProgressManager.Instance.CompleteCurrentStep(questId, stepId, maxStepId);
        }
    }

    private void OnStepChanged(int questId, int stepId)
    {
        // Reset counter khi sang step mới
        string key = GetKey(questId, stepId);
        collectedCount[key] = 0;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private CollectObjective? FindObjective(int questId, int stepId, int itemId)
    {
        foreach (var obj in objectives)
        {
            if (obj.questId == questId && obj.stepId == stepId && obj.itemId == itemId)
                return obj;
        }
        return null;
    }

    private static string GetKey(int questId, int stepId) => $"{questId}:{stepId}";
}