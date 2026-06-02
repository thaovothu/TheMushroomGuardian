using UnityEngine;

/// <summary>
/// Counter Attack Sequence — utility động:
///  - Chỉ có utility cao khi boss VỪA BỊ HIT VÀ player đang trong tầm đánh.
///  - Các trường hợp khác → utility = 0 để UtilitySelector chuyển sang nhánh khác
///    (Chase + Attack, Hit out-of-range, v.v.).
///
/// Phải dùng class này thay cho Sequence thường, vì Sequence thường có utility TĨNH
/// (DefaultUtility) → luôn được UtilitySelector chọn → fail ở IsHit → vòng lặp,
/// boss đứng yên.
/// </summary>
public class CounterAttackSequence : Sequence
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
        // Player ngoài tầm đánh → để HitSequence (out-of-range) xử lý.
        if (!bb.CanAttackPlayer()) return 0f;

        // Đủ điều kiện phản công ngay.
        return 25f;
    }
}
