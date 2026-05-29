using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAttackState : EnemyBaseState
{
    readonly NavMeshAgent agent;
    readonly Transform player;
    public EnemyAttackState(EnemyController enemyController, Animator animator, NavMeshAgent agent, Transform player) : base(enemyController, animator)
    {
        this.agent = agent;
        this.player = player;
    }
    // Tốc độ quay mặt mượt vào player khi đánh (Slerp factor).
    const float ROTATION_SPEED = 10f;

    public override void OnEnter()
    {
        _animator.CrossFade(AttackHash, crossFadeDuration);
        // Dừng agent khi đánh để khỏi "slide" trong lúc chơi animation.
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = true;
    }

    public override void OnExit()
    {
        // Bật lại agent để Chase/Wander tiếp theo di chuyển bình thường ngay.
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    public override void Update()
    {
        if (IsRangedEnemy())
        {
            float dist = Vector3.Distance(_enemyController.transform.position, player.position);
            if (dist > _enemyController.AttackRange)
            {
                // Player tạm ra xa → bật agent để áp sát.
                if (agent.isOnNavMesh) agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                // Trong tầm bắn → đứng yên, xoay mặt vào player.
                if (agent.isOnNavMesh) agent.isStopped = true;
                FacePlayer();
            }
            // Damage được áp qua Animation Event (EnemyAnimationEventHandler.OnAttack).
        }
        else
        {
            // Melee: đứng yên (isStopped đã set ở OnEnter), quay mặt vào player,
            // gọi Attack() — bên trong throttle bằng _attackTimer và re-trigger anim.
            FacePlayer();
            _enemyController.Attack();
        }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - _enemyController.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        var targetRot = Quaternion.LookRotation(dir);
        _enemyController.transform.rotation = Quaternion.Slerp(
            _enemyController.transform.rotation,
            targetRot,
            ROTATION_SPEED * Time.deltaTime);
    }

    bool IsRangedEnemy() => _enemyController.EnemyType == EnemyAttackType.Ranged;
}

