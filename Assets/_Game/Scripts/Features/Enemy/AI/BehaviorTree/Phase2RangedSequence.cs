using UnityEngine;

/// <summary>
/// Phase 2 (HP <= 50%): đánh xa Attack03/04.
/// Tốc độ trả về mức bình thường (BossModelSwitcher đã xử lý khi transition).
/// </summary>
public class Phase2RangedSequence : Sequence
{
    private BossBlackboard bb;

    public override void SetOwner(GameObject owner)
    {
        base.SetOwner(owner);
        bb = owner.GetComponent<BossBlackboard>();
    }

    public override float GetUtility()
    {
        if (bb == null) return 0f;

        var switcher = bb.GetComponent<BossModelSwitcher>();
        if (switcher == null || !switcher.IsInPhase2 || switcher.IsTransitioning) return 0f;

        if (!bb.CanDetectPlayer()) return 0f;

        return base.GetUtility() + 12f;
    }
}