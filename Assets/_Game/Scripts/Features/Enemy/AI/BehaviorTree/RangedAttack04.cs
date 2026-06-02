using UnityEngine;

/// <summary>
/// Phase 2 Attack04 — bắn 3 viên đạn spread (trái / giữa / phải).
/// Dùng làm combo nối sau Attack03.
/// </summary>
public class RangedAttack04 : ActionNode
{
    private BossBlackboard bb;

    private float attackDelay = 0.5f;
    private float spreadAngle = 20f;   // độ lệch mỗi bên

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override void OnEntry()
    {
        timer = 0f;

        Vector3 dir = (bb.player.position - Owner.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            Owner.transform.rotation = Quaternion.LookRotation(dir);

        bb.PlayAnimation(BossAnimState.Attack02);
        Debug.Log("[RangedAttack04] Entry — playing Attack02 anim (spread shot)");
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer < attackDelay) return TaskStatus.Running;

        if (bb.rangedProjectilePrefab != null && bb.player != null)
        {
            Vector3 spawnPos = Owner.transform.position + Vector3.up * 1.5f;
            Vector3 centerDir = (bb.player.position + Vector3.up * 1f - spawnPos).normalized;

            // 3 hướng: trái, giữa, phải
            Vector3[] dirs = new Vector3[]
            {
                Quaternion.Euler(0, -spreadAngle, 0) * centerDir,
                centerDir,
                Quaternion.Euler(0,  spreadAngle, 0) * centerDir,
            };

            foreach (var d in dirs)
            {
                var go = Object.Instantiate(bb.rangedProjectilePrefab, spawnPos, Quaternion.LookRotation(d));
                var proj = go.GetComponent<BossProjectile>();
                proj?.Initialize(d, bb.ResolveDamageVsPlayer(bb.damageBoss) * 0.8f); // spread shot nhẹ hơn
            }

            Debug.Log("[RangedAttack04] Spread 3 projectiles fired");
        }
        else
        {
            Debug.LogWarning("[RangedAttack04] rangedProjectilePrefab chưa assign!");
        }

        return TaskStatus.Success;
    }
}