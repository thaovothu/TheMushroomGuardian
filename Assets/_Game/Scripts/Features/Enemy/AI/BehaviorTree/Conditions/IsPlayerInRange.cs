//new 29/6/2024
using UnityEngine;
public class IsPlayerInRange : Task
{
    private BossBlackboard bb;
    private float range;

    public IsPlayerInRange(float range) => this.range = range;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        var result = bb.distanceToPlayer <= range ? TaskStatus.Success : TaskStatus.Failure;
        Debug.Log($"[IsPlayerInRange] Distance: {bb.distanceToPlayer}, Range: {range}, Result: {result}");
        return result;
    }
}
