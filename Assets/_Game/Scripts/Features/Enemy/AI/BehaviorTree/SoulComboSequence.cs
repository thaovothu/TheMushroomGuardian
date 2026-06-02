using UnityEngine;

/// <summary>
/// Soul phase 1A: HP > 70%, chỉ dùng Attack1 + Attack2 combo.
/// </summary>
public class SoulComboSequence : Sequence
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
        if (switcher != null && switcher.IsInPhase2) return 0f;

        // Active khi HP > 70% (phase 1A)
        float hp = bb.healthSystem.GetHPPercent();
        if (hp <= 0.7f) return 0f;

        return base.GetUtility() + 14f;
    }
}

/// <summary>
/// Soul phase 1B: 50% < HP ≤ 70%, dùng Attack1 + Attack2 + Skill.
/// </summary>
public class SoulFullKitSequence : Sequence
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
        if (switcher != null && switcher.IsInPhase2) return 0f;

        float hp = bb.healthSystem.GetHPPercent();
        if (hp > 0.7f || hp <= 0.5f) return 0f;

        return base.GetUtility() + 16f;
    }
}

/// <summary>
/// Giant phase 2A: 20% < HP ≤ 50%, Attack1 + Attack2 combo (chưa Last Stand).
/// </summary>
public class GiantComboSequence : Sequence
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
        if (switcher == null || !switcher.IsInPhase2) return 0f;

        float hp = bb.healthSystem.GetHPPercent();
        if (hp <= 0.2f) return 0f;

        return base.GetUtility() + 14f;
    }
}

/// <summary>
/// Giant phase 2B (Last Stand): HP ≤ 20%, Attack1 + Attack2 + Skill berserker.
/// </summary>
public class GiantLastStandSequence : Sequence
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
        if (switcher == null || !switcher.IsInPhase2) return 0f;

        float hp = bb.healthSystem.GetHPPercent();
        if (hp > 0.2f) return 0f;

        return base.GetUtility() + 18f; // ưu tiên cao nhất khi sắp chết
    }
}