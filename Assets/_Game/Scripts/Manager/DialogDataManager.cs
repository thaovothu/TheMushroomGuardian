using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Quản lý dữ liệu dialog toàn cục - Singleton
/// Cung cấp API để lấy dialog data cho UI, ...
/// Cấu trúc: NPCID -> List<DialogStep>
/// </summary>
public class DialogDataManager : BaseSingleton<DialogDataManager>
{
    [SerializeField] private bool dontDestroyOnLoad = true;

    private List<DialogData> dialogDataList = new List<DialogData>();
    private Dictionary<int, List<DialogData>> npcDialogMap = new Dictionary<int, List<DialogData>>();

    public event System.Action<List<DialogData>> OnDialogDataLoaded;

    protected override void Awake()
    {
        base.Awake();
        
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
    }

    /// <summary>
    /// Set dữ liệu dialog từ LoadResource
    /// </summary>
    public void SetDialogData(List<DialogData> data)
    {
        dialogDataList = data ?? new List<DialogData>();
        RebuildDialogMap();
        OnDialogDataLoaded?.Invoke(dialogDataList);
        Debug.Log($"[DialogDataManager] Dialog data set: {dialogDataList.Count} entries");
    }

    /// <summary>
    /// Rebuild map để lookup nhanh hơn theo npcId
    /// </summary>
    private void RebuildDialogMap()
    {
        npcDialogMap.Clear();

        foreach (var dialog in dialogDataList)
        {
            if (!npcDialogMap.ContainsKey(dialog.npcId))
            {
                npcDialogMap[dialog.npcId] = new List<DialogData>();
            }
            npcDialogMap[dialog.npcId].Add(dialog);
        }

        // Sort mỗi NPC's dialogs theo dialogStep
        foreach (var npcDialogs in npcDialogMap.Values)
        {
            npcDialogs.Sort((a, b) => a.dialogStep.CompareTo(b.dialogStep));
        }
    }

    /// <summary>
    /// Lấy tất cả dialog steps của một NPC
    /// </summary>
    public List<DialogData> GetDialogSteps(int npcId)
    {
        if (npcDialogMap.ContainsKey(npcId))
        {
            return new List<DialogData>(npcDialogMap[npcId]);
        }

        Debug.LogWarning($"[DialogDataManager] NPC with ID {npcId} not found");
        return new List<DialogData>();
    }

    /// <summary>
    /// Lấy một dialog step cụ thể của NPC
    /// </summary>
    public DialogData GetDialogStep(int npcId, int step)
    {
        if (npcDialogMap.ContainsKey(npcId))
        {
            var dialog = npcDialogMap[npcId].FirstOrDefault(d => d.dialogStep == step);
            if (dialog != null)
                return dialog;
        }

        Debug.LogWarning($"[DialogDataManager] Dialog not found for NPC {npcId} step {step}");
        return null;
    }

    /// <summary>
    /// Lấy tất cả dialog data
    /// </summary>
    public List<DialogData> GetAllDialogs()
    {
        return new List<DialogData>(dialogDataList);
    }

    /// <summary>
    /// Kiểm tra dialog data đã load chưa
    /// </summary>
    public bool IsDataLoaded => dialogDataList.Count > 0;

    /// <summary>
    /// Debug: In tất cả dialogs
    /// </summary>
    public void DebugPrintAllDialogs()
    {
        Debug.Log($"[DialogDataManager] Total dialogs: {dialogDataList.Count}");
        foreach (var npc in npcDialogMap.Keys)
        {
            Debug.Log($"  NPC {npc}: {npcDialogMap[npc].Count} steps");
            foreach (var dialog in npcDialogMap[npc])
            {
                Debug.Log($"    {dialog}");
            }
        }
    }
}
