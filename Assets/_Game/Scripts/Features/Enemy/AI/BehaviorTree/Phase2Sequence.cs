using UnityEngine;

/// <summary>
/// Phase 2 Sequence - chỉ có utility cao khi HP < 50%
/// Nếu HP >= 50%, utility = 0 để Phase 1 được chọn
/// </summary>
public class Phase2Sequence : Sequence
{
    private BossBlackboard bb;

    public override void SetOwner(GameObject owner)
    {
        base.SetOwner(owner);
        bb = owner.GetComponent<BossBlackboard>();
    }

    public override float GetUtility()
    {
        // Nếu HP >= 50%, phase 2 không khả thi → utility = 0
        if (bb != null && bb.healthSystem.GetHPPercent() >= 0.5f)
        {
            return 0f;
        }

        // Nếu HP < 50%, phase 2 có utility cao
        return base.GetUtility() + 10f;  // DefaultUtility = 10
    }
}
