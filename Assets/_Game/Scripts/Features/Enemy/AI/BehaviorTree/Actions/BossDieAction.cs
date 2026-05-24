using UnityEngine;

public class BossDieAction : ActionNode
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

        if (timer >= dieDuration)
        {
            // Dùng bossBaseElement thay vì currentElement
            ItemDropManager.Instance?.DropItemsOnBossDeath(bb.transform.position, bb.bossBaseElement);
            QuestSpawnManager.Instance?.NotifySpawnedEnemyDied(bb.gameObject);
            PoolSpawnManager.Instance.OnRelease?.Invoke(bb.gameObject);
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    protected override void OnExit()
    {
        // Bỏ agent.Resume() vì boss đã chết, agent không còn active
        // bb.agent.isStopped = false; ← đây là nguyên nhân warning "Resume on inactive agent"
    }
}
