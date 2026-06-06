using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý tiến trình quest của player.
/// Chỉ cho phép chuyển quest tuần tự (quest 1 -> 2 -> 3 -> ...).
/// Quản lý hoàn thành step tuần tự trong mỗi quest.
/// Lắng nghe OnIntroComplete để kick off Quest 1 Step 1 sau intro cinematic.
/// </summary>
public class QuestProgressManager : BaseSingleton<QuestProgressManager>
{
    [SerializeField] private int currentQuestId = 1;
    [SerializeField] private bool dontDestroyOnLoad = true;

    // Lưu trữ các step đã hoàn thành cho mỗi quest: questId -> List<completed stepId>
    private Dictionary<int, List<int>> completedStepsByQuest = new Dictionary<int, List<int>>();
    // Lưu trữ step hiện tại (active) của mỗi quest: questId -> active stepId
    private Dictionary<int, int> activeStepByQuest = new Dictionary<int, int>();

    // ── Unity ──────────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();

        // Initialize quest tracking
        for (int i = 1; i <= 7; i++)
        {
            if (!completedStepsByQuest.ContainsKey(i))
                completedStepsByQuest[i] = new List<int>();

            if (!activeStepByQuest.ContainsKey(i))
                activeStepByQuest[i] = 1; // Step 1 là step active đầu tiên
        }

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(transform.root.gameObject);
    }

    private void OnEnable()
    {
        GameEvent.Quest.OnIntroComplete += HandleIntroComplete;
    }

    private void OnDisable()
    {
        GameEvent.Quest.OnIntroComplete -= HandleIntroComplete;
    }

    // ── Intro ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Intro cinematic kết thúc → fire OnStepChanged để tất cả manager
    /// (QuestSpawnManager, QuestObjectiveManager, ...) bắt đầu xử lý step 1.
    /// </summary>
    private void HandleIntroComplete()
    {
        int stepId = GetActiveStepForQuest(currentQuestId);
        Debug.Log($"[QuestProgressManager] Intro complete → firing OnStepChanged({currentQuestId}, {stepId})");
        GameEvent.Quest.OnStepChanged?.Invoke(currentQuestId, stepId);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy quest hiện tại.
    /// </summary>
    public int GetCurrentQuestId()
    {
        return currentQuestId;
    }

    /// <summary>
    /// Hoàn thành quest hiện tại — fires OnQuestAboutToChange để LumiQuestDialogController
    /// có thể chèn blocking dialog. Gọi ConfirmQuestAdvance() khi sẵn sàng advance thực sự.
    /// </summary>
    public void CompleteCurrentQuest()
    {
        if (currentQuestId < 7)
        {
            int nextQuestId = currentQuestId + 1;
            Debug.Log($"[QuestProgressManager] Quest {currentQuestId} complete — pending advance to Quest {nextQuestId}");

            if (GameEvent.Quest.OnQuestAboutToChange != null)
                GameEvent.Quest.OnQuestAboutToChange.Invoke(nextQuestId);
            else
                ConfirmQuestAdvance(); // fallback nếu không có handler
        }
        else
        {
            Debug.Log("[QuestProgressManager] All quests completed!");
        }
    }

    /// <summary>
    /// Thực hiện advance quest sau khi blocking dialog (nếu có) đã xong.
    /// Gọi bởi LumiQuestDialogController (hoặc trực tiếp nếu không có dialog).
    /// </summary>
    public void ConfirmQuestAdvance()
    {
        if (currentQuestId < 7)
        {
            currentQuestId++;
            Debug.Log($"[QuestProgressManager] Quest advanced → Quest {currentQuestId}");
            GameEvent.Quest.OnQuestChanged?.Invoke(currentQuestId);
        }
    }

    /// <summary>
    /// Đặt quest hiện tại (chỉ set, không trigger event).
    /// </summary>
    public void SetCurrentQuestId(int questId)
    {
        if (questId >= 1 && questId <= 7)
        {
            currentQuestId = questId;
            Debug.Log($"[QuestProgressManager] Current quest set to {currentQuestId}");
        }
        else
        {
            Debug.LogWarning($"[QuestProgressManager] Invalid quest ID: {questId}");
        }
    }

    /// <summary>
    /// Kiểm tra quest có unlocked chưa.
    /// </summary>
    public bool IsQuestUnlocked(int questId)
    {
        return questId <= currentQuestId;
    }

    /// <summary>
    /// Kiểm tra quest có đang active không.
    /// </summary>
    public bool IsQuestActive(int questId)
    {
        return questId == currentQuestId;
    }

    /// <summary>
    /// Lấy step hiện tại (active) của quest.
    /// </summary>
    public int GetActiveStepForQuest(int questId)
    {
        if (activeStepByQuest.ContainsKey(questId))
            return activeStepByQuest[questId];
        return 1;
    }

    /// <summary>
    /// Kiểm tra step có đã hoàn thành không.
    /// </summary>
    public bool IsStepCompleted(int questId, int stepId)
    {
        if (!completedStepsByQuest.ContainsKey(questId))
            return false;
        return completedStepsByQuest[questId].Contains(stepId);
    }

    /// <summary>
    /// Kiểm tra step có là active step không.
    /// </summary>
    public bool IsStepActive(int questId, int stepId)
    {
        if (!activeStepByQuest.ContainsKey(questId))
            return false;
        return activeStepByQuest[questId] == stepId;
    }

    /// <summary>
    /// Hoàn thành step hiện tại của quest.
    /// Nếu là step cuối, hoàn thành cả quest.
    /// </summary>
    public void CompleteCurrentStep(int questId, int stepId, int maxStepId)
    {
        if (!completedStepsByQuest.ContainsKey(questId))
            completedStepsByQuest[questId] = new List<int>();

        // Chỉ cho phép hoàn thành step đang active
        if (GetActiveStepForQuest(questId) != stepId)
        {
            Debug.LogWarning($"[QuestProgressManager] Cannot complete step {stepId}. Active step is {GetActiveStepForQuest(questId)}");
            return;
        }

        // Thêm step vào danh sách completed
        if (!completedStepsByQuest[questId].Contains(stepId))
        {
            completedStepsByQuest[questId].Add(stepId);
            Debug.Log($"[QuestProgressManager] Quest {questId} Step {stepId} completed!");
        }

        // Bắn event để UI nhận thưởng và cập nhật sprite trước khi advance
        GameEvent.Quest.OnStepCompleted?.Invoke(questId, stepId);

        // Nếu là step cuối, hoàn thành quest
        if (stepId == maxStepId)
        {
            Debug.Log($"[QuestProgressManager] All steps of Quest {questId} completed! Moving to Quest {questId + 1}");
            CompleteCurrentQuest();
        }
        else
        {
            // Mở step tiếp theo
            int nextStep = stepId + 1;
            activeStepByQuest[questId] = nextStep;
            GameEvent.Quest.OnStepChanged?.Invoke(questId, nextStep);
            Debug.Log($"[QuestProgressManager] Quest {questId} now on Step {nextStep}");
        }
    }

    /// <summary>
    /// Debug: In trạng thái hoàn thành của tất cả quests.
    /// </summary>
    public void DebugPrintProgress()
    {
        Debug.Log("[QuestProgressManager] === Current Progress ===");
        for (int i = 1; i <= 7; i++)
        {
            if (completedStepsByQuest.ContainsKey(i))
            {
                var completed = string.Join(", ", completedStepsByQuest[i]);
                var activeStep = GetActiveStepForQuest(i);
                Debug.Log($"  Quest {i}: Active Step={activeStep}, Completed Steps=[{completed}]");
            }
        }
    }
    //HACK
    /// <summary>Debug only — force complete step không check active.</summary>
    public void ForceCompleteStep(int questId, int stepId, int maxStepId)
    {
        if (!completedStepsByQuest.ContainsKey(questId))
            completedStepsByQuest[questId] = new List<int>();

        if (!completedStepsByQuest[questId].Contains(stepId))
            completedStepsByQuest[questId].Add(stepId);

        int next = stepId + 1;
        if (next <= maxStepId)
            activeStepByQuest[questId] = next;
    }

    /// <summary>Debug only — force set active step.</summary>
    public void ForceSetActiveStep(int questId, int stepId)
    {
        activeStepByQuest[questId] = stepId;
    }

    /// <summary>
    /// Restore quest progress từ cloud (PlayFab) sau khi login.
    /// Set state im lặng rồi fire event để các hệ thống khác cập nhật UI/spawn.
    /// </summary>
    public void LoadFromCloud(int questId, int stepId)
    {
        if (questId < 1 || questId > 7) return;

        currentQuestId = questId;
        activeStepByQuest[questId] = stepId;

        Debug.Log($"[QuestProgressManager] Loaded from cloud — Quest: {questId}, Step: {stepId}");

        GameEvent.Quest.OnQuestChanged?.Invoke(currentQuestId);
        GameEvent.Quest.OnStepChanged?.Invoke(currentQuestId, stepId);
    }
}