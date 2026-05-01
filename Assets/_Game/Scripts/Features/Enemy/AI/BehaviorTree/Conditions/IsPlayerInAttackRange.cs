using UnityEngine;

public class IsPlayerInAttackRange : Task
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        var result = bb.CanAttackPlayer() ? TaskStatus.Success : TaskStatus.Failure;
        //Debug.Log($"[IsPlayerInAttackRange] Distance: {bb.distanceToPlayer:F1}m, AttackRange: {bb.attackRange}, Result: {result}");
        return result;
    }
}
