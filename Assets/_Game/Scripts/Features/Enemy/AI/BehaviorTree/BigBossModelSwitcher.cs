using System.Collections;
using UnityEngine;

/// <summary>
/// Cấu hình 1 phase của BigBoss: model + hệ + chỉ số chiến đấu + ngưỡng máu kích hoạt.
/// </summary>
[System.Serializable]
public class BigBossPhase
{
    public string label = "Phase";
    [Tooltip("Hệ của phase này — quyết định damage typing + nhánh đòn đánh trong BossAI_BigBoss.")]
    public ElementType element = ElementType.Earth;

    [Tooltip("Model hiển thị trong phase này (1 trong các model con của BigBoss).")]
    public GameObject model;
    [Tooltip("Animator của model. Để trống sẽ tự lấy từ model khi Awake.")]
    public Animator animator;

    [Range(0f, 1f)]
    [Tooltip("Vào phase này khi %máu <= giá trị này. Mặc định: Đất 1.0, Khí 0.75, Nước 0.5, Lửa 0.25.")]
    public float enterAtHPPercent = 1f;

    [Header("Chiến đấu")]
    [Tooltip("true = đánh xa (bắn đạn), false = cận chiến. Chỉ để tham khảo/gizmos; nhánh đòn do BT chọn theo hệ.")]
    public bool isRanged = false;
    [Tooltip("Tốc độ di chuyển trong phase này.")]
    public float moveSpeed = 5f;
    [Tooltip("Hệ số nhân damage đòn đánh trong phase này. Đất = yếu (0.6), Lửa = mạnh...")]
    public float damageMultiplier = 1f;
    [Tooltip("Khoảng cách ChasePlayer dừng lại. Cận chiến nên nhỏ (~3m), đánh xa nên lớn (~8m) để giữ khoảng cách.")]
    public float attackRange = 3f;
    [Tooltip("Độ cao bay (agent.baseOffset). 0 = đứng đất.")]
    public float flyHeight = 0f;

    [Header("Hiệu ứng (tuỳ chọn)")]
    [Tooltip("VFX spawn khi chuyển sang phase này.")]
    public GameObject enterVFX;
}

/// <summary>
/// Bộ chuyển model 4 phase cho boss cuối (BigBoss).
/// Khác BossModelSwitcher (2 phase, soul→giant) — file riêng để KHÔNG ảnh hưởng các boss cũ.
///
/// Phase đổi theo % máu: Đất (100→75) → Khí (75→50) → Nước (50→25) → Lửa (25→0).
/// Mỗi lần đổi: ẩn/hiện model, đổi animator, cập nhật chỉ số trên BossBlackboard, đổi currentElement,
/// bắn event BossEventBus, kèm VFX/slow-mo/freeze/AOE tuỳ chọn.
/// </summary>
public class BigBossModelSwitcher : MonoBehaviour
{
    [Header("Các phase (theo thứ tự máu giảm dần)")]
    [SerializeField] private BigBossPhase[] phases = new BigBossPhase[0];

    [Header("Hiệu ứng chuyển phase")]
    [Tooltip("Thời gian chờ animation 'switchElement' trước khi đổi model.")]
    [SerializeField] private float switchAnimLeadTime = 0.8f;
    [Tooltip("timeScale lúc slow-motion (1 = tắt slow-mo).")]
    [SerializeField] private float slowMotionScale = 0.4f;
    [SerializeField] private float slowMotionDuration = 0.25f;
    [Tooltip("Đứng yên thêm sau khi đổi model để player nhận biết.")]
    [SerializeField] private float freezeDuration = 0.5f;
    [Tooltip("Thời gian sống của VFX spawn ra (<=0 = không tự huỷ).")]
    [SerializeField] private float vfxLifetime = 5f;

    [Header("AOE khi chuyển phase (tuỳ chọn, 0 = tắt)")]
    [SerializeField] private float transitionAOEDamage = 0f;
    [SerializeField] private float transitionAOERadius = 4f;

    private BossBlackboard bb;
    private int currentIndex = 0;
    private bool isTransitioning = false;

