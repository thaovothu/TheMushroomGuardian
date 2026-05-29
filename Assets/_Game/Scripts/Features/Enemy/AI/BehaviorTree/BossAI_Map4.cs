using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/Boss/Map4")]
public class BossAI_Map4 : ExternalBehaviorTree
{
    public override Task CreateBehaviorTree()
    {
        // ── DIE SEQUENCE: Dynamic utility - only active when IsDead = true ──
        var dieSequence = new DieSequence();
        dieSequence.Name = "Die";
        dieSequence.CreateTasks(
            new IsHealthZero(),         // Check if health = 0
            new BossDieAction()         // Play die animation and release pool
        );
        // Utility set dynamically in DieSequence.GetUtility()

        // ── Counter Attack: Khi bị đánh + player in range → đánh trả ngay ──
        // Phase 1 ưu tiên cận chiến, không cho player spam Water rồi rút lui.
        // Dùng CounterAttackSequence (utility ĐỘNG): chỉ utility = 25 khi isHit && in-range,
        // còn lại = 0. Nếu dùng Sequence tĩnh = 25 thì UtilitySelector lặp vô tận ở counterAttack
        // → boss đứng yên không bao giờ tới Chase+Attack.
        var counterAttack = new CounterAttackSequence();
        counterAttack.Name = "Counter Attack (Lava Slam)";
        counterAttack.CreateTasks(
            new IsHit(),                        // Boss bị đánh
            new IsPlayerDetected(),             // Phát hiện player
            new IsPlayerInAttackRange(),        // Player trong tầm cận chiến
            new MeleeAttack()                   // Đánh trả ngay (cận chiến)
        );

        // ── Hit Animation: Nếu player out of range → chỉ play hit anim ──
        var hitSequence = new HitSequence();
        hitSequence.Name = "Hit Reaction (Out of Range)";
        hitSequence.CreateTasks(
            new IsHit(),
            new BossHitAction()
        );
        hitSequence.DefaultUtility = 22f;

        // ── Phase 2: Giáp vỡ, lõi lửa lộ ra → tấn công tầm xa bằng skill ──
        // HP < 60% → switch mode aggressive, dùng SkillAttack (3 cầu lửa hình quạt)
        var phase2 = new Phase2Sequence();
        phase2.Name = "Phase 2 (Core Exposed - Ranged Fire Skill)";
        phase2.CreateTasks(
            new IsPlayerDetected(),
            new IsHPBelow(0.6f),                    // giáp vỡ khi HP < 60%
            new SwitchElement(ElementType.Fire),    // boss vẫn là Fire, chỉ chuyển sang aggressive mode
            new IsAttackReady(skill: true),         // skill cooldown ok? (5s)
            new ChasePlayer(),                      // chase đến tầm skill
            new IsPlayerInAttackRange(),            // trong tầm skill?
            new SkillAttack()                       // bắn 3 cầu lửa
        );
        // Utility cao khi HP < 60% (Phase2Sequence tự xử lý threshold)

        // ── Phase 1: Đi bộ chậm + đánh cận chiến (giáp dung nham) ──
        var phase1Chase = new Sequence();
        phase1Chase.Name = "Phase 1 (Slow Chase + Melee - Armored)";
        phase1Chase.CreateTasks(
            new IsPlayerDetected(),
            new IsAttackReady(),                    // melee cooldown ok? (2s)
            new ChasePlayer(),                      // truy đuổi chậm
            new MeleeAttack()                       // đánh cận chiến
        );
        phase1Chase.DefaultUtility = 10f;

        // ── Idle fallback: wait for player detection ──
        var idle = new Idle();
        idle.Name = "Idle (no player detected)";
        idle.DefaultUtility = 1f;

        // ── Root: UtilitySelector chọn hành vi có utility cao nhất ──
        // Priority: Die (999 when dead) > Counter Attack (25) > Hit (22) 
        //         > Phase 2 (15 - dynamic via Phase2Sequence) > Phase 1 (10) > Idle (1)
        var root = new UtilitySelector();
        root.Name = "Boss Map 4 Root (Lava Giant)";
        root.CreateTasks(
            dieSequence,
            counterAttack,
            hitSequence,
            phase2,
            phase1Chase,
            idle
        );

        return root;
    }
}