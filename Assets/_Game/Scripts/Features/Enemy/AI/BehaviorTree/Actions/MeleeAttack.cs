using UnityEngine;
public class MeleeAttack : Action
{
    private BossBlackboard bb;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override void OnEntry()
    {
        timer = 0f;

        // Turn towards player
        Vector3 dir = (bb.player.position - Owner.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            Owner.transform.rotation = Quaternion.LookRotation(dir);

        // Play attack animation
        bb.PlayAnimation(BossAnimState.Attack);
        bb.lastAttackTime = Time.time;
        
        Debug.Log($"[MeleeAttack] ✓✓✓ TRIGGERED! Playing attack animation, applying damage in 0.8s");
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        Debug.Log($"[MeleeAttack] Timer: {timer:F2}/0.8s");
        
        if (timer >= 0.8f)
        {
            var playerHealth = bb.player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(bb.damageBoss, bb.currentElement);
                Debug.Log($"[MeleeAttack] ✓ Damage applied! Returning SUCCESS");
            }
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
}


