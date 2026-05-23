using UnityEngine;

public class SkillAttack : ActionNode
{
    private BossBlackboard bb;
    private float skillDamageMultiplier = 2.5f;  // Skill damage mạnh hơn để đánh đứng im

    private void RestoreAgent()
    {
        if (bb == null || bb.agent == null)
            return;

        bb.agent.enabled = true;
        bb.agent.isStopped = false;
        bb.agent.Warp(Owner.transform.position);
    }

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
        
        // Disable NavMeshAgent để update vị trí ngay lập tức thay vì qua velocity
        if (bb.agent != null)
            bb.agent.enabled = false;
        
        //Debug.Log($"[Boss] Skill Attack triggered! Element: {bb.currentElement}");
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.deltaTime;

        if (bb.animator == null)
            return TaskStatus.Running;

        if (bb.player == null)
        {
            RestoreAgent();
            return TaskStatus.Failure;
        }

        // Nếu player đã ra khỏi tầm đánh thì hủy skill để cây hành vi quay lại chase
        if (!bb.CanAttackPlayer())
        {
            RestoreAgent();
            return TaskStatus.Failure;
        }

        var state = bb.animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("Skill") || state.normalizedTime < 1f)
            return TaskStatus.Running;

        RestoreAgent();
        
        // Skill damage hit ở frame 1.0s (lâu hơn attack thường)
        var playerHealth = bb.player.GetComponent<HealthSystem>();
        if (playerHealth != null)
        {
            float damage = bb.healthSystem.GetDamage() * skillDamageMultiplier;
            playerHealth.TakeDamage(damage, bb.currentElement);
            
            //Debug.Log($"[Boss] Skill hit player! Damage: {damage}");
        }

        return TaskStatus.Success;
    }

    protected override void OnExit()
    {
        // Re-enable NavMeshAgent khi skill kết thúc hoặc bị hủy giữa chừng
        RestoreAgent();
    }
}