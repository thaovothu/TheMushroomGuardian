using UnityEngine;

/// <summary>
/// Phase 2 — khi player lại gần (distanceToPlayer <= burstTriggerRange),
/// spawn 1 vòng đạn đều quanh boss theo hình tròn.
/// Cooldown riêng để không spam.
/// </summary>
public class BurstBulletAction : ActionNode
{
    private BossBlackboard bb;

    private float burstCooldown = 6f;
    private float lastBurstTime = -99f;
    private int bulletCount = 12;     // số viên trong vòng tròn
    private float burstDamageRatio = 0.6f;   // % damageBoss mỗi viên

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override void OnEntry()
    {
        timer = 0f;
        // Dừng boss lại 0.3s để animation bắn
        if (bb.agent != null) bb.agent.isStopped = true;
        Debug.Log("[BurstBulletAction] Entry — spawning burst ring");
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer < 0.3f) return TaskStatus.Running;  // chờ nhỏ để sync anim

        SpawnBurstRing();
        lastBurstTime = Time.time;

        if (bb.agent != null) bb.agent.isStopped = false;
        return TaskStatus.Success;
    }

    protected override void OnExit()
    {
        if (bb.agent != null) bb.agent.isStopped = false;
    }

    void SpawnBurstRing()
    {
        if (bb.rangedProjectilePrefab == null)
        {
            Debug.LogWarning("[BurstBulletAction] rangedProjectilePrefab chưa assign!");
            return;
        }

        Vector3 origin = Owner.transform.position + Vector3.up * 1.2f;
        float dmg = bb.damageBoss * burstDamageRatio;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            var go = Object.Instantiate(bb.rangedProjectilePrefab, origin, Quaternion.LookRotation(dir));
            var proj = go.GetComponent<BossProjectile>();
            proj?.Initialize(dir, dmg);
        }

        Debug.Log($"[BurstBulletAction] Spawned {bulletCount} bullets in ring — dmg each={dmg:F1}");
    }

    /// <summary>
    /// BurstBulletSequence.GetUtility() gọi method này để check cooldown.
    /// </summary>
    public bool IsReady() => Time.time - lastBurstTime >= burstCooldown;
}