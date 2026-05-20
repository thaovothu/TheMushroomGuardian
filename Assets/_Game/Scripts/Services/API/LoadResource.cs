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
        
        // Bước 1: Load quest.tsv (80%)
        yield return StartCoroutine(LoadQuestDataAsync());
        
        if (!isLoading)
        {
            yield break;
        }

        OnProgressUpdate?.Invoke(0.8f);
        
        // Bước 2: Khác (20%) - có thể mở rộng sau
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
                    mapId = columns.Length > 8 ? int.Parse(columns[8]) : 1
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

    public override string ToString()
    {
        return $"Quest {questId}-{stepId}: {titleQuest}";
    }
}
