using UnityEngine;

/// <summary>
/// Boss cuối — BigBoss. 4 phase theo máu, mỗi phase 1 model + 1 hệ + 1 tầm đánh:
///   Đất  (100→75%) — cận chiến
///   Khí  (75→50%)  — đánh xa
///   Nước (50→25%)  — cận chiến
///   Lửa  (25→0%)   — đánh xa
///
/// Việc đổi model / hệ / chỉ số theo %máu do <see cref="BigBossModelSwitcher"/> xử lý.
/// BT này chỉ chọn bộ đòn đánh đúng theo phase hiện tại (guard bằng BigBossPhaseSequence).
///
/// Tái dùng toàn bộ node có sẵn (MeleeAttack/RangedAttack/ChasePlayer...) → không đụng boss cũ.
/// </summary>
[CreateAssetMenu(menuName = "BehaviorTree/Boss/BigBoss")]
public class BossAI_BigBoss : ExternalBehaviorTree
{
    [Header("Combo")]
    [Tooltip("Thời gian chờ giữa 2 đòn trong combo.")]
    public float comboGap = 0.3f;

    public override Task CreateBehaviorTree()
    {
        // ── DIE ──────────────────────────────────────────────────────────────
        var dieSequence = new DieSequence();
        dieSequence.Name = "Die";
        dieSequence.CreateTasks(new IsHealthZero(), new BossDieAction());

        // ── 4 PHASE ───────────────────────────────────────────────────────────
        var earth = BuildMeleePhase(ElementType.Earth, "Phase Đất — Cận chiến");
        var wind  = BuildRangedPhase(ElementType.Wind, "Phase Khí — Đánh xa");
        var water = BuildMeleePhase(ElementType.Water, "Phase Nước — Cận chiến");
        var fire  = BuildRangedPhase(ElementType.Fire, "Phase Lửa — Đánh xa");

        // ── IDLE ─────────────────────────────────────────────────────────────
        var idle = new Idle();
        idle.Name = "Idle";
        idle.DefaultUtility = 1f;

        // ── ROOT ─────────────────────────────────────────────────────────────
        // Utility: Die (999) > phase đang active (10) > Idle (1).
        // 4 phase guard theo CurrentElement của switcher nên luôn chỉ 1 phase > 0.
        var root = new UtilitySelector();
        root.Name = "BigBoss — Final (Đất→Khí→Nước→Lửa)";
        root.CreateTasks(dieSequence, earth, wind, water, fire, idle);

        return root;
    }

    /// <summary>Phase cận chiến: đuổi tới attackRange → Attack01 → chờ → Attack02.</summary>
    private BigBossPhaseSequence BuildMeleePhase(ElementType element, string name)
    {
        var seq = new BigBossPhaseSequence(element, 10f);
        seq.Name = name;
        seq.CreateTasks(
            new IsPlayerDetected(),
            new IsAttackReady(),
            new ChasePlayer(),
            new IsPlayerInAttackRange(),
            new MeleeAttack(),
            new Wait(comboGap),
            new MeleeAttack2()
        );
        return seq;
    }

    /// <summary>Phase đánh xa: đuổi tới khoảng cách giữ rồi bắn — Attack03 (1 viên) → chờ → Attack04 (spread).</summary>
    private BigBossPhaseSequence BuildRangedPhase(ElementType element, string name)
    {
        var seq = new BigBossPhaseSequence(element, 10f);
        seq.Name = name;
        seq.CreateTasks(
            new IsPlayerDetected(),
            new IsAttackReady(),
            new ChasePlayer(),
            new IsPlayerInSkillRange(),
            new RangedAttack03(),
            new Wait(comboGap),
            new RangedAttack04()
        );
        return seq;
    }
}
