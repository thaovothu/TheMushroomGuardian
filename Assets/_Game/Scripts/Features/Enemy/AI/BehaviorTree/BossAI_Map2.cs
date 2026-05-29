using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/Boss/Map2")]
public class BossAI_Map2 : ExternalBehaviorTree
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

        // ── Counter Attack: Khi bị đánh, phản công lại (không bị hit thường xuyên) ──
        // Dùng CounterAttackSequence (utility ĐỘNG) thay vì Sequence thường — nếu dùng
        // Sequence với DefaultUtility tĩnh = 25 thì UtilitySelector chọn nó mỗi frame,
        // IsHit fail → vòng lặp vô tận → boss đứng yên không bao giờ tới Chase+Attack.
        var counterAttack = new CounterAttackSequence();
        counterAttack.Name = "Counter Attack (Smart Reaction)";
        counterAttack.CreateTasks(
            new IsHit(),                        // Check if boss was hit
            new IsPlayerDetected(),             // Chỉ phản công nếu thấy player
            new IsPlayerInAttackRange(),        // Nếu player trong range
            new MeleeAttack()                   // Đánh lại ngay
        );
        // Utility set dynamically trong CounterAttackSequence.GetUtility() (= 25 khi đủ
        // điều kiện, = 0 khi không). Không cần DefaultUtility nữa.

        // ── Hit Animation: Nếu player k trong range thì chỉ play hit animation ──
        var hitSequence = new HitSequence();
        hitSequence.Name = "Hit (Reaction - Out of Range)";
        hitSequence.CreateTasks(
            new IsHit(),                // Check if boss was hit
            new BossHitAction()         // Play hit animation
        );
        hitSequence.DefaultUtility = 22f;

        // ── Health Regeneration: Hồi 2% HP nếu không bị đánh ──
        var regeneration = new Sequence();
        regeneration.Name = "Health Regeneration";
        regeneration.CreateTasks(
            new IsHealthNotFull(),              // Chỉ hồi khi máu k full
            new BossRegenerate()                // Hồi 2% máu
        );
        regeneration.DefaultUtility = 2f;      // Utility thấp, chỉ hồi khi f rỗi

        // ── Phase 2: HP < 50% → switch nguyên tố rồi mới dùng skill ──
        var phase2 = new Phase2Sequence();
        phase2.Name = "Phase 2 (Skill Attack)";
        phase2.CreateTasks(
            new IsPlayerDetected(),                     // Player detected?
            new IsHPBelow(0.5f),                        // check HP < 50%
            new SwitchElement(ElementType.Wind),        // đổi sang Wind nếu chưa
            new IsAttackReady(skill: true),             // cooldown skill ok? (5s)
            new ChasePlayer(),
            new IsPlayerInAttackRange(),                // trong tầm?
            new SkillAttack()                           // đánh skill (1.5x damage)
        );
        phase2.DefaultUtility = 15f;

        // ── Phase 1: đuổi → đánh thường ──
        var phase1Chase = new Sequence();
        phase1Chase.Name = "Chase + Attack (Normal)";
        phase1Chase.CreateTasks(
            new IsPlayerDetected(),         // Detect range (player detected?)
            new IsAttackReady(),            // cooldown thường ok? (2s)
            new ChasePlayer(),              // đuổi đến khi vào range
            new MeleeAttack()               // đánh thường
        );
        phase1Chase.DefaultUtility = 10f;    // Hạ hơn counterAttack để ưu tiên phản công

        // ── Idle fallback: wait for player detection ──
        var idle = new Idle();
        idle.Name = "Idle (Regenerate)";
        idle.DefaultUtility = 1f;

        // ── Root: UtilitySelector chọn hành vi có utility cao nhất ──
        // Priority: Die > Counter Attack > Hit Reaction > Phase 2 > Normal Attack > Regeneration > Idle
        var root = new UtilitySelector();
        root.Name = "Boss Map 2 Root (Smart & Regenerative)";
        root.CreateTasks(dieSequence, counterAttack, hitSequence, phase2, phase1Chase, regeneration, idle);

        return root;
    }
}
