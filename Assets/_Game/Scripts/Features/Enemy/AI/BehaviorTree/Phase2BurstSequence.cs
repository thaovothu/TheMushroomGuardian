using UnityEngine;

/// <summary>
/// Phase 2 — khi player lại gần quá (burstRange), boss xả vòng đạn tròn.
/// Utility cao hơn Phase2RangedSequence để ưu tiên khi player lao vào.
/// </summary>
public class Phase2BurstSequence : Sequence
{
    private BossBlackboard bb;
    private BurstBulletAction burstAction;
    private float burstRange;

    public Phase2BurstSequence(float burstRange = 4f)
    {
        this.burstRange = burstRange;
    }

    public override void SetOwner(GameObject owner)
    {
        base.SetOwner(owner);
        bb = owner.GetComponent<BossBlackboard>();
        // Tìm BurstBulletAction trong children của sequence để check cooldown
        // (được set khi CreateTasks gọi)
    }

    public override float GetUtility()
    {
        if (bb == null) return 0f;

        var switcher = bb.GetComponent<BossModelSwitcher>();
        if (switcher == null || !switcher.IsInPhase2 || switcher.IsTransitioning) return 0f;

        if (bb.distanceToPlayer > burstRange) return 0f;

        // Lazy-find BurstBulletAction từ children để check cooldown
        if (burstAction == null)
        {
            foreach (var task in tasks)
                if (task is BurstBulletAction ba) { burstAction = ba; break; }
        }
        if (burstAction != null && !burstAction.IsReady()) return 0f;

        return base.GetUtility() + 16f;
    }
}