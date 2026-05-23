// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Text;
// using UnityEngine;

// /// <summary>
// /// Xử lý việc load các file resource (quest.tsv, ...) từ StreamingAssets
// /// </summary>
// public class LoadResource : BaseSingleton<LoadResource>
// {
//     public delegate void OnProgressUpdateDelegate(float progress);
//     public delegate void OnLoadCompleteDelegate();

//     public event OnProgressUpdateDelegate OnProgressUpdate;
//     public event OnLoadCompleteDelegate OnLoadComplete;

//     private bool isLoading = false;

//     /// <summary>
//     /// Load tất cả resources cần thiết (quest.tsv, ...)
//     /// </summary>
//     public void LoadAllResources()
//     {
//         if (isLoading) return;
//         StartCoroutine(LoadAllResourcesAsync());
//     }

//     private IEnumerator LoadAllResourcesAsync()
//     {
//         isLoading = true;

//         // Bước 1: Load quest.tsv (40%)
//         yield return StartCoroutine(LoadQuestDataAsync());

//         if (!isLoading)
//         {
//             yield break;
//         }

//         OnProgressUpdate?.Invoke(0.4f);

//         // Bước 2: Load dialog.tsv (40%)
//         yield return StartCoroutine(LoadDialogDataAsync());

//         if (!isLoading)
//         {
//             yield break;
//         }

//         OnProgressUpdate?.Invoke(0.8f);

//         // Bước 3: Khác (20%) - có thể mở rộng sau
//         yield return new WaitForSeconds(0.1f);
//         OnProgressUpdate?.Invoke(1f);

//         // Hoàn thành
//         try
//         {
//             OnLoadComplete?.Invoke();
//             Debug.Log("[LoadResource] All resources loaded successfully!");
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"[LoadResource] Error in OnLoadComplete callback: {ex.Message}");
//         }
//         finally
//         {
//             isLoading = false;
//         }
//     }

//     /// <summary>
//     /// Load file quest.tsv từ StreamingAssets
//     /// </summary>
//     private IEnumerator LoadQuestDataAsync()
//     {
//         string questFilePath = Path.Combine(Application.streamingAssetsPath, "quest.tsv");

//         Debug.Log($"[LoadResource] Loading quest data from: {questFilePath}");

//         // Kiểm tra file tồn tại
//         if (!File.Exists(questFilePath))
//         {
//             Debug.LogError($"[LoadResource] Quest file not found at: {questFilePath}");
//             yield break;
//         }

//         // Load file - sử dụng async để không block game
//         yield return new WaitForEndOfFrame(); // Chờ frame kết thúc

//         byte[] fileData = null;
//         string fileContent = null;

//         try
//         {
//             // Đọc file
//             fileData = File.ReadAllBytes(questFilePath);
//             fileContent = Encoding.UTF8.GetString(fileData);

//             // Parse TSV data
//             var questDataList = ParseQuestTSV(fileContent);

//             // Lưu dữ liệu vào QuestDataManager
//             if (QuestDataManager.Instance != null)
//             {
//                 QuestDataManager.Instance.SetQuestData(questDataList);
//                 Debug.Log($"[LoadResource] Loaded {questDataList.Count} quests");
//             }
//             else
//             {
//                 Debug.LogWarning("[LoadResource] QuestDataManager not found!");
//             }

//             OnProgressUpdate?.Invoke(0.8f);
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"[LoadResource] Error reading quest file: {ex.Message}");
//         }

//         yield return null;
//     }

//     /// <summary>
//     /// Parse TSV file thành danh sách quest data
//     /// </summary>
//     private List<QuestData> ParseQuestTSV(string content)
//     {
//         var questList = new List<QuestData>();
//         var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

//         if (lines.Length < 2)
//         {
//             Debug.LogWarning("[LoadResource] TSV file is empty or malformed");
//             return questList;
//         }

//         // Bỏ qua header (dòng đầu)
//         for (int i = 1; i < lines.Length; i++)
//         {
//             if (string.IsNullOrWhiteSpace(lines[i]))
//                 continue;

//             var columns = lines[i].Split('\t');

//             // Yêu cầu tối thiểu các cột: QuestID, StepID, Title, Info, ...
//             if (columns.Length < 5)
//                 continue;

