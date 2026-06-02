using UnityEngine;

/// <summary>
/// Đạn của Boss — bay thẳng theo hướng được set lúc spawn.
/// Gắn lên prefab đạn boss, không dùng SkillProjectile (đó là đạn player).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BossProjectile : MonoBehaviour
{
    [SerializeField] float speed = 12f;
    [SerializeField] float lifetime = 4f;
    [SerializeField] float damage = 10f;  // override từ BurstBulletAction

    Rigidbody rb;
    bool hasHit;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi ngay sau Instantiate để set damage + kích hoạt chuyển động.
    /// direction phải là vector đã normalized.
    /// </summary>
    public void Initialize(Vector3 direction, float dmg)
    {
        damage = dmg;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = direction.normalized * speed;
        }
        Destroy(gameObject, lifetime);
        Debug.Log($"[BossProjectile] Init — dir={direction} dmg={damage} speed={speed}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (!other.CompareTag("Player")) return;

        hasHit = true;
        var hs = other.GetComponentInParent<HealthSystem>();
        if (hs != null)
        {
            hs.TakeDamage(damage, ElementType.Fire);
            Debug.Log($"[BossProjectile] Hit player — {damage} Fire dmg");
        }
        Destroy(gameObject);
    }
}