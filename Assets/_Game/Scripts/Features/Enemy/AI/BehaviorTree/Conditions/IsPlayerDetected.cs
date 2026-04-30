using UnityEngine;

public class IsPlayerDetected : Task
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        var result = bb.CanDetectPlayer() ? TaskStatus.Success : TaskStatus.Failure;
        Debug.Log($"[IsPlayerDetected] Distance: {bb.distanceToPlayer:F1}m, DetectRange: {bb.detectRange}m, Can Detect: {bb.CanDetectPlayer()}, Result: {result}");
        return result;
    }
}
