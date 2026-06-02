using UnityEngine;

/// <summary>
/// Sequence cho 1 phase của boss cuối (BigBoss).
/// Chỉ có utility > 0 khi BigBossModelSwitcher đang ở đúng hệ (element) và không đang transition.
/// Nhờ guard này, 4 nhánh phase không bao giờ chồng nhau — đúng 1 phase active theo %máu.
///
/// Chỉ phụ thuộc BigBossModelSwitcher → KHÔNG ảnh hưởng các boss cũ (dùng BossModelSwitcher).
/// </summary>
public class BigBossPhaseSequence : Sequence
{
    private BossBlackboard bb;
    private BigBossModelSwitcher switcher;
    private readonly ElementType phaseElement;
    private readonly float bonusUtility;

    public BigBossPhaseSequence(ElementType phaseElement, float bonusUtility = 10f)
    {
        this.phaseElement = phaseElement;
        this.bonusUtility = bonusUtility;
    }

    public override void SetOwner(GameObject owner)
    {
        base.SetOwner(owner);
        bb = owner.GetComponent<BossBlackboard>();
        switcher = owner.GetComponent<BigBossModelSwitcher>();
    }

    public override float GetUtility()
    {
        if (bb == null || switcher == null) return 0f;
        if (switcher.IsTransitioning) return 0f;            // đang đổi model → đứng yên
        if (switcher.CurrentElement != phaseElement) return 0f; // không phải phase của mình
        if (!bb.CanDetectPlayer()) return 0f;

        return base.GetUtility() + bonusUtility;
    }
}
