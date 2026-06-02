using System.Collections;
using UnityEngine;

/// <summary>
/// Gắn lên Bow prefab (child của WeaponHolderLeft).
/// Animation Event gọi FireArrow() đúng frame tay kéo dây cung.
/// </summary>
public class BowAttack : MonoBehaviour
{
    [Header("Spawn")]
    [Tooltip("Cộng thêm Y vào vị trí spawn arrow. Chỉnh nếu ArrowSpawnPoint đặt thấp.")]
    [SerializeField] float spawnHeightOffset = 0f;

    [Header("Damage")]
    [Tooltip("Damage flat của mũi tên. Không tính elemental multiplier.")]
    [SerializeField] float bowDamage = 15f;

    [Header("Timing")]
    [Tooltip("Giây từ lúc animation event gọi FireArrow đến khi arrow bay. 0 = ngay lập tức.")]
    [SerializeField] float fireDelay = 0f;

    Transform arrowSpawnPoint;
    PlayerController playerController;
    EnemyDetector enemyDetector;
    Transform playerRoot;

    void Start()
    {
        // ArrowSpawnPoint phải là child trực tiếp của Player root (không phải child của Bow model).
        arrowSpawnPoint = transform.root.Find("ArrowSpawnPoint");
        if (arrowSpawnPoint == null)
            Debug.LogWarning("[BowAttack] ArrowSpawnPoint không tìm thấy — dùng transform.position + offset.");

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerRoot = playerGO.transform;
            playerController = playerGO.GetComponent<PlayerController>();
            enemyDetector = playerGO.GetComponent<EnemyDetector>();

            if (enemyDetector == null)
                Debug.LogWarning("[BowAttack] EnemyDetector không tìm thấy trên Player — auto-aim sẽ không hoạt động.");
            if (playerController == null)
                Debug.LogWarning("[BowAttack] PlayerController không tìm thấy trên Player — player sẽ không xoay khi bắn.");
        }
        else
        {
            playerRoot = transform.root;
            Debug.LogWarning("[BowAttack] Không tìm thấy GameObject tag 'Player'.");
        }

        Debug.Log($"[BowAttack] Init — root={playerRoot?.name} dmg={bowDamage} delay={fireDelay}s");
    }

    // ── Animation Event ───────────────────────────────────────────────────────
    // Gọi đúng frame tay player kéo dây cung.
    // Direction snapshot ngay tại đây — không bị stale nếu có fireDelay.
    public void FireArrow()
    {
        Vector3 firePos = GetFirePosition();
        Vector3 dir = ComputeArrowDirection(firePos);

        // Xoay player về hướng bắn ngay (trước cả delay — player snap về target trước khi arrow ra).
        RotatePlayer(dir);

        Debug.Log($"[BowAttack] FireArrow — pos={firePos} dir={dir}");

        if (fireDelay > 0f)
            StartCoroutine(SpawnArrowDelayed(firePos, dir, fireDelay));
        else
            SpawnArrow(firePos, dir);
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────

    IEnumerator SpawnArrowDelayed(Vector3 firePos, Vector3 dir, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnArrow(firePos, dir);
    }

    void SpawnArrow(Vector3 firePos, Vector3 dir)
    {
        if (ArrowPool.Instance == null)
        {
            Debug.LogError("[BowAttack] ArrowPool.Instance is null!");
            return;
        }

        GameObject arrowGO = ArrowPool.Instance.GetArrow(firePos, Quaternion.LookRotation(dir));

        // Inject damage — Arrow dùng giá trị này thay vì SerializeField nội bộ.
        Arrow arrow = arrowGO.GetComponent<Arrow>();
        if (arrow != null)
            arrow.SetDamage(bowDamage);
        else
            Debug.LogWarning("[BowAttack] Prefab arrow thiếu component Arrow!");

        Debug.Log($"[BowAttack] Arrow spawned — dir={dir} dmg={bowDamage}");
    }

    // ── Direction ─────────────────────────────────────────────────────────────

    Vector3 ComputeArrowDirection(Vector3 firePos)
    {
        // Hỏi EnemyDetector — nó lo việc tìm và dedup enemy.
        Vector3? aimPos = enemyDetector?.GetNearestEnemyAimPosition();

        if (aimPos.HasValue)
        {
            Vector3 dir = (aimPos.Value - firePos).normalized;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Debug.Log($"[BowAttack] Auto-aim → {aimPos.Value}");
                return dir;
            }
        }

        // Fallback: bắn thẳng trước mặt player (flatten Y — không chúi xuống đất).
        Debug.Log("[BowAttack] Không có địch trong phạm vi → bắn thẳng trước mặt");
        Vector3 forward = playerRoot != null ? playerRoot.forward : transform.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    Vector3 GetFirePosition()
    {
        Vector3 pos = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
        pos.y += spawnHeightOffset;
        return pos;
    }

    void RotatePlayer(Vector3 dir)
    {
        if (playerController != null)
        {
            playerController.RotateToward(dir);
            return;
        }
        // Fallback nếu không có PlayerController.
        if (playerRoot != null)
        {
            Vector3 flat = new Vector3(dir.x, 0f, dir.z);
            if (flat.sqrMagnitude > 0.0001f)
                playerRoot.rotation = Quaternion.LookRotation(flat);
        }
    }
}