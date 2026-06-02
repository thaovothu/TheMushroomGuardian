using UnityEngine;

/// <summary>
/// Combo attack thứ 2 — gọi animation attack2 thay vì attack.
/// </summary>
public class MeleeAttack2 : ActionNode
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        if (bb == null || bb.player == null) return TaskStatus.Failure;

        // Quay mặt về player
        Vector3 dir = (bb.player.position - bb.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            bb.transform.rotation = Quaternion.Slerp(bb.transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 10f);

        bb.PlayAnimation(BossAnimState.Attack02);
        bb.lastAttackTime = Time.time;

        // Gây damage qua HealthSystem player
        var playerHealth = bb.player.GetComponent<HealthSystem>();
        if (playerHealth != null)
            playerHealth.TakeDamage(bb.ResolveDamageVsPlayer(bb.damageBoss), bb.currentElement);

        return TaskStatus.Success;
    }
}