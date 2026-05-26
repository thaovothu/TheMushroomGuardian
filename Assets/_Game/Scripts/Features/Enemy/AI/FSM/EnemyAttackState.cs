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
    public override void OnEnter()
    {
        _animator.CrossFade(AttackHash, crossFadeDuration);
        Debug.Log($"[EnemyAttackState] Entered Attack State. EnemyType: {_enemyController.EnemyType}");
    }
    public override void OnExit()
    {

    }
    // public override void Update()
    // {
    //     agent.SetDestination(player.position);
    //     _enemyController.Attack();
    // }
    // public override void Update()
    // {
    //     // Ranged: đứng yên và xoay về phía player khi đã đủ gần
    //     // Melee: tiếp tục đuổi theo như cũ
    //     if (IsRangedEnemy())
    //     {
    //         float dist = Vector3.Distance(_enemyController.transform.position, player.position);
    //         if (dist > _enemyController.AttackRange)
    //             agent.SetDestination(player.position);
    //         else
    //         {
    //             agent.SetDestination(_enemyController.transform.position); // đứng yên
    //             _enemyController.transform.LookAt(player);
    //         }
    //     }
    //     else
    //     {
    //         agent.SetDestination(player.position); // logic cũ giữ nguyên
    //     }

    //     // _enemyController.Attack();
    // }
    public override void Update()
    {
        if (IsRangedEnemy())
        {
            float dist = Vector3.Distance(_enemyController.transform.position, player.position);
            if (dist > _enemyController.AttackRange)
                agent.SetDestination(player.position);
            else
            {
                agent.SetDestination(_enemyController.transform.position);
                _enemyController.transform.LookAt(player);
            }
            // Không gọi Attack() ở đây — Animation Event sẽ lo
        }
        else
        {
            agent.SetDestination(player.position);
            _enemyController.Attack(); // melee giữ nguyên
        }
    }

    bool IsRangedEnemy() => _enemyController.EnemyType == EnemyAttackType.Ranged;
}

