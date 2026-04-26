using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class EnemyHitState : EnemyBaseState
{
    public EnemyHitState(EnemyController enemyController, Animator animator) : base(enemyController, animator)
    {
    }
    public override void OnEnter()
    {
        Debug.Log("Enemy Hit");
        _animator.CrossFade(HitHash, 0f);
        _enemyController.StartHitTimer();
    }

    public override void Update()
    {
        
    }
    public override void OnExit()
    {
        _enemyController.heathSystem.ClearHit();
    }
}