//             try
//             {
//                 var questData = new QuestData
//                 {
//                     questId = int.Parse(columns[0]),
//                     stepId = int.Parse(columns[1]),
//                     titleQuest = columns[2],
//                     infoQuest = columns[3],
//                     itemReward1 = columns[4],
//                     coinReward = columns.Length > 5 ? (string.IsNullOrEmpty(columns[5]) ? 0 : int.Parse(columns[5])) : 0,
//                     reward = columns.Length > 6 ? columns[6] : "",
//                     shortDescription = columns.Length > 7 ? columns[7] : "",
//                     mapId = columns.Length > 8 ? int.Parse(columns[8]) : 1
//                 };

//                 questList.Add(questData);
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogWarning($"[LoadResource] Error parsing quest line {i}: {ex.Message}");
//             }
//         }

//         return questList;
//     }

//     /// <summary>
//     /// Load file dialog.tsv từ StreamingAssets
//     /// </summary>
//     private IEnumerator LoadDialogDataAsync()
//     {
//         string dialogFilePath = Path.Combine(Application.streamingAssetsPath, "dialog.tsv");

//         Debug.Log($"[LoadResource] Loading dialog data from: {dialogFilePath}");

//         // Kiểm tra file tồn tại
//         if (!File.Exists(dialogFilePath))
//         {
//             Debug.LogError($"[LoadResource] Dialog file not found at: {dialogFilePath}");
//             yield break;
//         }

//         // Load file - sử dụng async để không block game
//         yield return new WaitForEndOfFrame(); // Chờ frame kết thúc

//         byte[] fileData = null;
//         string fileContent = null;

//         try
//         {
//             // Đọc file
//             fileData = File.ReadAllBytes(dialogFilePath);
//             fileContent = Encoding.UTF8.GetString(fileData);

//             // Parse TSV data
//             var dialogDataList = ParseDialogTSV(fileContent);

//             // Lưu dữ liệu vào DialogDataManager
//             if (DialogDataManager.Instance != null)
//             {
//                 DialogDataManager.Instance.SetDialogData(dialogDataList);
//                 Debug.Log($"[LoadResource] Loaded {dialogDataList.Count} dialogs");
//             }
//             else
//             {
//                 Debug.LogWarning("[LoadResource] DialogDataManager not found!");
//             }
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"[LoadResource] Error reading dialog file: {ex.Message}");
//         }

//         yield return null;
//     }

//     /// <summary>
//     /// Parse dialog TSV file thành danh sách dialog data
//     /// </summary>
//     private List<DialogData> ParseDialogTSV(string content)
//     {
//         var dialogList = new List<DialogData>();
//         var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

//         if (lines.Length < 2)
//         {
//             Debug.LogWarning("[LoadResource] Dialog TSV file is empty or malformed");
//             return dialogList;
//         }

//         // Bỏ qua header (dòng đầu)
//         for (int i = 1; i < lines.Length; i++)
//         {
//             if (string.IsNullOrWhiteSpace(lines[i]))
//                 continue;

//             var columns = lines[i].Split('\t');

//             // Yêu cầu tối thiểu các cột: NPCID, DialogStep, Text, DisplayDuration
//             if (columns.Length < 3)
//                 continue;

//             try
//             {
//                 var dialogData = new DialogData
//                 {
//                     npcId = int.Parse(columns[0]),
//                     dialogStep = int.Parse(columns[1]),
//                     text = columns[2],
//                     displayDuration = columns.Length > 3 ? float.Parse(columns[3]) : 2f
//                 };

//                 dialogList.Add(dialogData);
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogWarning($"[LoadResource] Error parsing dialog line {i}: {ex.Message}");
//             }
//         }

//         return dialogList;
//     }

//     public bool IsLoading => isLoading;
// }

// /// <summary>
// /// Data class cho một quest step
// /// </summary>
// [System.Serializable]
// public class QuestData
// {
//     public int questId;
//     public int stepId;
//     public string titleQuest;
//     public string infoQuest;
//     public string itemReward1;
//     public int coinReward;
//     public string reward;
//     public string shortDescription;
//     public int mapId;

//     public override string ToString()
//     {
//         return $"Quest {questId}-{stepId}: {titleQuest}";
//     }
// }

// /// <summary>
// /// Data class cho một dialog
// /// </summary>
// [System.Serializable]
// public class DialogData
// {
//     public int npcId;
//     public int dialogStep;
//     public string text;
//     public float displayDuration;

