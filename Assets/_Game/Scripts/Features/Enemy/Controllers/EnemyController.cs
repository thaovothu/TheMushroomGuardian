using System.Security.Cryptography;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [SerializeField] float wanderRadius = 10f;
    [SerializeField] PlayerDetector playerDetector;
    [SerializeField] float timeBetweenAttacks = 1f;
    [SerializeField] public  HeathSystem heathSystem;
    StateMachine stateMachine;
    List<Timer> timers;
    CountdownTimer attackTimer;
    CountdownTimer hitTimer;

    [SerializeField] float hitDuration = 2f;

    void OnValidate() => this.ValidateRefs();

    void Start()
    {
        attackTimer = new CountdownTimer(timeBetweenAttacks);
        hitTimer = new CountdownTimer(hitDuration);
        timers = new List<Timer> { attackTimer, hitTimer };

        stateMachine = new StateMachine();
        var WalkState = new EnemyWanderState(this, animator, agent, wanderRadius);
        var ChaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);
        var AttackState = new EnemyAttackState(this, animator,agent, playerDetector.Player);
        var HitState = new EnemyHitState(this, animator);
        var DieState = new EnemyDieState(this, animator);


        At(WalkState, ChaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
        At(ChaseState, WalkState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
        At(ChaseState, AttackState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
        At(AttackState, ChaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer()));
        Any(HitState, new FuncPredicate(() => heathSystem.IsHitPlayer()));
        Any(DieState, new FuncPredicate(() => heathSystem.IsDiePlayer()));

        stateMachine.SetState(WalkState);
    }

    void At (IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any (IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    void Update()
    {
        stateMachine.Update();
        attackTimer.Tick(Time.deltaTime);
        // Debug.Log("heathSystem.IsDiePlayer()"+ heathSystem.IsDiePlayer());
    }
    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }
    // public void Attack()
    // {
    //     if (attackTimer.IsRunning) return;

    //     attackTimer.Start();
    //     Debug.Log("Enemy Attack");
    //     if (TryGetComponent(out HeathSystem heathSystem))
    //     {
    //         heathSystem.TakeDamage(10);
    //     }
    // }

    public void Attack()
    {
        if (attackTimer.IsRunning) return;

        attackTimer.Start();
        Debug.Log("Enemy Attack");

        if (playerDetector != null && playerDetector.Player != null)
        {
            var player = playerDetector.Player;
            var health = player.GetComponent<HeathSystem>();
            if (health != null)
            {
                health.TakeDamage(10f);
            }
            else
            {
                Debug.LogWarning("Player has no HeathSystem component");
            }
        }
        else
        {
            Debug.LogWarning("playerDetector or player is null");
        }
    }

    public void DieEnemy()
    {
        PoolSpawnManager.OnRelease?.Invoke(gameObject);
    }

    public void StartHitTimer()
    {
        hitTimer.Start();
    }

}