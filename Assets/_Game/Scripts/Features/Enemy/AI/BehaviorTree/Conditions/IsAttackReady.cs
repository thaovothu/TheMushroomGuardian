using UnityEngine;

public class IsAttackReady : Task
{
    private BossBlackboard bb;
    private bool useSkillCooldown;

    public IsAttackReady(bool skill = false) => useSkillCooldown = skill;

    protected override void OnAwake()
    {
        bb = Owner.GetComponent<BossBlackboard>();
    }

    protected override TaskStatus OnUpdate()
    {
        float cooldown = useSkillCooldown ? bb.skillCooldown : bb.attackCooldown;
        float lastTime = useSkillCooldown ? bb.lastSkillTime : bb.lastAttackTime;
        float elapsed = Time.time - lastTime;
        var result = elapsed >= cooldown ? TaskStatus.Success : TaskStatus.Failure;
        
        Debug.Log($"[IsAttackReady] Skill={useSkillCooldown}, Elapsed={elapsed:F2}s, Cooldown={cooldown}s, LastTime={lastTime:F2}, Now={Time.time:F2}, Result={result}");
        
        return result;
    }
}
