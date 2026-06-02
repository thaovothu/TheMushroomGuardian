using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/Boss/Map4")]
public class BossAI_Map4 : ExternalBehaviorTree
{
    [Header("Phase 2 Burst")]
    [Tooltip("Khoảng cách (m) player lại gần boss để trigger vòng đạn tròn.")]
    public float burstTriggerRange = 4f;

    public override Task CreateBehaviorTree()
    {
        // ── DIE ──────────────────────────────────────────────────────────────
        var dieSequence = new DieSequence();
        dieSequence.Name = "Die";
        dieSequence.CreateTasks(new IsHealthZero(), new BossDieAction());

        // ── PHASE 1: HP > 50% — cận chiến, tốc độ +30% ──────────────────────
        // Sequence: detect → cooldown → chase → vào range → Attack01 → 0.3s → Attack02
        var phase1Melee = new Phase1MeleeSequence();
        phase1Melee.Name = "Phase 1 — Melee Combo (Attack01 + Attack02)";
        phase1Melee.CreateTasks(
            new IsPlayerDetected(),
            new IsAttackReady(),
            new ChasePlayer(),
            new IsPlayerInAttackRange(),
            new MeleeAttack(),          // Attack01 + delay 0.8s nội bộ
            new Wait(0.3f),
            new MeleeAttack2()          // Attack02
        );

        // ── PHASE 2A: HP <= 50% — đánh xa, player ở xa ──────────────────────
        // Sequence: detect → cooldown → chase nhẹ → Attack03 → 0.4s → Attack04 (spread)
        var phase2Ranged = new Phase2RangedSequence();
        phase2Ranged.Name = "Phase 2 — Ranged Combo (Attack03 + Attack04)";
        phase2Ranged.CreateTasks(
            new IsPlayerDetected(),
            new IsAttackReady(),
            new ChasePlayer(),              // chase đến skillRange rồi dừng
            new IsPlayerInSkillRange(),     // đủ trong tầm bắn là bắn, không cần vào sát
            new RangedAttack03(),           // bắn 1 viên thẳng
            new Wait(0.4f),
            new RangedAttack04()            // bắn 3 viên spread
        );

        // ── PHASE 2B: HP <= 50% — player lại gần → xả vòng đạn tròn ─────────
        var phase2Burst = new Phase2BurstSequence(burstTriggerRange);
        phase2Burst.Name = "Phase 2 — Burst Ring (player too close)";
        phase2Burst.CreateTasks(
            new IsPlayerDetected(),
            new IsPlayerTooClose(burstTriggerRange),
            new BurstBulletAction()
        );

        // ── IDLE ─────────────────────────────────────────────────────────────
        var idle = new Idle();
        idle.DefaultUtility = 1f;

        // ── ROOT ─────────────────────────────────────────────────────────────
        // Utility priority:
        //   Die (999) > Burst (16) > Ranged (12) > Melee (10) > Idle (1)
        // Phase guard trong GetUtility() đảm bảo phase 1 và phase 2 không overlap:
        //   - Phase1MeleeSequence  → trả 0 khi IsInPhase2 = true
        //   - Phase2RangedSequence → trả 0 khi IsInPhase2 = false
        //   - Phase2BurstSequence  → trả 0 khi IsInPhase2 = false hoặc player ở xa
        var root = new UtilitySelector();
        root.Name = "Boss Map 4 — Fire Boss";
        root.CreateTasks(
            dieSequence,    // 999 khi chết
            phase2Burst,    // 16 — phase 2, player lao vào
            phase2Ranged,   // 12 — phase 2, đánh xa
            phase1Melee,    // 10 — phase 1, cận chiến
            idle            // 1  — fallback
        );

        return root;
    }
}