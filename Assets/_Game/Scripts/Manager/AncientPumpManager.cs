using UnityEngine;

public class AncientPumpManager : BaseSingleton<AncientPumpManager>
{
    [SerializeField] private AncientPump[] pumps;

    [Header("Quest")]
    [SerializeField] private int questId = 4;
    [SerializeField] private int stepId = 2;

    private int activatedCount = 0;

    private void OnEnable() => GameEvent.Quest.OnStepChanged += OnStepChanged;
    private void OnDisable() => GameEvent.Quest.OnStepChanged -= OnStepChanged;

    private void OnStepChanged(int qId, int sId)
    {
        if (qId != questId || sId != stepId) return;

        activatedCount = 0;
        foreach (var pump in pumps) pump.Reset();

        // Waypoint máy bơm đầu tiên
        SetWaypointToPump(0);
        Debug.Log("[AncientPumpManager] Puzzle activated!");
    }

    public void OnPumpActivated()
    {
        activatedCount++;
        Debug.Log($"[AncientPumpManager] {activatedCount}/{pumps.Length} pumps activated");

        if (activatedCount >= pumps.Length)
        {
            // Xóa waypoint
            QuestObjectiveManager.Instance?.ClearObjective();
            CompleteStep();
        }
        else
        {
            // Waypoint máy bơm tiếp theo
            SetWaypointToPump(activatedCount);
        }
    }

    private void SetWaypointToPump(int index)
    {
        if (index >= pumps.Length) return;

        var pumpTransform = pumps[index].transform;
        var objective = new QuestObjectiveManager.ObjectiveLocation
        {
            name = $"Máy Bơm {index + 1}",
            position = pumpTransform.position
        };

        QuestObjectiveManager.Instance?.SetObjectiveDirectly(objective);
        Debug.Log($"[AncientPumpManager] Waypoint → Máy Bơm {index + 1} at {pumpTransform.position}");
    }

    private void CompleteStep()
    {
        Debug.Log("[AncientPumpManager] All pumps activated!");
        if (QuestProgressManager.Instance != null && QuestDataManager.Instance != null)
        {
            int maxStep = QuestDataManager.Instance.GetMaxStepId(questId);
            QuestProgressManager.Instance.CompleteCurrentStep(questId, stepId, maxStep);
        }
    }
}