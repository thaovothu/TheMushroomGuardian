using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gắn lên Player. Tìm enemy gần nhất trong phạm vi.
/// Tái dùng cho BowAttack, Sword auto-face, Skill targeting...
/// </summary>
public class EnemyDetector : MonoBehaviour
{
    [SerializeField] float detectionRange = 15f;
    [Tooltip("Offset Y khi tính vị trí aim. 1f = tầm ngực enemy, 0f = chân.")]
    [SerializeField] float aimHeightOffset = 1f;

    public float DetectionRange => detectionRange;

    int combatLayerMask;

    readonly Collider[] detectBuffer = new Collider[64];
    readonly HashSet<HealthSystem> seenHS = new HashSet<HealthSystem>();

    void Awake()
    {
        combatLayerMask = LayerMask.GetMask("Enemy", "Boss");
    }

    /// <summary>
    /// Transform của HealthSystem enemy gần nhất còn sống. Null nếu không có.
    /// KHÔNG dùng col.transform.root vì enemy có thể là child của một GameObject khác (vd: DataGame).
    /// Thay vào đó lấy hs.transform — luôn là chính xác enemy object chứa HealthSystem.
    /// </summary>
    public Transform GetNearestEnemyTransform()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, detectionRange,
            detectBuffer, combatLayerMask, QueryTriggerInteraction.Ignore);

        Debug.Log($"[EnemyDetector] count={count} mask=0x{combatLayerMask:X} pos={transform.position} range={detectionRange}");
        for (int i = 0; i < count; i++)
            Debug.Log($"  [{i}] {detectBuffer[i].name} layer={LayerMask.LayerToName(detectBuffer[i].gameObject.layer)} tag={detectBuffer[i].tag}");
        if (count == detectBuffer.Length)
            Debug.LogWarning($"[EnemyDetector] detectBuffer tràn ({count}) — tăng kích thước nếu cần.");

        seenHS.Clear();

        Transform nearest = null;
        float nearestSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = detectBuffer[i];
            if (col == null) continue;

            // Layer mask đã lọc Enemy/Boss layer → chỉ cần tìm HealthSystem.
            // KHÔNG dùng col.transform.root (có thể là DataGame hay SceneRoot).
            HealthSystem hs = col.GetComponentInParent<HealthSystem>();
            if (hs == null || hs.IsDead) continue;
            if (!seenHS.Add(hs)) continue; // dedup: cùng 1 enemy nhiều bone collider

            // Khoảng cách tính từ hs.transform.position — chính xác vị trí enemy.
            float sqr = (hs.transform.position - transform.position).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = hs.transform;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Vị trí aim vào enemy gần nhất (enemy.position + aimHeightOffset).
    /// Null nếu không có enemy trong range.
    /// </summary>
    public Vector3? GetNearestEnemyAimPosition()
    {
        Transform t = GetNearestEnemyTransform();
        if (t == null) return null;
        return t.position + Vector3.up * aimHeightOffset;
    }

    /// <summary>Có enemy nào trong range không.</summary>
    public bool HasEnemyInRange() => GetNearestEnemyTransform() != null;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}