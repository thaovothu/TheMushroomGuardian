using UnityEngine;

public class BossDieAction : Action
{
    private BossBlackboard bb;
    private float dieDuration = 2f;  // Time before boss is removed

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override void OnEntry()
    {
        timer = 0f;
        bb.PlayAnimation(BossAnimState.Die);
        bb.agent.isStopped = true;
        //Debug.Log($"[BossDieAction] ✓✓✓ DIE ACTION TRIGGERED! Playing die animation");
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        //Debug.Log($"[BossDieAction] Timer: {timer:F2}/{dieDuration}s");
        
        // After die duration, boss is removed (or respawned)
        if (timer >= dieDuration)
        {
            //Debug.Log($"[BossDieAction] Die animation finished! Removing boss");
            // Call death handler (e.g., PoolSpawnManager.OnRelease)
            PoolSpawnManager.OnRelease?.Invoke(bb.gameObject);
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    protected override void OnExit()
    {
        bb.agent.isStopped = false;
        //Debug.Log("[BossDieAction] Die action exited");
    }
}
