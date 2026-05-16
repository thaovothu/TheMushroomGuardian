using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor script để khởi tạo SkillSO với dữ liệu skill mặc định cho tất cả nguyên tố
/// Chạy: Assets > Initialize Skill System
/// </summary>
public class InitializeSkillSystem
{
    private const string SkillSOPath = "Assets/_Game/ScriptableObjects/Skill/SkillSO.asset";

    [MenuItem("Assets/Initialize Skill System")]
    public static void InitializeSkills()
    {
        // Load or create SkillSO
        SkillSO skillSO = AssetDatabase.LoadAssetAtPath<SkillSO>(SkillSOPath);

        if (skillSO == null)
        {
            Debug.LogError($"[InitializeSkillSystem] SkillSO not found at {SkillSOPath}");
            return;
        }

        // Clear existing skills
        var allSkills = skillSO.GetAllSkills();
        foreach (var skill in allSkills)
        {
            // We can't directly remove from serialized list, so we'll work with the asset
        }

        // Clear via reflection as the list is private
        var skillsField = typeof(SkillSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (skillsField != null)
        {
            var skillsList = (List<SkillData>)skillsField.GetValue(skillSO);
            skillsList?.Clear();
        }

        // Add default skills for each element
        int skillId = 1;
        
        // ===== EARTH (Đất) =====
        skillSO.AddSkill(new SkillData(skillId++, "Earth Shield", ElementType.Earth, SkillRange.Melee, 0, 20, 15)); // Shield: 0 damage, 20 defense
        skillSO.AddSkill(new SkillData(skillId++, "Earth Smash", ElementType.Earth, SkillRange.Melee, 30, 0, 20));  // Attack: 30 damage
        
        // ===== WIND (Khí) =====
        skillSO.AddSkill(new SkillData(skillId++, "Wind Guard", ElementType.Wind, SkillRange.Ranged, 0, 15, 12));   // Shield: 0 damage, 15 defense
        skillSO.AddSkill(new SkillData(skillId++, "Wind Blade", ElementType.Wind, SkillRange.Ranged, 25, 0, 18));   // Attack: 25 damage
        
        // ===== WATER (Nước) =====
        skillSO.AddSkill(new SkillData(skillId++, "Water Shield", ElementType.Water, SkillRange.Ranged, 0, 18, 14)); // Shield: 0 damage, 18 defense
        skillSO.AddSkill(new SkillData(skillId++, "Water Blast", ElementType.Water, SkillRange.Ranged, 28, 0, 19));  // Attack: 28 damage
        
        // ===== FIRE (Lửa) =====
        skillSO.AddSkill(new SkillData(skillId++, "Fire Shield", ElementType.Fire, SkillRange.Melee, 0, 16, 16));    // Shield: 0 damage, 16 defense
        skillSO.AddSkill(new SkillData(skillId++, "Fire Burst", ElementType.Fire, SkillRange.Ranged, 35, 0, 22));    // Attack: 35 damage

        // Mark as dirty and save
        EditorUtility.SetDirty(skillSO);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[InitializeSkillSystem] ✓ Successfully initialized SkillSO with {skillId - 1} skills!");
    }
}
