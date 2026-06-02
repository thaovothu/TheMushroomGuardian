using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class Arrow : MonoBehaviour
{
    [SerializeField] float arrowDamage = 10f;
    [SerializeField] float arrowSpeed = 50f;
    [SerializeField] float lifetime = 3f;

    Rigidbody rb;
    BoxCollider box;
    Coroutine lifeCoroutine;

    Vector3 prevPosition;
    int combatLayerMask;

    // Dedup theo HealthSystem — tránh damage cùng 1 enemy 2 lần dù nhiều bone collider.
    readonly List<HealthSystem> hasHit = new List<HealthSystem>();
    readonly RaycastHit[] sweepBuffer = new RaycastHit[16];

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
    }

    void OnEnable()
    {
        // Đảm bảo refs không null khi được lấy từ pool.
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (box == null) box = GetComponent<BoxCollider>();

        hasHit.Clear();
        combatLayerMask = LayerMask.GetMask("Enemy", "Boss");

        if (rb != null)
        {
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.velocity = transform.forward * arrowSpeed;
        }

        prevPosition = transform.position;

        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        lifeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    void OnDisable()
    {
        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Inject damage từ BowAttack. Gọi ngay sau ArrowPool.GetArrow().
    /// </summary>
    public void SetDamage(float damage) => arrowDamage = damage;

    // ── Hit detection ─────────────────────────────────────────────────────────

    // SphereCast dọc đoạn arrow vừa di chuyển mỗi FixedUpdate.
    // Cách này là swept detection thực sự — bắt được enemy dù arrow 50 m/s đi qua
    // trong 1 frame mà không để lại overlap (OnTriggerEnter không đủ tin cậy ở tốc độ cao).
    void FixedUpdate()
    {
        Vector3 delta = transform.position - prevPosition;
        float dist = delta.magnitude;

        if (dist > 0.001f)
        {
            int count = Physics.SphereCastNonAlloc(
                prevPosition, 0.15f, delta.normalized,
                sweepBuffer, dist,
                combatLayerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                if (!gameObject.activeInHierarchy) break;
                TryHit(sweepBuffer[i].collider);
            }
        }

        prevPosition = transform.position;
    }

    // OnTriggerEnter là backup cho va chạm cận cảnh mà sweep có thể bỏ sót.
    void OnTriggerEnter(Collider other)
    {
        if (IsSelf(other)) return;

        Transform root = other.transform.root;
        bool isCombat = IsCombatTag(other, root);

        if (isCombat)
        {
            TryHit(other);
            return;
        }

        // Bỏ qua player và trigger thuần (trigger zone, pickup...).
        if (other.CompareTag("Player") || root.CompareTag("Player")) return;
        if (other.isTrigger) return;

        // Solid terrain / wall → thu hồi arrow.
        ReturnToPool();
    }

    void TryHit(Collider other)
    {
        if (other == null || IsSelf(other)) return;

        Transform root = other.transform.root;
        if (!IsCombatTag(other, root)) return;

        HealthSystem hs = other.GetComponentInParent<HealthSystem>();
        if (hs == null || hs.IsDead) return;
        if (hasHit.Contains(hs)) return; // dedup

        hasHit.Add(hs);
        Debug.Log($"[Arrow] Hit {hs.name} — {arrowDamage} dmg");
        hs.TakeDamage(arrowDamage);
        ReturnToPool();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    bool IsSelf(Collider other) =>
        other.transform == transform || other.transform.IsChildOf(transform);

    static bool IsCombatTag(Collider col, Transform root) =>
        col.CompareTag("Enemy") || col.CompareTag("Boss") ||
        root.CompareTag("Enemy") || root.CompareTag("Boss");

    IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(lifetime);
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (ArrowPool.Instance != null)
            ArrowPool.Instance.ReturnArrow(gameObject);
        else
            gameObject.SetActive(false);
    }
}