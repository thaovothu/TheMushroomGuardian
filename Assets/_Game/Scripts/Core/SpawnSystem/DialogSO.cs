using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogLine
{
    [TextArea(2, 4)]
    public string text;
    [Range(0.5f, 5f)]
    public float displayDuration = 2f; // Thời gian hiển thị
}

[CreateAssetMenu(fileName = "Dialog", menuName = "Dialog/Create Dialog", order = 0)]
public class DialogSO : ScriptableObject
{
    public string dialogName = "New Dialog";
    public List<DialogLine> lines = new List<DialogLine>();
    
    [Header("Settings")]
    [Range(0.1f, 2f)]
    public float textDisplaySpeed = 0.05f; // Tốc độ hiển thị từng ký tự
    [Range(0.5f, 3f)]
    public float delayBetweenLines = 0.5f; // Delay giữa các dòng

    public int GetLineCount() => lines.Count;
    
    public DialogLine GetLine(int index)
    {
        if (index >= 0 && index < lines.Count)
            return lines[index];
        return null;
    }
}
