using UnityEngine;

public class EnemyExplodeState : EnemyBaseState
{
    public EnemyExplodeState(EnemyController enemyController, Animator animator)
        : base(enemyController, animator) { }

    public override void OnEnter()
    {
        _enemyController.Explode();
    }
}
