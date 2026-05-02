using UnityEngine;
public class EnemyDieState : EnemyBaseState
{
    private bool _hasDied = false;

    public EnemyDieState(EnemyController enemyController, Animator animator)
        : base(enemyController, animator)
    {
    }

    public override void OnEnter()
    {
        //Debug.Log("Enemy Die");
        _animator.CrossFade(DieHash, 0f);
    }

    public override void Update()
    {
        if (_hasDied) return;

        var state = _animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Die") && state.normalizedTime >= 1f)
        {
            _hasDied = true;
            // Drop items trước khi die
            _enemyController.DropItems();
            _enemyController.DieEnemy();
        }
    }

    public override void OnExit()
    {
    }
}
