using UnityEngine;

public class IsHealthZero : Task
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        var result = bb.healthSystem.IsDead ? TaskStatus.Success : TaskStatus.Failure;
        Debug.Log($"[IsHealthZero] CurrentHealth: {bb.healthSystem.CurrentHealth}, IsDead: {bb.healthSystem.IsDead}, Result: {result}");
        return result;
    }
}
