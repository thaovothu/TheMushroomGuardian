// using UnityEngine;

// /// <summary>
// /// Example script - Hiển thị cách sử dụng QuestDataManager
// /// Gắn script này vào GameObject bất kì để test
// /// </summary>
// public class QuestDataExample : MonoBehaviour
// {
//     private void Start()
//     {
//         // Chờ cho QuestDataManager load dữ liệu xong
//         if (QuestDataManager.Instance != null)
//         {
//             QuestDataManager.Instance.OnQuestDataLoaded += OnQuestDataLoaded;
//         }
//     }

//     private void OnQuestDataLoaded(System.Collections.Generic.List<QuestData> questList)
//     {
//         Debug.Log($"=== Quest Data Loaded: {questList.Count} quests ===");

//         // Example 1: Lấy quest title
//         string title = QuestDataManager.Instance.GetQuestTitle(questId: 1);
//         Debug.Log($"Quest 1 Title: {title}");

//         // Example 2: Lấy quest info chi tiết
//         string info = QuestDataManager.Instance.GetQuestInfo(questId: 1, stepId: 1);
//         Debug.Log($"Quest 1 Step 1 Info: {info}");

//         // Example 3: Lấy tất cả steps của quest 1
//         var steps = QuestDataManager.Instance.GetQuestSteps(questId: 1);
//         Debug.Log($"Quest 1 has {steps.Count} steps:");
//         foreach (var step in steps)
//         {
//             Debug.Log($"  Step {step.stepId}: {step.shortDescription}");
//         }

//         // Example 4: Lấy reward
//         var (coin, item, reward) = QuestDataManager.Instance.GetQuestReward(questId: 1, stepId: 3);
//         Debug.Log($"Quest 1 Step 3 Reward - Coin: {coin}, Item: {item}, Reward: {reward}");

//         // Example 5: Lấy tất cả quests
//         var allQuests = QuestDataManager.Instance.GetAllQuests();
//         Debug.Log($"Total quests in database: {allQuests.Count}");

//         // Debug: Print tất cả
//         QuestDataManager.Instance.DebugPrintAllQuests();
//     }

//     private void OnDestroy()
//     {
//         if (QuestDataManager.Instance != null)
//         {
//             QuestDataManager.Instance.OnQuestDataLoaded -= OnQuestDataLoaded;
//         }
//     }
// }
