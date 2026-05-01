using UnityEngine;

public class IsHit : Task
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        var result = bb.isHit ? TaskStatus.Success : TaskStatus.Failure;
        //Debug.Log($"[IsHit] isHit: {bb.isHit}, Result: {result}");
        return result;
    }
}
