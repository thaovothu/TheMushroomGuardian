using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillIconEntry
{
    public int skillId;
    public Sprite icon;
}

/// <summary>
/// Lưu trữ icon cho 8 skill (4 nguyên tố × 2 chiêu đỡ/đánh).
/// Dùng skillId để lấy icon tương ứng.
/// SkillId mặc định: 1=EarthShield, 2=EarthSmash, 3=WindGuard, 4=WindBlade,
///                   5=WaterShield, 6=WaterBlast, 7=FireShield, 8=FireBurst
/// </summary>
[CreateAssetMenu(fileName = "SkillIconSO", menuName = "Skill/SkillIconSO")]
public class SkillIconSO : ScriptableObject
{
    [SerializeField] private List<SkillIconEntry> skillIcons = new();
    [SerializeField] private Sprite defaultIcon;

    public Sprite GetIcon(int skillId)
    {
        foreach (var entry in skillIcons)
        {
            if (entry.skillId == skillId && entry.icon != null)
                return entry.icon;
        }

        Debug.LogWarning($"[SkillIconSO] Icon không tìm thấy cho skill ID: {skillId}");
        return defaultIcon;
    }

    public bool HasIcon(int skillId)
    {
        foreach (var entry in skillIcons)
        {
            if (entry.skillId == skillId && entry.icon != null)
                return true;
        }
        return false;
    }
}