//     public override string ToString()
//     {
//         return $"Dialog NPC{npcId}-Step{dialogStep}: {text}";
//     }
// }

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Xử lý việc load các file resource (quest.tsv, ...) từ StreamingAssets
/// </summary>
public class LoadResource : BaseSingleton<LoadResource>
{
    public delegate void OnProgressUpdateDelegate(float progress);
    public delegate void OnLoadCompleteDelegate();

    public event OnProgressUpdateDelegate OnProgressUpdate;
    public event OnLoadCompleteDelegate OnLoadComplete;

    private bool isLoading = false;

    /// <summary>
    /// Load tất cả resources cần thiết (quest.tsv, ...)
    /// </summary>
    public void LoadAllResources()
    {
        if (isLoading) return;
        StartCoroutine(LoadAllResourcesAsync());
    }

    private IEnumerator LoadAllResourcesAsync()
    {
        isLoading = true;

        // Bước 1: Load quest.tsv (40%)
        yield return StartCoroutine(LoadQuestDataAsync());

        if (!isLoading)
        {
            yield break;
        }

        OnProgressUpdate?.Invoke(0.4f);

        // Bước 2: Load dialog.tsv (40%)
        yield return StartCoroutine(LoadDialogDataAsync());

        if (!isLoading)
        {
            yield break;
        }

        OnProgressUpdate?.Invoke(0.8f);

        // Bước 3: Khác (20%) - có thể mở rộng sau
        yield return new WaitForSeconds(0.1f);
        OnProgressUpdate?.Invoke(1f);

        // Hoàn thành
        try
        {
            OnLoadComplete?.Invoke();
            Debug.Log("[LoadResource] All resources loaded successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LoadResource] Error in OnLoadComplete callback: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    /// <summary>
    /// Load file quest.tsv từ StreamingAssets
    /// </summary>
    private IEnumerator LoadQuestDataAsync()
    {
        string questFilePath = Path.Combine(Application.streamingAssetsPath, "quest.tsv");

        Debug.Log($"[LoadResource] Loading quest data from: {questFilePath}");

        // Kiểm tra file tồn tại
        if (!File.Exists(questFilePath))
        {
            Debug.LogError($"[LoadResource] Quest file not found at: {questFilePath}");
            yield break;
        }

        // Load file - sử dụng async để không block game
        yield return new WaitForEndOfFrame(); // Chờ frame kết thúc

        byte[] fileData = null;
        string fileContent = null;

        try
        {
            // Đọc file
            fileData = File.ReadAllBytes(questFilePath);
            fileContent = Encoding.UTF8.GetString(fileData);

            // Parse TSV data
            var questDataList = ParseQuestTSV(fileContent);

            // Lưu dữ liệu vào QuestDataManager
            if (QuestDataManager.Instance != null)
            {
                QuestDataManager.Instance.SetQuestData(questDataList);
                Debug.Log($"[LoadResource] Loaded {questDataList.Count} quests");
            }
            else
            {
                Debug.LogWarning("[LoadResource] QuestDataManager not found!");
            }

            OnProgressUpdate?.Invoke(0.8f);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LoadResource] Error reading quest file: {ex.Message}");
        }

        yield return null;
    }

    /// <summary>
    /// Parse TSV file thành danh sách quest data
    /// </summary>
    private List<QuestData> ParseQuestTSV(string content)
    {
        var questList = new List<QuestData>();
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length < 2)
        {
            Debug.LogWarning("[LoadResource] TSV file is empty or malformed");
            return questList;
        }

        // Bỏ qua header (dòng đầu)
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var columns = lines[i].Split('\t');

            // Yêu cầu tối thiểu các cột: QuestID, StepID, Title, Info, ...
            if (columns.Length < 5)
                continue;

            try
            {
                var questData = new QuestData
                {
                    questId = int.Parse(columns[0]),
                    stepId = int.Parse(columns[1]),
                    titleQuest = columns[2],
                    infoQuest = columns[3],
                    itemReward1 = columns[4],
                    coinReward = columns.Length > 5 ? (string.IsNullOrEmpty(columns[5]) ? 0 : int.Parse(columns[5])) : 0,
                    reward = columns.Length > 6 ? columns[6] : "",
                    shortDescription = columns.Length > 7 ? columns[7] : "",
                    mapId = columns.Length > 8 ? int.Parse(columns[8]) : 1,
                    spawnGroupId = columns.Length > 9 ? columns[9].Trim() : "",
                    lumiHint = columns.Length > 10 ? columns[10].Trim() : ""
                };

                questList.Add(questData);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LoadResource] Error parsing quest line {i}: {ex.Message}");
            }
        }

