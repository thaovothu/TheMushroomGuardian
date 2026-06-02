using UnityEngine;

/// <summary>
/// Linh hồn dash ra xa khỏi player khi player áp sát.
/// Tận dụng NavMeshAgent có sẵn của boss.
/// </summary>
public class SoulKiteAway : ActionNode
{
    private BossBlackboard bb;
    private float kiteDistance = 6f;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        if (bb == null || bb.player == null || bb.agent == null)
            return TaskStatus.Failure;

        // Tính hướng ngược lại player
        Vector3 awayDir = (bb.transform.position - bb.player.position).normalized;
        Vector3 kiteTarget = bb.transform.position + awayDir * kiteDistance;

        // Sample NavMesh để chắc chắn điểm target hợp lệ
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(kiteTarget, out hit, kiteDistance, UnityEngine.AI.NavMesh.AllAreas))
        {
            bb.agent.SetDestination(hit.position);
            bb.PlayAnimation(BossAnimState.Run);
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}