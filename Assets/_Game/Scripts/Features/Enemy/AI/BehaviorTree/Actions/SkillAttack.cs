using UnityEngine;

public class SkillAttack : Action
{
    private BossBlackboard bb;
    private float skillDamageMultiplier = 1.5f;  // Skill 1.5x damage

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

        // Play skill animation
        bb.PlayAnimation(BossAnimState.Skill);
        bb.lastSkillTime = Time.time;
        
        //Debug.Log($"[Boss] Skill Attack triggered! Element: {bb.currentElement}");
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;
        
        // Skill damage hit ở frame 1.0s (lâu hơn attack thường)
        if (timer >= 1.0f)
        {
            var playerHealth = bb.player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                float damage = bb.healthSystem.GetDamage() * skillDamageMultiplier;
                playerHealth.TakeDamage(damage, bb.currentElement);
                
                //Debug.Log($"[Boss] Skill hit player! Damage: {damage}");
            }

            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }
}