    // ── API cho BT đọc ─────────────────────────────────────────────────────────
    public int CurrentPhaseIndex => currentIndex;
    public bool IsTransitioning => isTransitioning;
    public ElementType CurrentElement =>
        (phases != null && currentIndex >= 0 && currentIndex < phases.Length && phases[currentIndex] != null)
            ? phases[currentIndex].element
            : ElementType.Earth;
    public bool IsInPhase(ElementType e) => CurrentElement == e;

    private void Awake()
    {
        bb = GetComponent<BossBlackboard>();

        // Tắt BossModelSwitcher (2-phase) nếu còn sót lại → tránh xung đột set animator/transition.
        var legacy = GetComponent<BossModelSwitcher>();
        if (legacy != null && legacy.enabled)
        {
            legacy.enabled = false;
            Debug.LogWarning("[BigBossModelSwitcher] Đã tắt BossModelSwitcher (2-phase) trên BigBoss để tránh xung đột. Nên Remove hẳn component này.");
        }

        // Auto-fetch animator cho từng phase nếu để trống.
        if (phases != null)
        {
            foreach (var p in phases)
            {
                if (p == null || p.model == null || p.animator != null) continue;
                p.animator = p.model.GetComponent<Animator>()
                          ?? p.model.GetComponentInChildren<Animator>(true);
            }
        }
    }

    private void Start()
    {
        if (phases == null || phases.Length == 0)
        {
            Debug.LogError("[BigBossModelSwitcher] Chưa cấu hình phases!");
            return;
        }

        currentIndex = 0;
        ActivateOnly(0);
        SetActiveAnimator(phases[0]);
        ApplyPhaseStats(phases[0]);
        BossEventBus.OnElementChanged?.Invoke(phases[0].element);
        BossEventBus.OnPhaseChanged?.Invoke(0);
    }

    private void OnEnable() => GameEvent.Combat.OnHealthChanged += OnHealthChanged;
    private void OnDisable()
    {
        GameEvent.Combat.OnHealthChanged -= OnHealthChanged;
        Time.timeScale = 1f;
    }

    private void OnHealthChanged(HealthSystem hs, float current, float max)
    {
        if (bb == null || hs != bb.healthSystem) return;
        if (max <= 0f || isTransitioning) return;
        if (bb.healthSystem != null && bb.healthSystem.IsDead) return;

        int target = ComputePhaseIndex(current / max);
        if (target > currentIndex)
            StartCoroutine(TransitionTo(target));
    }

    /// <summary>Index phase ứng với %máu hiện tại (phase máu thấp nhất mà hp đã chạm tới).</summary>
    private int ComputePhaseIndex(float hpPercent)
    {
        int idx = 0;
        for (int i = 0; i < phases.Length; i++)
            if (phases[i] != null && hpPercent <= phases[i].enterAtHPPercent)
                idx = i;
        return idx;
    }

    private IEnumerator TransitionTo(int targetIndex)
    {
        isTransitioning = true;

        // ── Dừng AI ──
        if (bb != null && bb.agent != null && bb.agent.isOnNavMesh)
            bb.agent.isStopped = true;
        if (bb != null) bb.PlayAnimation(BossAnimState.SwitchElement);

        // ── Slow motion (tuỳ chọn) ──
        if (slowMotionScale < 1f && slowMotionDuration > 0f)
        {
            Time.timeScale = slowMotionScale;
            yield return new WaitForSecondsRealtime(slowMotionDuration);
            Time.timeScale = 1f;
        }

        // ── Chờ animation switch ──
        if (switchAnimLeadTime > 0f)
            yield return new WaitForSeconds(switchAnimLeadTime);

        var target = phases[targetIndex];

        // ── VFX ──
        if (target.enterVFX != null)
        {
            var vfx = Instantiate(target.enterVFX, transform.position, Quaternion.identity);
            if (vfxLifetime > 0f) Destroy(vfx, vfxLifetime);
        }

        // ── Đổi model + animator + chỉ số + hệ ──
        ActivateOnly(targetIndex);
        SetActiveAnimator(target);
        ApplyPhaseStats(target);
        currentIndex = targetIndex;

        // ── Thông báo ra ngoài ──
        BossEventBus.OnElementChanged?.Invoke(target.element);
        BossEventBus.OnPhaseChanged?.Invoke(currentIndex);

        // ── AOE (tuỳ chọn) ──
        if (transitionAOEDamage > 0f && transitionAOERadius > 0f)
            DoAOE(target.element);

        // ── Freeze ──
        if (freezeDuration > 0f)
            yield return new WaitForSeconds(freezeDuration);

        if (bb != null && bb.agent != null && bb.agent.isOnNavMesh
            && (bb.healthSystem == null || !bb.healthSystem.IsDead))
            bb.agent.isStopped = false;

        isTransitioning = false;

        // ── Nếu lúc transition máu tụt thêm qua phase sau → chuyển tiếp ──
        if (bb != null && bb.healthSystem != null && !bb.healthSystem.IsDead)
        {
            int again = ComputePhaseIndex(bb.healthSystem.GetHPPercent());
            if (again > currentIndex)
                StartCoroutine(TransitionTo(again));
        }
    }

