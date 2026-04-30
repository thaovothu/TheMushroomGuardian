using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class EnemyBaseState : IState
{
    protected readonly EnemyController _enemyController;
    protected readonly Animator _animator;

    protected static readonly int IdleHash = Animator.StringToHash("IdleNormal");
    protected static readonly int WalkHash = Animator.StringToHash("WalkFWD");
    protected static readonly int RunHash = Animator.StringToHash("RunFWD");
    protected static readonly int AttackHash = Animator.StringToHash("Attack01");
    protected static readonly int DieHash = Animator.StringToHash("Die");
    protected static readonly int HitHash = Animator.StringToHash("Hit");

    protected const float crossFadeDuration = 0.01f;
    protected EnemyBaseState(EnemyController enemyController, Animator animator)
    {
        this._enemyController = enemyController;
        this._animator = animator;
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }

}