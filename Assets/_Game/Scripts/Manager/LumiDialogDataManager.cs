using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Quản lý dialog data của Lumi — keyed theo questId thay vì npcId.
/// Tự load lumi_dialog.tsv trong Awake, không phụ thuộc vào LoadResource.
/// </summary>
public class LumiDialogDataManager : BaseSingleton<LumiDialogDataManager>
{
    [SerializeField] private bool dontDestroyOnLoad = true;

    private readonly Dictionary<int, List<LumiDialogData>> questDialogMap = new Dictionary<int, List<LumiDialogData>>();

    protected override void Awake()
    {
        base.Awake();
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(transform.root.gameObject);

        GameEvent.Auth.OnLoginSuccess += OnLoginReady;
    }

    private void OnLoginReady(string _)
    {
        GameEvent.Auth.OnLoginSuccess -= OnLoginReady;
        LoadFromFile();
    }

    private void LoadFromFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "lumi_dialog.tsv");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[LumiDialogDataManager] File not found: {path}");
            return;
        }

        try
        {
            string content = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                var cols = lines[i].Split('\t');
                if (cols.Length < 3) continue;

                try
                {
                    var entry = new LumiDialogData
                    {
                        questId = int.Parse(cols[0]),
                        stepId = int.Parse(cols[1]),
                        text = cols[2],
                        displayDuration = cols.Length > 3 ? float.Parse(cols[3]) : 2f,
                        playerReply = cols.Length > 4 ? cols[4].Trim() : ""
                    };

                    if (!questDialogMap.ContainsKey(entry.questId))
                        questDialogMap[entry.questId] = new List<LumiDialogData>();

                    questDialogMap[entry.questId].Add(entry);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[LumiDialogDataManager] Parse error line {i}: {ex.Message}");
                }
            }

            foreach (var steps in questDialogMap.Values)
                steps.Sort((a, b) => a.stepId.CompareTo(b.stepId));

            Debug.Log($"[LumiDialogDataManager] Loaded dialogs for quests: [{string.Join(", ", questDialogMap.Keys)}]");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LumiDialogDataManager] Failed to load file: {ex.Message}");
        }
    }

    public List<LumiDialogData> GetDialogSteps(int questId)
    {
        if (questDialogMap.TryGetValue(questId, out var steps))
            return new List<LumiDialogData>(steps);

        return new List<LumiDialogData>();
    }

    public bool HasDialog(int questId) => questDialogMap.ContainsKey(questId) && questDialogMap[questId].Count > 0;

    public LumiDialogData GetDialogStep(int questId, int stepId)
    {
        if (!questDialogMap.TryGetValue(questId, out var steps)) return null;
        return steps.Find(s => s.stepId == stepId);
    }
}
