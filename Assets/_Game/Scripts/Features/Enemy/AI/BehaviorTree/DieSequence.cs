using UnityEngine;

/// <summary>
/// Die Sequence - chỉ có utility khi Boss chết (IsDead = true)
/// Nếu còn sống, utility = 0 để các behavior khác được chọn
/// </summary>
public class DieSequence : Sequence
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
            //Debug.LogError("[DieSequence] BossBlackboard is NULL!");
            return 0f;
        }

        // Nếu chưa chết, utility = 0 (không chọn die sequence)
        if (!bb.healthSystem.IsDead)
        {
            return 0f;
        }

        // Nếu chết, có utility cao nhất (999) để interrupt tất cả
        float utility = 999f;
        //Debug.Log($"[DieSequence] ✓ IsDead detected! Returning utility: {utility}");
        return utility;
    }
}
