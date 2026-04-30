using UnityEngine;
using UnityEngine.AI;

public class ChasePlayer : Action
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override void OnEntry()
    {
        bb.agent.isStopped = false;
    }

    protected override TaskStatus OnUpdate()
    {
        if (bb.player == null) return TaskStatus.Failure;

        bb.agent.SetDestination(bb.player.position);
        
        // Choose Run or Walk based on distance, and set speed accordingly
        if (bb.distanceToPlayer > 10f)
        {
            bb.PlayAnimation(BossAnimState.Run);
            bb.agent.speed = bb.runSpeed;
        }
        else
        {
            bb.PlayAnimation(BossAnimState.Walk);
            bb.agent.speed = bb.moveSpeed;
        }

        Debug.Log($"[ChasePlayer] Distance: {bb.distanceToPlayer:F2}m, Speed: {bb.agent.speed:F2}, AttackRange: {bb.attackRange:F2}m");

        // Khi đã vào attack range thì dừng chase → Success để Sequence chuyển sang Attack
        if (bb.distanceToPlayer <= bb.attackRange)
        {
            bb.agent.isStopped = true;
            Debug.Log($"[ChasePlayer] ✓ IN ATTACK RANGE! Returning SUCCESS → MeleeAttack will trigger");
            return TaskStatus.Success;
        }

        // Nếu player quá xa thì bỏ đuổi
        if (bb.distanceToPlayer > bb.detectRange)
        {
            Debug.Log($"[ChasePlayer] ✗ Player too far ({bb.distanceToPlayer:F2}m > {bb.detectRange}m)! Returning FAILURE");
            return TaskStatus.Failure;
        }

        Debug.Log("Speedis"+ bb.agent.speed);
        
        Debug.Log($"[ChasePlayer] Chasing... distance {bb.distanceToPlayer:F2}m, need to reach {bb.attackRange:F2}m");
        return TaskStatus.Running;
    }

    protected override void OnExit()
    {
        bb.PlayAnimation(BossAnimState.Idle);
    }
}
