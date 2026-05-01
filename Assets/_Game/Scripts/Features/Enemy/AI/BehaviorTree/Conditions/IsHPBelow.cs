using UnityEngine;
public class IsHPBelow : Task
{
    private BossBlackboard bb;
    private float threshold; // 0..1

    public IsHPBelow(float threshold) => this.threshold = threshold;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        float hpPercent = bb.healthSystem.GetHPPercent();
        var result = hpPercent < threshold ? TaskStatus.Success : TaskStatus.Failure;
        // UnityEngine.//Debug.Log($"[IsHPBelow] HPPercent: {hpPercent:F2}, Threshold: {threshold}, Result: {result}");
        return result;
    }
}
