using System;
using UnityEngine;

/// <summary>
/// Quản lý tiến trình quest của player
/// Chỉ cho phép chuyển quest tuần tự (quest 1 -> 2 -> 3 -> ...)
/// </summary>
public class QuestProgressManager : BaseSingleton<QuestProgressManager>
{
    [SerializeField] private int currentQuestId = 1;
    [SerializeField] private bool dontDestroyOnLoad = true;

    public event Action<int> OnQuestChanged; // Trigger khi quest thay đổi

    protected override void Awake()
    {
        base.Awake();
        
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Lấy quest hiện tại
    /// </summary>
    public int GetCurrentQuestId()
    {
        return currentQuestId;
    }

    /// <summary>
    /// Hoàn thành quest hiện tại, chuyển sang quest tiếp theo
    /// </summary>
    public void CompleteCurrentQuest()
    {
        if (currentQuestId < 6) // Chỉ có 6 quests
        {
            currentQuestId++;
            Debug.Log($"[QuestProgressManager] Quest completed! Moving to Quest {currentQuestId}");
            OnQuestChanged?.Invoke(currentQuestId);
        }
        else
        {
            Debug.Log("[QuestProgressManager] All quests completed!");
        }
    }

    /// <summary>
    /// Đặt quest hiện tại (chỉ set, không trigger event)
    /// </summary>
    public void SetCurrentQuestId(int questId)
    {
        if (questId >= 1 && questId <= 6)
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
    /// Kiểm tra quest có unlocked chưa
    /// </summary>
    public bool IsQuestUnlocked(int questId)
    {
        return questId <= currentQuestId;
    }

    /// <summary>
    /// Kiểm tra quest có đang active không
    /// </summary>
    public bool IsQuestActive(int questId)
    {
        return questId == currentQuestId;
    }
}
