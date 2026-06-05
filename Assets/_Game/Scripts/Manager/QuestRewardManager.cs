using UnityEngine;

/// <summary>
/// Lắng nghe OnStepCompleted và trao thưởng dựa theo cột ItemReward / SkillReward trong quest.tsv:
///   ItemReward  — số (vd: 10) → vàng | "Kiếm" → Sword | "Cung" → Bow
///   SkillReward — "Skill Đất/Khí/Nước/Lửa" → mở khóa nguyên tố tương ứng
/// </summary>
public class QuestRewardManager : BaseSingleton<QuestRewardManager>
{
    [SerializeField] private bool dontDestroyOnLoad = true;

    protected override void Awake()
    {
        base.Awake();
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(transform.root.gameObject);
    }

    private void OnEnable()
    {
        GameEvent.Quest.OnStepCompleted += HandleStepCompleted;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnStepCompleted -= HandleStepCompleted;
    }

    private void HandleStepCompleted(int questId, int stepId)
    {
        var data = QuestDataManager.Instance?.GetQuestStep(questId, stepId);
        if (data == null) return;

        HandleItemReward(questId, stepId, data.itemReward1?.Trim());
        HandleSkillReward(questId, stepId, data.skillReward?.Trim());
        HandleExtendReward(questId, stepId, data.reward?.Trim());

        // Lưu checkpoint SAU KHI rewards đã vào inventory — respawn sẽ khôi phục về trạng thái này.
        CheckpointManager.Instance?.SaveCheckpoint();
    }

    private void HandleItemReward(int questId, int stepId, string raw)
    {
        if (string.IsNullOrEmpty(raw)) return;

        if (int.TryParse(raw, out int coins))
        {
            UIMoney.Instance?.AddCoin(coins);
            Debug.Log($"[QuestRewardManager] {questId}-{stepId}: +{coins} vàng");
        }
        else if (raw == "Kiếm")
        {
            InventorySystem.Instance?.AddItem((int)ItemType.Sword);
            Debug.Log($"[QuestRewardManager] {questId}-{stepId}: nhận Kiếm");
        }
        else if (raw == "Cung")
        {
            InventorySystem.Instance?.AddItem((int)ItemType.Bow);
            Debug.Log($"[QuestRewardManager] {questId}-{stepId}: nhận Cung");
        }
    }

    private void HandleExtendReward(int questId, int stepId, string raw)
    {
        if (string.IsNullOrEmpty(raw)) return;

        if (raw == "Tinh linh")
        {
            GameEvent.Player.OnLumiUnlocked?.Invoke();

            // Lumi là standalone GO (không phải child player) — tìm qua FindObjectOfType
            var lumi = Object.FindObjectOfType<LumiController>(true);
            if (lumi != null)
                lumi.UnlockAndFollow();
            else
                Debug.LogWarning("[QuestRewardManager] LumiController not found in scene!");

            Debug.Log($"[QuestRewardManager] {questId}-{stepId}: Tinh linh Lumi unlocked");
        }
    }

    private void HandleSkillReward(int questId, int stepId, string raw)
    {
        if (string.IsNullOrEmpty(raw)) return;

        ElementType? element = raw.ToLower() switch
        {
            "skill đất" => ElementType.Earth,
            "skill khí" => ElementType.Wind,
            "skill nước" => ElementType.Water,
            "skill lửa" => ElementType.Fire,
            _ => null
        };

        if (element == null) return;

        GameEvent.Player.OnSkillUnlocked?.Invoke(element.Value);
        Debug.Log($"[QuestRewardManager] {questId}-{stepId}: mở khóa Skill {element.Value}");
    }
}
