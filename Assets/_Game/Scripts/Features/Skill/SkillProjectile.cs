using UnityEngine;

/// <summary>
/// Controller cho skill projectile bay tầm xa
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
    [SerializeField] private ParticleSystem particleSystem;

    void Start()
    {
        // Setup trail color theo element
        if (trailRenderer != null && skillData != null)
        {
            trailRenderer.startColor = GetElementColor(skillData.element);
            trailRenderer.endColor = GetElementColor(skillData.element);
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

        // Hết lifetime → destroy
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (skillData == null) return; // Chưa initialize
        
        // Bỏ qua player
        if (collision.gameObject.CompareTag("Player"))
            return;

        // Đã hit rồi → bỏ qua
        if (hasHit) return;

        hasHit = true;

        // Kiểm tra enemy
        HealthSystem enemyHealth = collision.GetComponent<HealthSystem>();
        if (enemyHealth != null && !enemyHealth.IsDead)
        {
            // Tính damage
            float multiplier = SkillSystem.Instance.GetElementalMultiplier(skillData.element, enemyHealth.GetElement());
            float finalDamage = skillData.damage * multiplier;

            enemyHealth.TakeDamage(finalDamage, skillData.element);
            Debug.Log($"[SkillProjectile] ✓ Hit {collision.name}: {finalDamage} damage");

            // Spawn explosion effect
            if (particleSystem != null)
            {
                var particles = Instantiate(particleSystem, transform.position, Quaternion.identity);
                particles.Play();
                Destroy(particles.gameObject, 2f);
            }
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

        Debug.Log($"[SkillProjectile] Initialized: {skill.skillName}, Element: {skill.element}, Speed: {speed}");
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
