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

        Debug.Log($"[ChasePlayer] Distance: {bb.distanceToPlayer:F2}, Speed: {bb.agent.speed}, AttackRange: {bb.attackRange}");

        // Khi đã vào attack range thì dừng chase → Success để Sequence chuyển sang Attack
        if (bb.distanceToPlayer <= bb.attackRange)
        {
            bb.agent.isStopped = true;
            Debug.Log($"[ChasePlayer] In attack range! Stopping.");
            return TaskStatus.Success;
        }

        // Nếu player quá xa thì bỏ đuổi
        if (bb.distanceToPlayer > bb.detectRange)
        {
            Debug.Log($"[ChasePlayer] Player too far! Stopping chase.");
            return TaskStatus.Failure;
        }
        
        Debug.Log($"[ChasePlayer] Chasing... running.");
        return TaskStatus.Running;
    }

    protected override void OnExit()
    {
        bb.PlayAnimation(BossAnimState.Idle);
    }
}