        return questList;
    }

    /// <summary>
    /// Load file dialog.tsv từ StreamingAssets
    /// </summary>
    private IEnumerator LoadDialogDataAsync()
    {
        string dialogFilePath = Path.Combine(Application.streamingAssetsPath, "dialog.tsv");

        Debug.Log($"[LoadResource] Loading dialog data from: {dialogFilePath}");

        // Kiểm tra file tồn tại
        if (!File.Exists(dialogFilePath))
        {
            Debug.LogError($"[LoadResource] Dialog file not found at: {dialogFilePath}");
            yield break;
        }

        // Load file - sử dụng async để không block game
        yield return new WaitForEndOfFrame(); // Chờ frame kết thúc

        byte[] fileData = null;
        string fileContent = null;

        try
        {
            // Đọc file
            fileData = File.ReadAllBytes(dialogFilePath);
            fileContent = Encoding.UTF8.GetString(fileData);

            // Parse TSV data
            var dialogDataList = ParseDialogTSV(fileContent);

            // Lưu dữ liệu vào DialogDataManager
            if (DialogDataManager.Instance != null)
            {
                DialogDataManager.Instance.SetDialogData(dialogDataList);
                Debug.Log($"[LoadResource] Loaded {dialogDataList.Count} dialogs");
            }
            else
            {
                Debug.LogWarning("[LoadResource] DialogDataManager not found!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LoadResource] Error reading dialog file: {ex.Message}");
        }

        yield return null;
    }

    /// <summary>
    /// Parse dialog TSV file thành danh sách dialog data
    /// </summary>
    private List<DialogData> ParseDialogTSV(string content)
    {
        var dialogList = new List<DialogData>();
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length < 2)
        {
            Debug.LogWarning("[LoadResource] Dialog TSV file is empty or malformed");
            return dialogList;
        }

        // Bỏ qua header (dòng đầu)
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            var columns = lines[i].Split('\t');

            // Yêu cầu tối thiểu các cột: NPCID, DialogStep, Text, DisplayDuration
            if (columns.Length < 3)
                continue;

            try
            {
                var dialogData = new DialogData
                {
                    npcId = int.Parse(columns[0]),
                    dialogStep = int.Parse(columns[1]),
                    text = columns[2],
                    displayDuration = columns.Length > 3 ? float.Parse(columns[3]) : 2f,
                    playerReply = columns.Length > 4 ? columns[4].Trim() : "" // ← thêm dòng này
                };

                dialogList.Add(dialogData);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LoadResource] Error parsing dialog line {i}: {ex.Message}");
            }
        }

        return dialogList;
    }

    public bool IsLoading => isLoading;
}

/// <summary>
/// Data class cho một quest step
/// </summary>
[System.Serializable]
public class QuestData
{
    public int questId;
    public int stepId;
    public string titleQuest;
    public string infoQuest;
    public string itemReward1;
    public int coinReward;
    public string reward;
    public string shortDescription;
    public int mapId;
    public string lumiHint;
    /// <summary>
    /// ID nhóm spawn enemy cho step này.
    /// Rỗng = step không spawn enemy (waypoint, dialog, collect...).
    /// Phải khớp với spawnGroupId trong QuestSpawnConfig asset.
    /// VD: "spawn_q1s2_cactus", "spawn_q2s2_mushroom"
    /// </summary>
    public string spawnGroupId;

    public override string ToString()
    {
        return $"Quest {questId}-{stepId}: {titleQuest}";
    }
}

/// <summary>
/// Data class cho một dialog
/// </summary>
[System.Serializable]
public class DialogData
{
    public int npcId;
    public int dialogStep;
    public string text;
    public float displayDuration;
    public string playerReply; // Câu trả lời của player — hiện trong button Next, trống = không hiện

    public override string ToString()
    {
        return $"Dialog NPC{npcId}-Step{dialogStep}: {text}";
    }
}