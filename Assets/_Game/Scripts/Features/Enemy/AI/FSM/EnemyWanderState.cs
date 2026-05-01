using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyWanderState : EnemyBaseState
{
    readonly NavMeshAgent agent;
    readonly Vector3 wanderPoint;
    readonly float wanderRadius = 5f;
    
    public EnemyWanderState (EnemyController enemyController, Animator animator, NavMeshAgent agent, float wanderRadius) : base(enemyController, animator)
    {
        this.agent = agent;
        this.wanderPoint = agent.transform.position;
        this.wanderRadius = wanderRadius;
    }

    public override void OnEnter ()
    {
        _animator.CrossFade(WalkHash, crossFadeDuration);
    }

    public override void OnExit ()
    {
        //Debug.Log("Enemy exit Walk");
    }

    public override void Update ()
    {
        if (HasReachedDestination())
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += wanderPoint;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
            
            var finalPosition = hit.position;
            agent.SetDestination(finalPosition);
        }
    }

    private bool HasReachedDestination()
    {
        return !agent.pathPending
            && agent.remainingDistance <= agent.stoppingDistance
            && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }
}
