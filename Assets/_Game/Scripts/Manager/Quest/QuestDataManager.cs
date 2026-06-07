using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Quản lý dữ liệu quest toàn cục - Singleton
/// Cung cấp API để lấy quest data cho UI, AI, ...
/// </summary>
public class QuestDataManager : BaseSingleton<QuestDataManager>
{
    [SerializeField] private bool dontDestroyOnLoad = true;

    private List<QuestData> questDataList = new List<QuestData>();
    private Dictionary<int, List<QuestData>> questStepMap = new Dictionary<int, List<QuestData>>();

    /// <summary>
    /// Set dữ liệu quest từ LoadResource
    /// </summary>
    public void SetQuestData(List<QuestData> data)
    {
        questDataList = data ?? new List<QuestData>();
        RebuildQuestMap();
        GameEvent.Quest.OnDataLoaded?.Invoke(questDataList);
        Debug.Log($"[QuestDataManager] Quest data set: {questDataList.Count} entries");
    }

    /// <summary>
    /// Rebuild map để lookup nhanh hơn
    /// </summary>
    private void RebuildQuestMap()
    {
        questStepMap.Clear();

        foreach (var quest in questDataList)
        {
            if (!questStepMap.ContainsKey(quest.questId))
            {
                questStepMap[quest.questId] = new List<QuestData>();
            }
            questStepMap[quest.questId].Add(quest);
        }

        // Sort mỗi quest theo stepId
        foreach (var questSteps in questStepMap.Values)
        {
            questSteps.Sort((a, b) => a.stepId.CompareTo(b.stepId));
        }
    }

    /// <summary>
    /// Lấy tất cả quest data
    /// </summary>
    public List<QuestData> GetAllQuests()
    {
        return new List<QuestData>(questDataList);
    }

    /// <summary>
    /// Lấy tất cả steps của một quest bằng questId
    /// </summary>
    public List<QuestData> GetQuestSteps(int questId)
    {
        if (questStepMap.ContainsKey(questId))
        {
            return new List<QuestData>(questStepMap[questId]);
        }

        return new List<QuestData>();
    }

    /// <summary>
    /// Lấy một quest step cụ thể
    /// </summary>
    public QuestData GetQuestStep(int questId, int stepId)
    {
        if (questStepMap.ContainsKey(questId))
        {
            var step = questStepMap[questId].FirstOrDefault(q => q.stepId == stepId);
            return step;
        }

        return null;
    }

    /// <summary>
    /// Lấy quest info để hiển thị trên UI
    /// </summary>
    public string GetQuestInfo(int questId, int stepId)
    {
        var questData = GetQuestStep(questId, stepId);
        return questData != null ? questData.infoQuest : "";
    }

    /// <summary>
    /// Lấy quest title
    /// </summary>
    public string GetQuestTitle(int questId)
    {
        var questSteps = GetQuestSteps(questId);
        if (questSteps.Count > 0)
        {
            return questSteps[0].titleQuest;
        }

        return "";
    }

    /// <summary>
    /// Lấy reward của một quest step
    /// </summary>
    public (string itemReward, string skillReward, string extendReward) GetQuestReward(int questId, int stepId)
    {
        var questData = GetQuestStep(questId, stepId);
        if (questData != null)
            return (questData.itemReward1, questData.skillReward, questData.reward);

        return ("", "", "");
    }

    /// <summary>
    /// Kiểm tra quest data đã load chưa
    /// </summary>
    public bool IsDataLoaded => questDataList.Count > 0;

    /// <summary>
    /// Debug: In tất cả quests
    /// </summary>
    public void DebugPrintAllQuests()
    {
        Debug.Log($"[QuestDataManager] Total quests: {questDataList.Count}");
        foreach (var quest in questDataList)
        {
            Debug.Log($"  {quest}");
        }
    }

    /// <summary>
    /// Lấy số step tối đa của một quest
    /// </summary>
    public int GetMaxStepId(int questId)
    {
        if (questStepMap.ContainsKey(questId) && questStepMap[questId].Count > 0)
            return questStepMap[questId].Max(q => q.stepId);

        return 1;
    }
}
