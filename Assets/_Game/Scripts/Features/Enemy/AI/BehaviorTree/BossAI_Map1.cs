using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/Boss/Map1")]
public class BossAI_Map1 : ExternalBehaviorTree
{
    public override Task CreateBehaviorTree()
    {
        // ── DIE SEQUENCE: Dynamic utility - only active when IsDead = true ──
        var dieSequence = new DieSequence();
        dieSequence.Name = "Die";
        dieSequence.CreateTasks(
            new IsHealthZero(),         // Check if health = 0
            new BossDieAction()         // Play die animation and remove
        );
        // Utility set dynamically in DieSequence.GetUtility()

        // ── Hit Sequence: khi bị đánh thì chuyển sang hit state ──
        var hitSequence = new HitSequence();
        hitSequence.Name = "Hit (Reaction)";
        hitSequence.CreateTasks(
            new IsHit(),                // Check if boss was hit
            new BossHitAction()         // Play hit animation
        );

        // ── Phase 2: HP < 50% → switch nguyên tố rồi mới dùng skill ──
        var phase2 = new Phase2Sequence();
        phase2.Name = "Phase 2 (Skill Attack)";
        phase2.CreateTasks(
            new IsPlayerDetected(),                     // Player detected?
            new IsHPBelow(0.5f),                        // check HP
            new SwitchElement(ElementType.Wind),        // đổi sang Wind nếu chưa
            new IsAttackReady(skill: true),             // cooldown skill ok? (5s)
            new ChasePlayer(),
            new IsPlayerInAttackRange(),                // trong tầm?
            new SkillAttack()                           // đánh skill (1.5x damage)
        );

        // ── Phase 1: đuổi → đánh thường ──
        // Similar to Enemy pattern: Chase only if player detected
        var phase1Chase = new Sequence();
        phase1Chase.Name = "Chase + Attack";
        phase1Chase.CreateTasks(
            new IsPlayerDetected(),         // Detect range (player detected?)
            new IsAttackReady(),            // cooldown thường ok? (2s)
            new ChasePlayer(),              // đuổi đến khi vào range (ChasePlayer tự check attack range)
            new MeleeAttack()               // đánh thường
        );
        phase1Chase.DefaultUtility = 5f;

        // ── Idle fallback: wait for player detection ──
        var idle = new Idle();
        idle.Name = "Idle (no player detected)";
        idle.DefaultUtility = 1f;

        // ── Root: UtilitySelector chọn hành vi có utility cao nhất ──
        // Priority: Die (0 normally, 999 when dead) > Hit (20) > Phase 2 (10) > Phase 1 (5) > Idle (1)
        var root = new UtilitySelector();
        root.Name = "Boss Root";
        root.CreateTasks(dieSequence, hitSequence, phase2, phase1Chase, idle);

        return root;
    }
}
