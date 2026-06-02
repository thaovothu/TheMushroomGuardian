using UnityEngine;

/// <summary>
/// Controller cho skill projectile bay tầm xa
/// Mỗi skill có riêng 1 SkillProjectile prefab với VFX khác nhau
/// </summary>
public class SkillProjectile : MonoBehaviour
{
    private SkillData skillData;
    private Vector3 direction;
    private float speed;
    private float lifetime = 3f;
    private float elapsedTime = 0f;
    private bool hasHit = false;

    [Header("Visual")]
    [SerializeField] private TrailRenderer trailRenderer;

    void Start()
    {
        Debug.Log($"[SkillProjectile] START - Position: {transform.position}, Direction: {direction}, Speed: {speed}");

        // Setup trail color theo element
        if (trailRenderer != null && skillData != null)
        {
            trailRenderer.startColor = GetElementColor(skillData.element);
            trailRenderer.endColor = GetElementColor(skillData.element);
            Debug.Log($"[SkillProjectile] Trail color set for {skillData.element}");
        }

        // Spawn projectile VFX qua SkillVFXManager
        if (skillData != null && SkillVFXManager.Instance != null)
        {
            Debug.Log($"[SkillProjectile] Spawning VFX for skill ID: {skillData.skillId}");
            SkillVFXManager.Instance.SpawnSkillVFX(skillData.skillId, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning($"[SkillProjectile] ✗ Cannot spawn VFX - skillData: {skillData != null}, VFXManager: {SkillVFXManager.Instance != null}");
        }
    }

    void Update()
    {
        // Nếu chưa initialize → return
        if (skillData == null) return;
        if (hasHit) return;

        elapsedTime += Time.deltaTime;

        // Chuyển động
        transform.position += direction * speed * Time.deltaTime;

        // Visualize đường đi
        Debug.DrawLine(
            transform.position - direction * speed * Time.deltaTime,
            transform.position,
            GetElementColor(skillData.element),
            0.1f
        );

        // Debug info mỗi frame (chi tiết quá thì comment đi)
        if (elapsedTime % 0.5f < Time.deltaTime) // Log mỗi 0.5 giây
        {
            Debug.Log($"[SkillProjectile] {skillData.skillName} - Pos: {transform.position}, Time: {elapsedTime:F2}s/{lifetime}s");
        }

        // Hết lifetime → destroy
        if (elapsedTime >= lifetime)
        {
            Debug.Log($"[SkillProjectile] ✓ {skillData.skillName} reached lifetime, destroying");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        Debug.Log($"[SkillProjectile] OnTriggerEnter: {collision.name}, Tag: {collision.tag}");

        if (skillData == null) return; // Chưa initialize
        
        // Bỏ qua player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"[SkillProjectile] Skipped Player collision");
            return;
        }

        // Đã hit rồi → bỏ qua
        if (hasHit)
        {
            Debug.Log($"[SkillProjectile] Already hit, ignoring");
            return;
        }

        hasHit = true;
        Debug.Log($"[SkillProjectile] Hit registered! Position: {transform.position}");

        // Kiểm tra enemy — GetComponentInParent để trúng cả collider của model con
        // (boss có HealthSystem ở root, collider hiển thị có thể nằm ở child).
        HealthSystem enemyHealth = collision.GetComponentInParent<HealthSystem>();
        if (enemyHealth == null)
        {
            // Trúng vật cản (tường/sàn) không có HealthSystem → bỏ qua, đạn bay tiếp thay vì tự huỷ.
            hasHit = false;
            return;
        }

        if (enemyHealth.IsDead)
        {
            Debug.Log($"[SkillProjectile] ✗ {collision.name} is already dead");
            Destroy(gameObject);
            return;
        }

        // Lấy SkillSystem instance
        if (SkillSystem.Instance == null)
        {
            Debug.LogError($"[SkillProjectile] ✗ SkillSystem.Instance is NULL!");
            Destroy(gameObject);
            return;
        }

        // Tính damage
        Debug.Log($"[SkillProjectile] Calculating damage: skill element={skillData.element}, enemy element={enemyHealth.GetElement()}");
        float multiplier = SkillSystem.Instance.GetElementalMultiplier(skillData.element, enemyHealth.GetElement());
        float finalDamage = skillData.damage * multiplier;

        Debug.Log($"[SkillProjectile] Damage calc: {skillData.damage} * {multiplier:F2} = {finalDamage}");

        enemyHealth.TakeDamage(finalDamage, skillData.element);
        Debug.Log($"[SkillProjectile] ✓ {skillData.skillName} hit {collision.name}: {finalDamage} damage (multiplier: {multiplier:F2})");

        // Spawn impact VFX qua SkillVFXManager
        if (skillData != null && SkillVFXManager.Instance != null)
        {
            SkillVFXManager.Instance.SpawnImpactVFX(skillData.skillId, transform.position);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Setup projectile từ SkillData
    /// </summary>
    public void Initialize(SkillData skill, Vector3 moveDirection)
    {
        skillData = skill;
        direction = moveDirection.normalized;
        speed = GetProjectileSpeed(skill.element);
        lifetime = 3f; // Tối đa 3 giây bay

        // Lấy collision
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero; // Kinematic
            rb.isKinematic = true;
        }

        // Collider check
        Collider col = GetComponent<Collider>();
        Debug.Log($"[SkillProjectile] INIT: {skill.skillName}");
        Debug.Log($"  • Element: {skill.element}");
        Debug.Log($"  • Speed: {speed} units/s");
        Debug.Log($"  • Direction: {direction}");
        Debug.Log($"  • Max distance: {speed * lifetime} units");
        Debug.Log($"  • Has Rigidbody: {rb != null}");
        Debug.Log($"  • Has Collider: {col != null}");
    }

    /// <summary>
    /// Lấy tốc độ tuỳ element
    /// </summary>
    private float GetProjectileSpeed(ElementType element)
    {
        return element switch
        {
            ElementType.Earth => 15f,   // Chậm, mạnh (Địa chấn quyền)
            ElementType.Wind => 25f,    // Nhanh (Cuồng phong)
            ElementType.Water => 18f,   // Vừa (Sóng thần)
            ElementType.Fire => 22f,    // Nhanh (Nham thần)
            _ => 20f
        };
    }

    /// <summary>
    /// Lấy màu tuỳ element
    /// </summary>
    private Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Earth => new Color(0.6f, 0.4f, 0.2f),  // Brown
            ElementType.Wind => new Color(0.7f, 0.9f, 1f),     // Light blue
            ElementType.Water => new Color(0.2f, 0.6f, 1f),    // Blue
            ElementType.Fire => new Color(1f, 0.5f, 0f),       // Orange
            _ => Color.white
        };
    }
}
