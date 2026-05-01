using UnityEngine;

/// <summary>
/// Hit Sequence - chỉ có utility khi Boss bị hit (isHit = true)
/// Nếu không bị hit, utility = 0 để Phase 1 được chọn
/// </summary>
public class HitSequence : Sequence
{
    private BossBlackboard bb;

    public override void SetOwner(GameObject owner)
    {
        base.SetOwner(owner);
        bb = owner.GetComponent<BossBlackboard>();
    }

    public override float GetUtility()
    {
        if (bb == null)
        {
            //Debug.LogError("[HitSequence] BossBlackboard is NULL!");
            return 0f;
        }

        // Nếu không bị hit, hit sequence không khả thi → utility = 0
        if (!bb.isHit)
        {
            return 0f;
        }

        // Nếu bị hit, có utility cao (20) để interrupt hành động khác
        float utility = 20f;
        //Debug.Log($"[HitSequence] ✓ isHit detected! Returning utility: {utility}");
        return utility;
    }
}
