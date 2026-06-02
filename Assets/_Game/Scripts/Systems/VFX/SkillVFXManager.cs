using UnityEngine;

/// <summary>
/// Quản lý VFX cho tất cả Skill (Attack Melee + Attack Ranged + Defend)
/// Sử dụng SkillProjectileConfig unity (một config cho tất cả)
/// </summary>
public class SkillVFXManager : BaseSingleton<SkillVFXManager>
{
    [SerializeField] private SkillProjectileConfig skillConfig;

    void Start()
    {
        if (skillConfig == null)
        {
            Debug.LogError("[SkillVFXManager] ✗ SkillProjectileConfig not assigned!");
            return;
        }
        Debug.Log("[SkillVFXManager] ✓ Initialized");
    }

    /// <summary>
    /// Spawn VFX chính cho skill (Attack Melee hoặc Defend)
    /// </summary>
    public void SpawnSkillVFX(int skillId, Vector3 position, Quaternion rotation = default)
    {
        if (skillConfig == null)
        {
            Debug.LogError("[SkillVFXManager] ✗ Config not assigned!");
            return;
        }

        var vfx = skillConfig.GetSkillVFX(skillId);
        if (vfx == null)
        {
            Debug.LogWarning($"[SkillVFXManager] ✗ No VFX for skill ID: {skillId}");
            return;
        }

        var instance = Instantiate(vfx, position, rotation);
        // Prefab VFX đã bật "Play On Awake" → tự chạy khi Instantiate.
        // KHÔNG gọi Play() nữa, gọi thêm sẽ phát hiệu ứng 2 lần.
        Destroy(instance.gameObject, 3f);

        Debug.Log($"[SkillVFXManager] ✓ Spawned skill VFX for skill ID: {skillId}");
    }

    /// <summary>
    /// Spawn impact VFX khi hit enemy
    /// </summary>
    public void SpawnImpactVFX(int skillId, Vector3 position)
    {
        if (skillConfig == null)
        {
            Debug.LogError("[SkillVFXManager] ✗ Config not assigned!");
            return;
        }

        var vfx = skillConfig.GetImpactVFX(skillId);
        if (vfx == null)
        {
            Debug.LogWarning($"[SkillVFXManager] ✗ No impact VFX for skill ID: {skillId}");
            return;
        }

        var instance = Instantiate(vfx, position, Quaternion.identity);
        // Đã có Play On Awake → không gọi Play() để tránh phát 2 lần.
        Destroy(instance.gameObject, 2f);

        Debug.Log($"[SkillVFXManager] ✓ Spawned impact VFX for skill ID: {skillId}");
    }

    /// <summary>
    /// Lấy projectile prefab cho ranged attack
    /// </summary>
    public GameObject GetProjectilePrefab(int skillId)
    {
        if (skillConfig == null)
        {
            Debug.LogError("[SkillVFXManager] ✗ Config not assigned!");
            return null;
        }

        var prefab = skillConfig.GetProjectilePrefab(skillId);
        if (prefab == null)
        {
            Debug.LogError($"[SkillVFXManager] ✗ Projectile prefab not assigned for skill ID: {skillId}. Check SkillProjectileConfig in Inspector!");
        }
        return prefab;
    }
}
