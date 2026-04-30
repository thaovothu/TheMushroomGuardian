using UnityEngine;

public class BossHitAction : Action
{
    private BossBlackboard bb;
    private float hitDuration = 1f;  // Duration of hit animation

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override void OnEntry()
    {
        timer = 0f;
        bb.PlayAnimation(BossAnimState.Hit);
        bb.agent.isStopped = true;
        Debug.Log($"[BossHitAction] ✓✓✓ HIT ACTION TRIGGERED! Animation: {BossAnimState.Hit}, Agent stopped: {bb.agent.isStopped}");
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        Debug.Log($"[BossHitAction] Timer: {timer:F2}/{hitDuration}");
        
        // After hit duration, exit hit state
        if (timer >= hitDuration)
        {
            bb.isHit = false;
            bb.agent.isStopped = false;
            Debug.Log($"[BossHitAction] Hit duration finished! Exiting...");
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    protected override void OnExit()
    {
        bb.isHit = false;  // Reset hit flag when exiting hit state
        bb.agent.isStopped = false;
        Debug.Log("[BossHitAction] ✓✓✓ Hit finished, resetting isHit flag, agent resuming");
    }
}
