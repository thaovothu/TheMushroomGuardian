using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unified Config: Map 8 Skills → 5 VFX theo hệ thống phần tử
/// 
/// DEFEND (gần, 1 VFX dùng chung):
/// - Skill 1: Defend Earth
/// - Skill 3: Defend Wind  
/// - Skill 5: Defend Water
/// - Skill 7: Defend Fire
/// 
/// ATTACK (xa, 4 VFX riêng từng hệ):
/// - Skill 2: Attack Earth
/// - Skill 4: Attack Wind
/// - Skill 6: Attack Water
/// - Skill 8: Attack Fire
/// 
/// Tổng: 5 VFX (1 Defend + 4 Attack)
/// </summary>
[CreateAssetMenu(fileName = "SkillProjectileConfig", menuName = "Skill/Projectile Config")]
public class SkillProjectileConfig : ScriptableObject
{
    [Header("DEFEND VFX - Dùng chung cho 4 hệ (gần)")]
    [SerializeField] private ParticleSystem defendVFX;
    
    [Header("ATTACK VFX - Riêng cho mỗi hệ (xa)")]
    [SerializeField] private ParticleSystem attackEarthVFX;
    [SerializeField] private ParticleSystem attackWindVFX;
    [SerializeField] private ParticleSystem attackWaterVFX;
    [SerializeField] private ParticleSystem attackFireVFX;

    [Header("IMPACT VFX - Khi hit enemy")]
    [SerializeField] private ParticleSystem impactEarthVFX;
    [SerializeField] private ParticleSystem impactWindVFX;
    [SerializeField] private ParticleSystem impactWaterVFX;
    [SerializeField] private ParticleSystem impactFireVFX;

    [Header("PROJECTILE PREFABS - Cho ranged attacks")]
    [SerializeField] private GameObject projectileEarthPrefab;
    [SerializeField] private GameObject projectileWindPrefab;
    [SerializeField] private GameObject projectileWaterPrefab;
    [SerializeField] private GameObject projectileFirePrefab;

    [System.Serializable]
    public class SkillMapping
    {
        public int skillId;
        public string skillName;
        public string element; // earth, wind, water, fire
        public bool isDefend;  // true = defend, false = attack
    }

    [SerializeField] private List<SkillMapping> skillMappings = new();

    private void OnEnable()
    {
        // Initialize skill mappings nếu chưa có
        if (skillMappings.Count == 0)
        {
            skillMappings = new List<SkillMapping>
            {
                new SkillMapping { skillId = 1, skillName = "Defend Earth", element = "earth", isDefend = true },
                new SkillMapping { skillId = 2, skillName = "Attack Earth", element = "earth", isDefend = false },
                new SkillMapping { skillId = 3, skillName = "Defend Wind", element = "wind", isDefend = true },
                new SkillMapping { skillId = 4, skillName = "Attack Wind", element = "wind", isDefend = false },
                new SkillMapping { skillId = 5, skillName = "Defend Water", element = "water", isDefend = true },
                new SkillMapping { skillId = 6, skillName = "Attack Water", element = "water", isDefend = false },
                new SkillMapping { skillId = 7, skillName = "Defend Fire", element = "fire", isDefend = true },
                new SkillMapping { skillId = 8, skillName = "Attack Fire", element = "fire", isDefend = false },
            };
        }
    }

    public ParticleSystem GetSkillVFX(int skillId)
    {
        var mapping = GetMapping(skillId);
        if (mapping == null) return null;

        // Defend: sử dụng VFX chung
        if (mapping.isDefend)
            return defendVFX;

        // Attack: sử dụng VFX riêng theo hệ
        return GetAttackVFXByElement(mapping.element);
    }

    private ParticleSystem GetAttackVFXByElement(string element)
    {
        return element switch
        {
            "earth" => attackEarthVFX,
            "wind" => attackWindVFX,
            "water" => attackWaterVFX,
            "fire" => attackFireVFX,
            _ => null
        };
    }

    private SkillMapping GetMapping(int skillId)
    {
        foreach (var mapping in skillMappings)
        {
            if (mapping.skillId == skillId)
                return mapping;
        }
        return null;
    }

    // Getters cho từng VFX
    public ParticleSystem GetDefendVFX() => defendVFX;
    public ParticleSystem GetAttackEarthVFX() => attackEarthVFX;
    public ParticleSystem GetAttackWindVFX() => attackWindVFX;
    public ParticleSystem GetAttackWaterVFX() => attackWaterVFX;
    public ParticleSystem GetAttackFireVFX() => attackFireVFX;

    /// <summary>
    /// Lấy impact VFX dựa trên skill ID
    /// </summary>
    public ParticleSystem GetImpactVFX(int skillId)
    {
        var mapping = GetMapping(skillId);
        if (mapping == null) return null;

        return GetImpactVFXByElement(mapping.element);
    }

    private ParticleSystem GetImpactVFXByElement(string element)
    {
        return element switch
        {
            "earth" => impactEarthVFX,
            "wind" => impactWindVFX,
            "water" => impactWaterVFX,
            "fire" => impactFireVFX,
            _ => null
        };
    }

    /// <summary>
    /// Lấy projectile prefab dựa trên skill ID
    /// </summary>
    public GameObject GetProjectilePrefab(int skillId)
    {
        var mapping = GetMapping(skillId);
        if (mapping == null) return null;

        return GetProjectilePrefabByElement(mapping.element);
    }

    private GameObject GetProjectilePrefabByElement(string element)
    {
        return element switch
        {
            "earth" => projectileEarthPrefab,
            "wind" => projectileWindPrefab,
            "water" => projectileWaterPrefab,
            "fire" => projectileFirePrefab,
            _ => null
        };
    }

    public List<SkillMapping> GetAllMappings() => new List<SkillMapping>(skillMappings);
}
