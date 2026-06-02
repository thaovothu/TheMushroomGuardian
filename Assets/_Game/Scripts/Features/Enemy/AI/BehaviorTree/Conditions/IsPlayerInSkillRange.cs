using UnityEngine;

public class IsPlayerInSkillRange : Task
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        if (bb == null || bb.player == null) return TaskStatus.Failure;
        return bb.distanceToPlayer <= bb.skillRange
            ? TaskStatus.Success
            : TaskStatus.Failure;
    }
}