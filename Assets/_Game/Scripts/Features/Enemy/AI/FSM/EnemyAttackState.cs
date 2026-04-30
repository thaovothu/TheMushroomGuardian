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

    }
    public override void OnExit()
    {

    }
    public override void Update()
    {
        agent.SetDestination(player.position);
        _enemyController.Attack();
    }
}

