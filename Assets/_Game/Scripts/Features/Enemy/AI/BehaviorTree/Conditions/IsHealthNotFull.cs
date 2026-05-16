using UnityEngine;

public class IsHealthNotFull : Task
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        if (bb.healthSystem == null)
            return TaskStatus.Failure;

        // Kiểm tra nếu máu không full (so sánh với max health)
        bool isNotFull = bb.healthSystem.CurrentHealth < bb.healthSystem.MaxHealth;
        
        var result = isNotFull ? TaskStatus.Success : TaskStatus.Failure;
        //Debug.Log($"[IsHealthNotFull] Current: {bb.healthSystem.CurrentHealth}, Max: {bb.healthSystem.MaxHealth}, IsNotFull: {isNotFull}");
        
        return result;
    }
}
