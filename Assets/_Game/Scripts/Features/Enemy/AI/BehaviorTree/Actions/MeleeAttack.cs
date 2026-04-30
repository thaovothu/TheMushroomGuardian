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
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= 0.8f)
        {
            var playerHealth = bb.player.GetComponent<HealthSystem>();
            if (playerHealth != null)
                playerHealth.TakeDamage(bb.healthSystem.GetDamage(), bb.currentElement);

            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }
}


