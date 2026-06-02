using UnityEngine;

/// <summary>
/// Phase 2 Attack03 — bắn 1 viên đạn thẳng về phía player.
/// Tương tự MeleeAttack nhưng spawn projectile thay vì check khoảng cách.
/// </summary>
public class RangedAttack03 : ActionNode
{
    private BossBlackboard bb;

    [Header("Projectile")]
    // Prefab assign qua BossBlackboard.rangedProjectilePrefab (set trong Inspector boss)
    private float attackDelay = 0.6f;   // giây chờ animation trước khi bắn

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override void OnEntry()
    {
        timer = 0f;

        // Quay về phía player
        Vector3 dir = (bb.player.position - Owner.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            Owner.transform.rotation = Quaternion.LookRotation(dir);

        bb.PlayAnimation(BossAnimState.Attack01);
        bb.lastAttackTime = Time.time;
        Debug.Log("[RangedAttack03] Entry — playing Attack01 anim");
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer < attackDelay) return TaskStatus.Running;

        // Spawn projectile
        if (bb.rangedProjectilePrefab != null && bb.player != null)
        {
            Vector3 spawnPos = Owner.transform.position + Vector3.up * 1.5f;
            Vector3 dir = (bb.player.position + Vector3.up * 1f - spawnPos).normalized;

            var go = Object.Instantiate(bb.rangedProjectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            var proj = go.GetComponent<BossProjectile>();
            if (proj != null)
                proj.Initialize(dir, bb.ResolveDamageVsPlayer(bb.damageBoss));
            else
                Debug.LogWarning("[RangedAttack03] BossProjectile component không tìm thấy trên prefab!");

            Debug.Log($"[RangedAttack03] Projectile spawned → {dir}");
        }
        else
        {
            Debug.LogWarning("[RangedAttack03] rangedProjectilePrefab chưa assign trong BossBlackboard!");
        }

        return TaskStatus.Success;
    }
}