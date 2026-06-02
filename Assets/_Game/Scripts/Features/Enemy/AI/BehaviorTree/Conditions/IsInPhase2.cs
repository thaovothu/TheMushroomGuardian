using UnityEngine;


public class IsInPhase2 : Task
{
    private BossModelSwitcher switcher;

    protected override void OnAwake()
    {
        switcher = Owner.GetComponent<BossModelSwitcher>();
    }

    protected override TaskStatus OnUpdate()
    {
        if (switcher == null) return TaskStatus.Failure;
        return switcher.IsInPhase2 ? TaskStatus.Success : TaskStatus.Failure;
    }
}