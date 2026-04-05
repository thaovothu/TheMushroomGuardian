using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EnemyChaseState : EnemyBaseState
{
    readonly NavMeshAgent agent;
    readonly Transform player;
    public EnemyChaseState(EnemyController enemyController, Animator animator, NavMeshAgent agent, Transform player) : base(enemyController, animator)
    {
        this.agent = agent;
        this.player = player;
    }
    public override void OnEnter()
    {
        _animator.CrossFade(RunHash,crossFadeDuration);
    }

    public override void Update()
    {
        agent.SetDestination(player.position);
    }
}
