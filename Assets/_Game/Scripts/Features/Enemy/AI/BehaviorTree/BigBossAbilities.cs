using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điều phối các chiêu đặc biệt theo phase của BigBoss (boss cuối). Chạy SONG SONG với
/// BehaviorTree (BT lo chase + đòn đánh thường), còn component này lo cơ chế riêng từng phase:
///
///   • Đất  — không có gì thêm (đánh gần, damage yếu do BigBossModelSwitcher set).
///   • Khí  — spawn 4 trụ + BẤT TỬ; phá hết trụ thì boss mới nhận damage để tụt xuống phase sau.
///   • Nước — liên tục tạo vùng nước (DoT) tại chỗ player, player phải né ra.
///   • Lửa  — định kỳ quét vòng lửa khắp sàn; player phải nhảy hoặc dash để né.
///
/// Phase do BigBossModelSwitcher quyết định (theo %máu). Component này chỉ đọc CurrentElement.
/// Đặt trên cùng GameObject với BossBlackboard / HealthSystem / BigBossModelSwitcher.
/// </summary>
[RequireComponent(typeof(BigBossModelSwitcher))]
public class BigBossAbilities : MonoBehaviour
{
    [Header("Phase Khí — 4 Trụ bất tử")]
    [Tooltip("Prefab trụ: cần HealthSystem + Collider + tag mà player đánh trúng được (Enemy/Boss).")]
    public GameObject pillarPrefab;
    [Tooltip("Vị trí đặt trụ: kéo các empty GameObject (nên là con của BigBoss) vào đây. " +
             "Để TRỐNG → tự xếp vòng tròn quanh boss theo pillarCount/pillarRadius.")]
    public Transform[] pillarSpawnPoints;
    [Tooltip("Số trụ khi tự xếp vòng tròn (bỏ qua nếu đã gán pillarSpawnPoints).")]
    public int pillarCount = 4;
    [Tooltip("Bán kính vòng tròn đặt trụ quanh boss (chế độ tự xếp).")]
    public float pillarRadius = 6f;
    [Tooltip("Máu mỗi trụ.")]
    public float pillarHP = 30f;
    public GameObject pillarDeathVFX;

    [Header("Phase Nước — Vùng nước (DoT)")]
    public GameObject waterZonePrefab;
    [Tooltip("Bao lâu tạo 1 vùng nước (giây).")]
    public float waterZoneInterval = 2f;
    public float waterZoneDamage = 8f;
    public float waterZoneLifetime = 4f;

    [Header("Phase Lửa — Quét sàn vòng tròn")]
    public GameObject fireSweepPrefab;
    [Tooltip("Chờ bao lâu sau khi vào phase Lửa mới quét lần đầu.")]
    public float fireSweepFirstDelay = 2f;
    [Tooltip("Bao lâu quét 1 lần (giây).")]
    public float fireSweepInterval = 6f;
    public float fireSweepDamage = 35f;
    public float fireSweepExpandSpeed = 12f;
    public float fireSweepMaxRadius = 20f;

    [Header("Chung")]
    [Tooltip("Bất tử trong lúc biến hình để tránh bị chém khi đang đứng yên đổi model.")]
    public bool invulnerableDuringTransition = true;

    private BossBlackboard bb;
    private BigBossModelSwitcher switcher;
    private HealthSystem bossHealth;

    private ElementType _lastElement = ElementType.None;
    private readonly List<HealthSystem> _pillars = new List<HealthSystem>();
    private Coroutine _waterRoutine;
    private Coroutine _fireRoutine;

    private void Awake()
    {
        bb = GetComponent<BossBlackboard>();
        switcher = GetComponent<BigBossModelSwitcher>();
        bossHealth = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        _lastElement = ElementType.None;     // để re-enter phase khi spawn/tái dùng
        GameEvent.Combat.OnDeath += OnAnyDeath;
    }

    private void OnDisable()
    {
        GameEvent.Combat.OnDeath -= OnAnyDeath;
        StopHazards();
        ClearPillars();
        if (bossHealth != null) bossHealth.IsInvulnerable = false;
    }

    private void Update()
    {
        if (switcher == null || bossHealth == null) return;

        // Đang biến hình → bất tử (tuỳ chọn), chưa xử lý phase mới tới khi xong.
        if (switcher.IsTransitioning)
        {
            if (invulnerableDuringTransition) bossHealth.IsInvulnerable = true;
            return;
        }

        ElementType cur = switcher.CurrentElement;
        if (cur != _lastElement)
        {
            ExitPhase(_lastElement);
            EnterPhase(cur);
            _lastElement = cur;
        }
    }

    // ── Phase enter/exit ─────────────────────────────────────────────────────

    private void EnterPhase(ElementType element)
    {
        switch (element)
        {
            case ElementType.Wind:   // Khí — trụ + bất tử
                SpawnPillars();
                bossHealth.IsInvulnerable = _pillars.Count > 0; // chỉ bất tử nếu thực sự có trụ
                break;

            case ElementType.Water:  // Nước — vùng nước
                bossHealth.IsInvulnerable = false;
                _waterRoutine = StartCoroutine(WaterLoop());
                break;

            case ElementType.Fire:   // Lửa — quét sàn
                bossHealth.IsInvulnerable = false;
                _fireRoutine = StartCoroutine(FireLoop());
                break;

            default:                 // Đất / None
                bossHealth.IsInvulnerable = false;
                break;
        }
    }

