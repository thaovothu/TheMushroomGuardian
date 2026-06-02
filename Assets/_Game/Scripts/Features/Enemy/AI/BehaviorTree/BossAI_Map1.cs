using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/Boss/Map1")]
public class BossAI_Map1 : ExternalBehaviorTree
{
    public override Task CreateBehaviorTree()
    {
        // ── DIE ──
        var dieSequence = new DieSequence();
        dieSequence.Name = "Die";
        dieSequence.CreateTasks(
            new IsHealthZero(),
            new BossDieAction()
        );

        // ── Phase 2: HP < 50% → switch element + skill ──
        var phase2 = new Phase2Sequence();
        phase2.Name = "Phase 2 (Skill Attack)";
        phase2.CreateTasks(
            new IsPlayerDetected(),
            new IsHPBelow(0.5f),
            new SwitchElement(ElementType.Wind),
            new IsAttackReady(skill: true),
            new ChasePlayer(),
            new IsPlayerInAttackRange(),
            new SkillAttack()
        );

        // ── Phase 1: Chase + Attack thường ──
        var phase1Chase = new Sequence();
        phase1Chase.Name = "Chase + Attack";
        phase1Chase.CreateTasks(
            new IsPlayerDetected(),
            new IsAttackReady(),
            new ChasePlayer(),
            new MeleeAttack()
        );
        phase1Chase.DefaultUtility = 5f;

        // ── Idle fallback ──
        var idle = new Idle();
        idle.Name = "Idle";
        idle.DefaultUtility = 1f;

        // Priority: Die (999) > Phase 2 (10) > Phase 1 (5) > Idle (1)
        var root = new UtilitySelector();
        root.Name = "Boss Root";
        root.CreateTasks(dieSequence, phase2, phase1Chase, idle);

        return root;
    }
}