    private void ActivateOnly(int index)
    {
        // Model đích của phase hiện tại.
        GameObject target = (index >= 0 && index < phases.Length && phases[index] != null)
            ? phases[index].model : null;

        // Tắt mọi model KHÁC target trước (an toàn khi 2 phase share chung 1 model:
        // không tắt nhầm chính model đang cần bật).
        for (int i = 0; i < phases.Length; i++)
        {
            if (phases[i] == null || phases[i].model == null) continue;
            if (phases[i].model != target) phases[i].model.SetActive(false);
        }

        if (target != null) target.SetActive(true);
    }

    private void SetActiveAnimator(BigBossPhase phase)
    {
        if (bb == null || phase == null || phase.animator == null) return;
        bb.animator = phase.animator;
        bb.currentAnimState = BossAnimState.Idle;
    }

    private void ApplyPhaseStats(BigBossPhase phase)
    {
        if (bb == null || phase == null) return;

        if (bb.agent != null)
        {
            bb.agent.baseOffset = phase.flyHeight;
            bb.agent.speed = phase.moveSpeed;
        }
        bb.moveSpeed = phase.moveSpeed;
        bb.attackRange = phase.attackRange;
        bb.currentElement = phase.element;
        bb.damageMultiplier = phase.damageMultiplier;
    }

    private void DoAOE(ElementType element)
    {
        var hits = Physics.OverlapSphere(transform.position, transitionAOERadius);
        foreach (var h in hits)
        {
            if (!h.CompareTag("Player")) continue;
            var ph = h.GetComponent<HealthSystem>();
            if (ph != null) ph.TakeDamage(transitionAOEDamage, element);
        }
    }

    /// <summary>Reset về phase đầu — gọi khi tái sử dụng từ pool.</summary>
    public void ResetSwitcher()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        isTransitioning = false;
        currentIndex = 0;

        if (phases != null && phases.Length > 0)
        {
            ActivateOnly(0);
            SetActiveAnimator(phases[0]);
            ApplyPhaseStats(phases[0]);
        }
    }

    // Giá trị mặc định khi Add component / Reset trong Editor — đúng spec boss cuối.
    private void Reset()
    {
        phases = new BigBossPhase[]
        {
            new BigBossPhase { label = "Đất (gần, yếu)", element = ElementType.Earth, enterAtHPPercent = 1.00f, isRanged = false, moveSpeed = 5f, attackRange = 4f, damageMultiplier = 0.6f },
            new BigBossPhase { label = "Khí (xa)",        element = ElementType.Wind,  enterAtHPPercent = 0.75f, isRanged = true,  moveSpeed = 6f, attackRange = 9f, damageMultiplier = 1.0f },
            new BigBossPhase { label = "Nước (gần)",      element = ElementType.Water, enterAtHPPercent = 0.50f, isRanged = false, moveSpeed = 5f, attackRange = 4f, damageMultiplier = 1.0f },
            new BigBossPhase { label = "Lửa (xa, mạnh)",  element = ElementType.Fire,  enterAtHPPercent = 0.25f, isRanged = true,  moveSpeed = 6f, attackRange = 9f, damageMultiplier = 1.3f },
        };
    }

    private void OnDrawGizmosSelected()
    {
        if (transitionAOEDamage > 0f)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, transitionAOERadius);
        }
    }
}