    private void ExitPhase(ElementType element)
    {
        switch (element)
        {
            case ElementType.Wind:
                ClearPillars();
                bossHealth.IsInvulnerable = false;
                break;
            case ElementType.Water:
                StopWater();
                break;
            case ElementType.Fire:
                StopFire();
                break;
        }
    }

    // ── Trụ (phase Khí) ──────────────────────────────────────────────────────

    private void SpawnPillars()
    {
        ClearPillars();

        if (pillarPrefab == null)
        {
            Debug.LogWarning("[BigBossAbilities] Chưa gán pillarPrefab → bỏ cơ chế bất tử (boss nhận damage bình thường).");
            return;
        }

        // Ưu tiên: pillarSpawnPoints (trên boss) > BigBossArena (đặt sẵn trong scene) > tự xếp vòng tròn.
        Transform[] points = ResolvePillarPoints();
        if (points != null && points.Length > 0)
        {
            foreach (var pt in points)
                if (pt != null) SpawnOnePillar(pt.position, pt.rotation);
        }
        else
        {
            int n = Mathf.Max(1, pillarCount);
            for (int i = 0; i < n; i++)
            {
                float ang = i * (360f / n);
                Vector3 offset = Quaternion.Euler(0f, ang, 0f) * Vector3.forward * pillarRadius;
                SpawnOnePillar(transform.position + offset, Quaternion.identity);
            }
        }
    }

    /// <summary>Vị trí trụ: ưu tiên điểm gán trên boss → BigBossArena trong scene → null (tự xếp vòng tròn).</summary>
    private Transform[] ResolvePillarPoints()
    {
        if (pillarSpawnPoints != null && pillarSpawnPoints.Length > 0)
            return pillarSpawnPoints;

        var arena = FindObjectOfType<BigBossArena>();
        if (arena != null && arena.pillarPoints != null && arena.pillarPoints.Length > 0)
            return arena.pillarPoints;

        return null;
    }

    private void SpawnOnePillar(Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(pillarPrefab, pos, rot);
        var hs = go.GetComponent<HealthSystem>();
        if (hs != null)
        {
            hs.Init(pillarHP);
            _pillars.Add(hs);
        }
        else
        {
            Debug.LogWarning("[BigBossAbilities] Pillar prefab thiếu HealthSystem!");
            Destroy(go);
        }
    }

    private void ClearPillars()
    {
        foreach (var hs in _pillars)
            if (hs != null) Destroy(hs.gameObject);
        _pillars.Clear();
    }

    private void OnAnyDeath(HealthSystem hs)
    {
        if (hs == null || !_pillars.Remove(hs)) return; // không phải trụ của ta

        if (pillarDeathVFX != null)
            Instantiate(pillarDeathVFX, hs.transform.position, Quaternion.identity);
        Destroy(hs.gameObject);

        // Phá hết trụ → boss hết bất tử, player có thể đánh tụt máu xuống phase sau.
        if (_pillars.Count == 0 && bossHealth != null)
        {
            bossHealth.IsInvulnerable = false;
            Debug.Log("[BigBossAbilities] Đã phá hết trụ — boss hết bất tử!");
        }
    }

    // ── Vùng nước (phase Nước) ───────────────────────────────────────────────

    private IEnumerator WaterLoop()
    {
        var wait = new WaitForSeconds(waterZoneInterval);
        while (true)
        {
            if (waterZonePrefab != null && bb != null && bb.player != null)
            {
                Vector3 pos = bb.player.position;
                var go = Instantiate(waterZonePrefab, pos, Quaternion.identity);
                var wz = go.GetComponent<BossWaterZone>();
                if (wz != null) wz.Init(waterZoneDamage, waterZoneLifetime);
            }
            yield return wait;
        }
    }

    private void StopWater()
    {
        if (_waterRoutine != null) { StopCoroutine(_waterRoutine); _waterRoutine = null; }
    }

    // ── Quét lửa (phase Lửa) ─────────────────────────────────────────────────

    private IEnumerator FireLoop()
    {
        if (fireSweepFirstDelay > 0f) yield return new WaitForSeconds(fireSweepFirstDelay);
        var wait = new WaitForSeconds(fireSweepInterval);
        while (true)
        {
            if (fireSweepPrefab != null)
            {
                var go = Instantiate(fireSweepPrefab, transform.position, Quaternion.identity);
                var fs = go.GetComponent<BossFireSweep>();
                if (fs != null)
                    fs.Init(bb != null ? bb.player : null, fireSweepDamage, fireSweepExpandSpeed, fireSweepMaxRadius);
            }
            yield return wait;
        }
    }

    private void StopFire()
    {
        if (_fireRoutine != null) { StopCoroutine(_fireRoutine); _fireRoutine = null; }
    }

    private void StopHazards()
    {
        StopWater();
        StopFire();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.6f);
        if (pillarSpawnPoints != null && pillarSpawnPoints.Length > 0)
        {
            foreach (var pt in pillarSpawnPoints)
                if (pt != null) Gizmos.DrawWireSphere(pt.position, 0.6f);
        }
        else
        {
            int n = Mathf.Max(1, pillarCount);
            for (int i = 0; i < n; i++)
            {
                float ang = i * (360f / n);
                Vector3 offset = Quaternion.Euler(0f, ang, 0f) * Vector3.forward * pillarRadius;
                Gizmos.DrawWireSphere(transform.position + offset, 0.6f);
            }
        }
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, fireSweepMaxRadius);
    }
}
