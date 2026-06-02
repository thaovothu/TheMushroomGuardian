using System.Collections.Generic;
using UnityEngine;

public enum ElementType
{
    None = 0,
    Earth = 1,   // Đất
    Wind = 2,    // Khí
    Water = 3,   // Nước
    Fire = 4     // Lửa
}

public enum SkillRange
{
    Melee = 0,   // Gần
    Ranged = 1   // Xa
}

[System.Serializable]
public class SkillData
{
    [SerializeField] public int skillId;
    [SerializeField] public string skillName;
    [SerializeField] public ElementType element;
    [SerializeField] public SkillRange range;
    [SerializeField] public int damage;
    [SerializeField] public int defense;      // Giá trị bảo vệ/chống
    [SerializeField] public int manaCost;
    [Tooltip("Bán kính vùng gây damage (m). 0 = dùng mặc định trên PlayerSkillController (aoeRadius / waterRadius).")]
    [SerializeField] public float areaRadius;

    public SkillData(int skillId, string skillName, ElementType element, SkillRange range, int damage, int defense, int manaCost)
    {
        this.skillId = skillId;
        this.skillName = skillName;
        this.element = element;
        this.range = range;
        this.damage = damage;
        this.defense = defense;
        this.manaCost = manaCost;
    }
}

/// <summary>
/// ScriptableObject chứa cấu hình tất cả kỹ năng
/// </summary>
[CreateAssetMenu(fileName = "SkillConfig", menuName = "EntityData/Skill")]
public class SkillSO : ScriptableObject
{
    [SerializeField] private List<SkillData> skills = new();

    /// <summary>
    /// Lấy thông tin kỹ năng theo ID
    /// </summary>
    public SkillData GetSkill(int skillId)
    {
        if (skills == null) return null;
        
        foreach (var skill in skills)
        {
            if (skill != null && skill.skillId == skillId)
                return skill;
        }
        return null;
    }

    /// <summary>
    /// Lấy thông tin kỹ năng theo tên
    /// </summary>
    public SkillData GetSkillByName(string skillName)
    {
        if (string.IsNullOrEmpty(skillName) || skills == null) return null;
        
        foreach (var skill in skills)
        {
            Debug.Log($"Checking skill: {skill?.skillName} against {skillName}");
            if (skill != null && skill.skillName == skillName)
                return skill;
        }
        return null;
    }

    /// <summary>
    /// Lấy tất cả kỹ năng của một yếu tố
    /// </summary>
    public List<SkillData> GetSkillsByElement(ElementType element)
    {
        var result = new List<SkillData>();
        if (skills == null) return result;
        
        foreach (var skill in skills)
        {
            if (skill != null && skill.element == element)
                result.Add(skill);
        }
        return result;
    }

    /// <summary>
    /// Lấy kỹ năng theo yếu tố và index
    /// </summary>
    public SkillData GetSkillByElementAndIndex(ElementType element, int index)
    {
        var elementSkills = GetSkillsByElement(element);

        if (index >= 0 && index < elementSkills.Count)
        {
            return elementSkills[index];
        }

        //Debug.LogWarning($"SkillSO '{name}': Skill index {index} not found for element {element}!");
        return null;
    }

    /// <summary>
    /// Lấy damage của kỹ năng
    /// </summary>
    public int GetDamage(int skillId)
    {
        var skill = GetSkill(skillId);
        return skill?.damage ?? 0;
    }

    /// <summary>
    /// Lấy defense/chống của kỹ năng
    /// </summary>
    public int GetDefense(int skillId)
    {
        var skill = GetSkill(skillId);
        return skill?.defense ?? 0;
    }

    /// <summary>
    /// Lấy mana cost của kỹ năng
    /// </summary>
    public int GetManaCost(int skillId)
    {
        var skill = GetSkill(skillId);
        return skill?.manaCost ?? 0;
    }

    /// <summary>
    /// Lấy tầm đánh của kỹ năng
    /// </summary>
    public SkillRange GetSkillRange(int skillId)
    {
        var skill = GetSkill(skillId);
        return skill?.range ?? SkillRange.Melee;
    }

    /// <summary>
    /// Lấy yếu tố của kỹ năng
    /// </summary>
    public ElementType GetElement(int skillId)
    {
        var skill = GetSkill(skillId);
        return skill?.element ?? ElementType.None;
    }

    /// <summary>
    /// Kiểm tra có đủ mana để dùng kỹ năng không
    /// </summary>
    public bool CanUseSkill(int skillId, int currentMana)
    {
        int manaCost = GetManaCost(skillId);
        return currentMana >= manaCost;
    }

    /// <summary>
    /// Lấy tất cả kỹ năng
    /// </summary>
    public List<SkillData> GetAllSkills()
    {
        return skills != null ? new List<SkillData>(skills) : new List<SkillData>();
    }

    /// <summary>
    /// Kiểm tra kỹ năng tồn tại
    /// </summary>
    public bool SkillExists(int skillId)
    {
        return GetSkill(skillId) != null;
    }

    /// <summary>
    /// Lấy số lượng kỹ năng
    /// </summary>
    public int GetSkillCount()
    {
        return skills != null ? skills.Count : 0;
    }

    /// <summary>
    /// Lấy số lượng kỹ năng theo yếu tố
    /// </summary>
    public int GetSkillCountByElement(ElementType element)
    {
        return GetSkillsByElement(element).Count;
    }

    /// <summary>
    /// Thêm skill vào danh sách (dùng cho runtime initialization)
    /// </summary>
    public void AddSkill(SkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogWarning("[SkillSO] Cannot add null skill!");
            return;
        }

        // Ensure skills list exists
        if (skills == null)
        {
            skills = new List<SkillData>();
        }

        skills.Add(skillData);
        Debug.Log($"[SkillSO] Added skill: {skillData.skillName} (ID: {skillData.skillId}, Element: {skillData.element})");
    }
}