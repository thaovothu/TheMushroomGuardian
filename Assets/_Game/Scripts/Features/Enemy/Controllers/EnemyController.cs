// using System.Security.Cryptography;
// using KBCore.Refs;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.AI;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// [RequireComponent(typeof(NavMeshAgent))]
// [RequireComponent(typeof(PlayerDetector))]
// public class EnemyController : MonoBehaviour
// {
//     [SerializeField] NavMeshAgent agent;
//     [SerializeField] Animator animator;
//     [SerializeField] float wanderRadius = 10f;
//     [SerializeField] PlayerDetector playerDetector;
//     [SerializeField] float timeBetweenAttacks = 1f;
//     [SerializeField] public  HeathSystem heathSystem;
//     StateMachine stateMachine;
//     // EnemyStats stats;
//     // CombatSystem combatSystem;
//     List<Timer> timers;
//     CountdownTimer attackTimer;
//     CountdownTimer hitTimer;

//     [SerializeField] float hitDuration = 2f;

//     void OnValidate() => this.ValidateRefs();

//     void Start()
//     {
//         // stats = new EnemyStats(100, 10);
//         // combatSystem = new CombatSystem();

//         attackTimer = new CountdownTimer(timeBetweenAttacks);
//         hitTimer = new CountdownTimer(hitDuration);
//         timers = new List<Timer> { attackTimer, hitTimer };

//         stateMachine = new StateMachine();
//         SetUpStateMachine();
//     }

//     void At (IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
//     void Any (IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

//     void Update()
//     {
//         stateMachine.Update();
//         attackTimer.Tick(Time.deltaTime);
//         // Debug.Log("heathSystem.IsDiePlayer()"+ heathSystem.IsDiePlayer());
//     }
//     void FixedUpdate()
//     {
//         stateMachine.FixedUpdate();
//     }

//     void SetUpStateMachine()
//     {
//         var WalkState = new EnemyWanderState(this, animator, agent, wanderRadius);
//         var ChaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);
//         var AttackState = new EnemyAttackState(this, animator, agent, playerDetector.Player);
//         var HitState = new EnemyHitState(this, animator);
//         var DieState = new EnemyDieState(this, animator);


//         At(WalkState, ChaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
//         At(ChaseState, WalkState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
//         At(ChaseState, AttackState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
//         At(AttackState, ChaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer()));
//         Any(HitState, new FuncPredicate(() => heathSystem.IsHitPlayer()));
//         Any(DieState, new FuncPredicate(() => heathSystem.IsDiePlayer()));

//         stateMachine.SetState(WalkState);
//     }
//     public void Attack()
//     {
//         if (attackTimer.IsRunning) return;

//         attackTimer.Start();
//         Debug.Log("Enemy Attack");

//         if (playerDetector != null && playerDetector.Player != null)
//         {
//             var player = playerDetector.Player;
//             var health = player.GetComponent<HeathSystem>();
//             if (health != null)
//             {
//                 health.TakeDamage(10f);
//             }
//             else
//             {
//                 Debug.LogWarning("Player has no HeathSystem component");
//             }
//         }
//         else
//         {
//             Debug.LogWarning("playerDetector or player is null");
//         }
//     }
//     public void DieEnemy()
//     {
//         PoolSpawnManager.OnRelease?.Invoke(gameObject);
//     }

//     public void StartHitTimer()
//     {
//         hitTimer.Start();
//     }

// }
// Features/Enemy/Controllers/EnemyController.cs
using KBCore.Refs;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerDetector))]
public class EnemyController : MonoBehaviour, IPoolSpawned
{
    [Header("Refs")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [SerializeField] PlayerDetector playerDetector;
    [SerializeField] public HealthSystem healthSystem;

    [Header("Settings")]
    [SerializeField] float wanderRadius = 10f;
    [SerializeField] float timeBetweenAttacks = 1f;
    [SerializeField] float hitDuration = 2f;

    // ── Runtime ────────────────────────────────────
    BaseEnemyData _data;
    StateMachine _stateMachine;
    CountdownTimer _attackTimer;
    CountdownTimer _hitTimer;

    // ── Unity ──────────────────────────────────────
    void OnValidate() => this.ValidateRefs();

    void Awake()
    {
        _attackTimer = new CountdownTimer(timeBetweenAttacks);
        _hitTimer = new CountdownTimer(hitDuration);
        _stateMachine = new StateMachine();
        SetUpStateMachine();
    }

    void Update()
    {
        _stateMachine.Update();
        _attackTimer.Tick(Time.deltaTime);
        _hitTimer.Tick(Time.deltaTime);
    }

    void FixedUpdate() => _stateMachine.FixedUpdate();

    // ── IPoolSpawned ───────────────────────────────
    // Gọi bởi PoolSpawnManager mỗi lần enemy được lấy ra khỏi pool
    void IPoolSpawned.OnSpawned() { }  // không dùng — dùng OnSpawn(data) bên dưới

    public void OnSpawn(BaseEnemyData data)
    {
        _data = data ?? new BaseEnemyData { hp = 100, damage = 10, moveSpeed = 3.5f };

        // Apply stats từ data
        agent.speed = _data.moveSpeed;
        healthSystem.Init(_data.hp);

        // Reset timers
        _attackTimer = new CountdownTimer(timeBetweenAttacks);
        _hitTimer = new CountdownTimer(hitDuration);

        // Reset FSM
        SetUpStateMachine();

        Debug.Log($"[EnemyController] {name} — hp:{_data.hp} dmg:{_data.damage} spd:{_data.moveSpeed}");
    }

    // ── FSM ────────────────────────────────────────
    void SetUpStateMachine()
    {
        _stateMachine = new StateMachine();

        var walkState = new EnemyWanderState(this, animator, agent, wanderRadius);
        var chaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);
        var attackState = new EnemyAttackState(this, animator, agent, playerDetector.Player);
        var hitState = new EnemyHitState(this, animator);
        var dieState = new EnemyDieState(this, animator);

        At(walkState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
        At(chaseState, walkState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
        At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
        At(attackState, chaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer()));
        Any(hitState, new FuncPredicate(() => healthSystem.IsHit));
        // Exit HitState khi timer kết thúc
        At(hitState, chaseState, new FuncPredicate(() => !healthSystem.IsHit && playerDetector.CanDetectPlayer()));
        At(hitState, walkState, new FuncPredicate(() => !healthSystem.IsHit && !playerDetector.CanDetectPlayer()));
        Any(dieState, new FuncPredicate(() => healthSystem.IsDead));

        _stateMachine.SetState(walkState);
    }

    void At(IState from, IState to, IPredicate c) => _stateMachine.AddTransition(from, to, c);
    void Any(IState to, IPredicate c) => _stateMachine.AddAnyTransition(to, c);

    // ── Public API ─────────────────────────────────
    public void Attack()
    {
        if (_attackTimer.IsRunning) return;
        _attackTimer.Start();

        var health = playerDetector.Player?.GetComponent<HealthSystem>();
        if (health == null)
        {
            Debug.LogWarning($"[EnemyController] {name}: Player không có HealthSystem.");
            return;
        }

        float damage = _data?.damage ?? 10;
        health.TakeDamage(damage);
    }

    public void StartHitTimer() => _hitTimer.Start();
    
    public bool IsHitTimerDone() => _hitTimer.IsFinished();

    public void DieEnemy() => PoolSpawnManager.OnRelease?.Invoke(gameObject);
}