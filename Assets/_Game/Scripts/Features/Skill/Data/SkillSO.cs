using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum cho loại yếu tố
/// </summary>
public enum ElementType
{
    None = 0,
    Earth = 1,   // Đất
    Wind = 2,    // Khí
    Water = 3,   // Nước
    Fire = 4     // Lửa
}

/// <summary>
/// Enum cho tầm đánh
/// </summary>
public enum SkillRange
{
    Melee = 0,   // Gần
    Ranged = 1   // Xa
}

/// <summary>
/// Dữ liệu của từng kỹ năng
/// </summary>
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

    private Dictionary<int, SkillData> skillLookup;
    private Dictionary<string, SkillData> skillNameLookup;
    private Dictionary<ElementType, List<SkillData>> skillsByElement;
    private bool isInitialized = false;

    private void Init()
    {
        if (isInitialized) return;

        skillLookup = new Dictionary<int, SkillData>();
        skillNameLookup = new Dictionary<string, SkillData>();
        skillsByElement = new Dictionary<ElementType, List<SkillData>>();

        // Initialize element lists
        foreach (ElementType element in System.Enum.GetValues(typeof(ElementType)))
        {
            skillsByElement[element] = new List<SkillData>();
        }

        if (skills == null || skills.Count == 0)
        {
            //Debug.LogWarning($"SkillSO '{name}': No skill data configured!");
            return;
        }

        foreach (var skill in skills)
        {
            if (skill == null)
            {
                //Debug.LogWarning($"SkillSO '{name}': Null skill data found, skipping.");
                continue;
            }

            // Kiểm tra duplicate ID
            if (skillLookup.ContainsKey(skill.skillId))
            {
                //Debug.LogWarning($"SkillSO '{name}': Duplicate skill ID '{skill.skillId}', skipping.");
                continue;
            }

            // Kiểm tra duplicate name
            if (!string.IsNullOrEmpty(skill.skillName) && skillNameLookup.ContainsKey(skill.skillName))
            {
                //Debug.LogWarning($"SkillSO '{name}': Duplicate skill name '{skill.skillName}', skipping.");
                continue;
            }

            skillLookup.Add(skill.skillId, skill);

            if (!string.IsNullOrEmpty(skill.skillName))
            {
                skillNameLookup.Add(skill.skillName, skill);
            }

            // Add to element list
            if (skill.element != ElementType.None && skillsByElement.ContainsKey(skill.element))
            {
                skillsByElement[skill.element].Add(skill);
            }
        }

        isInitialized = true;
    }

    /// <summary>
    /// Lấy thông tin kỹ năng theo ID
    /// </summary>
    public SkillData GetSkill(int skillId)
    {
        Init();

        if (skillLookup.TryGetValue(skillId, out SkillData skill))
        {
            return skill;
        }

        //Debug.LogWarning($"SkillSO '{name}': Skill ID {skillId} not found!");
        return null;
    }

    /// <summary>
    /// Lấy thông tin kỹ năng theo tên
    /// </summary>
    public SkillData GetSkillByName(string skillName)
    {
        Init();

        if (skillNameLookup.TryGetValue(skillName, out SkillData skill))
        {
            return skill;
        }

        //Debug.LogWarning($"SkillSO '{name}': Skill '{skillName}' not found!");
        return null;
    }

    /// <summary>
    /// Lấy tất cả kỹ năng của một yếu tố
    /// </summary>
    public List<SkillData> GetSkillsByElement(ElementType element)
    {
        Init();

        if (skillsByElement.TryGetValue(element, out List<SkillData> elementSkills))
        {
            return new List<SkillData>(elementSkills);
        }

        return new List<SkillData>();
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
        Init();
        return new List<SkillData>(skills);
    }

    /// <summary>
    /// Kiểm tra kỹ năng tồn tại
    /// </summary>
    public bool SkillExists(int skillId)
    {
        Init();
        return skillLookup.ContainsKey(skillId);
    }

    /// <summary>
    /// Lấy số lượng kỹ năng
    /// </summary>
    public int GetSkillCount()
    {
        Init();
        return skillLookup.Count;
    }

    /// <summary>
    /// Lấy số lượng kỹ năng theo yếu tố
    /// </summary>
    public int GetSkillCountByElement(ElementType element)
    {
        return GetSkillsByElement(element).Count;
    }
}