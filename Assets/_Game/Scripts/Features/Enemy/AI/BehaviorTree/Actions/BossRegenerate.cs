using UnityEngine;

public class BossRegenerate : ActionNode
{
    private BossBlackboard bb;
    private float regenerationRate = 0.02f;  // Hồi 2% mỗi frame
    private float regenerationTimer = 0f;
    private float regenerationInterval = 0.5f;  // Hồi mỗi 0.5s để tránh quá nhanh

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override void OnEntry()
    {
        regenerationTimer = 0f;
        //Debug.Log("[BossRegenerate] ✓ Started regeneration");
    }

    protected override TaskStatus OnUpdate()
    {
        if (bb.healthSystem == null)
            return TaskStatus.Failure;

        regenerationTimer += Time.deltaTime;

        // Hồi máu mỗi interval
        if (regenerationTimer >= regenerationInterval)
        {
            float regenAmount = bb.healthSystem.MaxHealth * regenerationRate;
            bb.healthSystem.Recover(regenAmount);
            regenerationTimer = 0f;

            //Debug.Log($"[BossRegenerate] Healed: +{regenAmount}, Current: {bb.healthSystem.CurrentHealth}/{bb.healthSystem.MaxHealth}");
        }

        // Nếu máu đã full, dừng hồi
        if (bb.healthSystem.CurrentHealth >= bb.healthSystem.MaxHealth)
        {
            //Debug.Log("[BossRegenerate] ✓ Health is full, stopping regeneration");
            return TaskStatus.Success;
        }

        // Tiếp tục hồi nếu còn có thời gian
        return TaskStatus.Running;
    }

    protected override void OnExit()
    {
        //Debug.Log("[BossRegenerate] Exit regeneration");
    }
}
