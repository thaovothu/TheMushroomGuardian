using UnityEngine;

/// <summary>
/// Phase 1 (HP > 50%): chase + đánh gần Attack01/02, tốc độ +30%.
/// </summary>
public class Phase1MeleeSequence : Sequence
{
    private BossBlackboard bb;
    private const float SPEED_BOOST = 1.3f;
    private bool speedApplied;

    public override void SetOwner(GameObject owner)
    {
        base.SetOwner(owner);
        bb = owner.GetComponent<BossBlackboard>();
    }

    public override float GetUtility()
    {
        if (bb == null) return 0f;

        var switcher = bb.GetComponent<BossModelSwitcher>();
        if (switcher != null && (switcher.IsInPhase2 || switcher.IsTransitioning)) return 0f;

        if (!bb.CanDetectPlayer()) return 0f;

        // Apply speed boost 1 lần khi vào phase 1
        if (!speedApplied)
        {
            bb.agent.speed = bb.moveSpeed * SPEED_BOOST;
            speedApplied = true;
            Debug.Log($"[Phase1MeleeSequence] Speed boost x{SPEED_BOOST} applied: {bb.agent.speed}");
        }

        return base.GetUtility() + 10f;
    }
}