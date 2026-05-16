using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống quản lý Skill toàn cầu - Singleton
/// Cung cấp interface để truy vấn & sử dụng skill
/// </summary>
public class SkillSystem : BaseSingleton<SkillSystem>
{
    [SerializeField] private SkillSO skillConfig;
    private Dictionary<ElementType, float> lastSkillCastTime = new();

    void Awake()
    {
        // Initialize cooldown tracker
        foreach (ElementType element in System.Enum.GetValues(typeof(ElementType)))
        {
            lastSkillCastTime[element] = -999f;
        }

        if (skillConfig == null)
        {
            Debug.LogError("[SkillSystem] SkillSO not assigned!");
            return;
        }

        // Auto-initialize default skills if none exist
        if (skillConfig.GetSkillCount() == 0)
        {
            Debug.Log("[SkillSystem] No skills found, initializing default skills...");
            InitializeDefaultSkills();
        }
    }

    /// <summary>
    /// Khởi tạo các skill mặc định cho tất cả nguyên tố
    /// </summary>
    private void InitializeDefaultSkills()
    {
        if (skillConfig == null) return;

        int skillId = 1;

        // ===== EARTH (Đất) =====
        skillConfig.AddSkill(new SkillData(skillId++, "Earth Shield", ElementType.Earth, SkillRange.Melee, 0, 20, 15));
        skillConfig.AddSkill(new SkillData(skillId++, "Earth Smash", ElementType.Earth, SkillRange.Melee, 30, 0, 20));

        // ===== WIND (Khí) =====
        skillConfig.AddSkill(new SkillData(skillId++, "Wind Guard", ElementType.Wind, SkillRange.Ranged, 0, 15, 12));
        skillConfig.AddSkill(new SkillData(skillId++, "Wind Blade", ElementType.Wind, SkillRange.Ranged, 25, 0, 18));

        // ===== WATER (Nước) =====
        skillConfig.AddSkill(new SkillData(skillId++, "Water Shield", ElementType.Water, SkillRange.Ranged, 0, 18, 14));
        skillConfig.AddSkill(new SkillData(skillId++, "Water Blast", ElementType.Water, SkillRange.Ranged, 28, 0, 19));

        // ===== FIRE (Lửa) =====
        skillConfig.AddSkill(new SkillData(skillId++, "Fire Shield", ElementType.Fire, SkillRange.Melee, 0, 16, 16));
        skillConfig.AddSkill(new SkillData(skillId++, "Fire Burst", ElementType.Fire, SkillRange.Ranged, 35, 0, 22));

        Debug.Log($"[SkillSystem] ✓ Initialized {skillId - 1} default skills");
    }

    /// <summary>
    /// Lấy SkillSO config
    /// </summary>
    public SkillSO GetSkillConfig() => skillConfig;

    /// <summary>
    /// Kiểm tra có thể cast skill không (cooldown, mana, ...)
    /// </summary>
    public bool CanCastSkill(int skillId, int currentMana, float cooldown = 1f)
    {
        if (skillConfig == null) return false;

        var skill = skillConfig.GetSkill(skillId);
        if (skill == null) return false;

        // Kiểm tra mana
        if (currentMana < skill.manaCost)
            return false;

        // Kiểm tra cooldown
        if (Time.time - lastSkillCastTime[skill.element] < cooldown)
            return false;

        return true;
    }

    /// <summary>
    /// Ghi nhận thời điểm cast skill (cho cooldown)
    /// </summary>
    public void RecordSkillCast(int skillId)
    {
        var skill = skillConfig.GetSkill(skillId);
        if (skill != null)
        {
            lastSkillCastTime[skill.element] = Time.time;
        }
    }

    /// <summary>
    /// Lấy kỹ năng Skill1 (Shield) của nguyên tố
    /// </summary>
    public SkillData GetElementShield(ElementType element)
    {
        if (skillConfig == null) return null;
        var skills = skillConfig.GetSkillsByElement(element);
        return skills.Count > 0 ? skills[0] : null;
    }

    /// <summary>
    /// Lấy kỹ năng Skill2 (Attack) của nguyên tố
    /// </summary>
    public SkillData GetElementAttack(ElementType element)
    {
        if (skillConfig == null) return null;
        var skills = skillConfig.GetSkillsByElement(element);
        return skills.Count > 1 ? skills[1] : null;
    }

    /// <summary>
    /// Lấy damage multiplier dựa vào nguyên tố
    /// Dùng để tính damage cuối cùng khi có elemental advantage
    /// </summary>
    public float GetElementalMultiplier(ElementType attacker, ElementType defender)
    {
        return ElementalSystem.GetMultiplier(attacker, defender);
    }

    /// <summary>
    /// Lấy tất cả kỹ năng của một nguyên tố
    /// </summary>
    public List<SkillData> GetElementSkills(ElementType element)
    {
        if (skillConfig == null) return new List<SkillData>();
        return skillConfig.GetSkillsByElement(element);
    }
}
