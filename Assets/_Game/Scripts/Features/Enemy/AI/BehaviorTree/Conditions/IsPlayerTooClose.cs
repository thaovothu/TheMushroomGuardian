using UnityEngine;

/// <summary>
/// True khi player lại gần boss hơn threshold.
/// Dùng để trigger BurstBulletAction phase 2.
/// </summary>
public class IsPlayerTooClose : Task
{
    private BossBlackboard bb;
    private float threshold;

    public IsPlayerTooClose(float threshold = 4f) => this.threshold = threshold;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        if (bb == null || bb.player == null) return TaskStatus.Failure;
        return bb.distanceToPlayer <= threshold ? TaskStatus.Success : TaskStatus.Failure;
    }
}