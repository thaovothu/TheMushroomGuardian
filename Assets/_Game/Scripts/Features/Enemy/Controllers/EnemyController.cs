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
    [SerializeField] bool isExplosive = false;

    // ── Runtime ────────────────────────────────────
    BaseEnemyData _data;
    StateMachine _stateMachine;
    CountdownTimer _attackTimer;
    CountdownTimer _hitTimer;

    // EnemyController.cs — thêm 2 property
    public float AttackRange => playerDetector.AttackRange; // hoặc lấy từ _data
    public EnemyAttackType EnemyType => _data?.enemyType ?? EnemyAttackType.Melee;

    // ── Unity ──────────────────────────────────────
    // void OnValidate() => this.ValidateRefs();

    void Awake()
    {
        Debug.Log($"[EnemyController.Awake] {name} at {transform.position}");
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

    void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    // void LateUpdate()
    // {
    //     Debug.Log($"[EnemyController.LateUpdate] {name} at {transform.position}");
    // }

    // ── IPoolSpawned ───────────────────────────────
    // Gọi bởi PoolSpawnManager mỗi lần enemy được lấy ra khỏi pool
    void IPoolSpawned.OnSpawned() { }  // không dùng — dùng OnSpawn(data) bên dưới

    public void OnSpawn(BaseEnemyData data)
    {
        Debug.Log($"[EnemyController.OnSpawn START] {name} at {transform.position}");
        
        _data = data ?? new BaseEnemyData { hp = 100, damage = 10, moveSpeed = 3.5f };

        // Apply stats từ data
        agent.speed = _data.moveSpeed;
        Debug.Log($"[EnemyController] After setting agent.speed: {name} at {transform.position}");
        
        healthSystem.Init(_data.hp);
        Debug.Log($"[EnemyController] After healthSystem.Init: {name} at {transform.position}");

        // Reset timers
        _attackTimer = new CountdownTimer(timeBetweenAttacks);
        _hitTimer = new CountdownTimer(hitDuration);
        Debug.Log($"[EnemyController] After resetting timers: {name} at {transform.position}");

        // Reset FSM
        SetUpStateMachine();
        Debug.Log($"[EnemyController.OnSpawn END] {name} at {transform.position}");

        //Debug.Log($"[EnemyController] {name} — hp:{_data.hp} dmg:{_data.damage} spd:{_data.moveSpeed}");
    }

    // ── FSM ────────────────────────────────────────
    void SetUpStateMachine()
    {
        _stateMachine = new StateMachine();

        var walkState  = new EnemyWanderState(this, animator, agent, wanderRadius);
        var chaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);

        if (isExplosive)
        {
            var explodeState = new EnemyExplodeState(this, animator);

            At(walkState,  chaseState,   new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            At(chaseState, walkState,    new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
            At(chaseState, explodeState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
        }
        else
        {
            var attackState = new EnemyAttackState(this, animator, agent, playerDetector.Player);
            var hitState    = new EnemyHitState(this, animator);
            var dieState    = new EnemyDieState(this, animator);

            At(walkState,   chaseState,  new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            At(chaseState,  walkState,   new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
            At(chaseState,  attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
            At(attackState, chaseState,  new FuncPredicate(() => !playerDetector.CanAttackPlayer()));
            Any(hitState, new FuncPredicate(() => healthSystem.IsHit));
            At(hitState, chaseState, new FuncPredicate(() => !healthSystem.IsHit && playerDetector.CanDetectPlayer()));
            At(hitState, walkState,  new FuncPredicate(() => !healthSystem.IsHit && !playerDetector.CanDetectPlayer()));
            Any(dieState, new FuncPredicate(() => healthSystem.IsDead));
        }

        _stateMachine.SetState(walkState);
    }

    void At(IState from, IState to, IPredicate c) => _stateMachine.AddTransition(from, to, c);
    void Any(IState to, IPredicate c) => _stateMachine.AddAnyTransition(to, c);

    // ── Public API ─────────────────────────────────
    // public void Attack()
    // {
    //     if (_attackTimer.IsRunning) return;
    //     _attackTimer.Start();

    //     var health = playerDetector.Player?.GetComponent<HealthSystem>();
    //     if (health == null) return;

    //     float damage = _data?.damage ?? 10;
    //     health.TakeDamage(damage);
    // }

    public void OnAttack()
    {
        Attack();
    }
    // Hash của animation tấn công — dùng để re-trigger ở mỗi đòn melee, tránh case
    // animation chạy 1 lần ở OnEnter rồi dừng ở frame cuối.
    static readonly int AttackAnimHash = Animator.StringToHash("Attack01");

    public void Attack()
    {
        // Attack() bị gọi MỖI FRAME từ EnemyAttackState (melee). Phải return sớm TRƯỚC mọi
        // Debug.Log, nếu không log + nội suy chuỗi sẽ chạy mỗi frame -> tụt FPS lúc combat.
        if (_attackTimer.IsRunning) return;
        _attackTimer.Start();

        switch (_data?.enemyType)
        {
            case EnemyAttackType.Ranged:
                // Ranged: animation play 1 lần ở OnEnter + Animation Event spawn đạn.
                RangeAttack();
                break;
            default: // Melee
                // Re-trigger animation cho MỖI đòn (đòn 2, 3, ... ) — nếu không, sau lần
                // CrossFade ở OnEnter, animation kết thúc và quái đứng yên ở frame cuối.
                if (animator != null)
                    animator.CrossFade(AttackAnimHash, 0.05f);

                var health = playerDetector.Player?.GetComponent<HealthSystem>();
                if (health != null)
                    health.TakeDamage(_data?.damage ?? 10);
                break;
        }
    }

    void RangeAttack()
    {
        if (_data?.bulletPrefab == null || playerDetector.Player == null) return;

        GameObject bulletGO = Instantiate(
            _data.bulletPrefab,
            transform.position + Vector3.up,
            Quaternion.identity
        );
        // Dùng EnemyBullet từ code tham khảo
        var bullet = bulletGO.GetComponent<EnemyBullet>();
        if (bullet != null)
        {
            bullet.damage = _data?.damage ?? 1;
            bullet.SetTarget(playerDetector.Player);
        }
    }

    public void Explode()
    {
        if (playerDetector.Player != null &&
            playerDetector.Player.TryGetComponent(out HealthSystem health))
            health.TakeDamage(_data?.damage ?? 10);

        DropItems();
        if (QuestSpawnManager.Instance != null)
            QuestSpawnManager.Instance.NotifySpawnedEnemyDied(gameObject);
        PoolSpawnManager.Instance.OnRelease?.Invoke(gameObject);
    }

    public void StartHitTimer() => _hitTimer.Start();
    
    public bool IsHitTimerDone() => _hitTimer.IsFinished();

    public void DropItems()
    {
        // Drop item khi enemy chết
        if (ItemDropManager.Instance != null)
        {
            // TODO: Check nếu là boss
            ItemDropManager.Instance.DropItemsOnEnemyDeath(transform.position, false);
        }
    }

    public void DieEnemy()
    {
        QuestSpawnManager.Instance?.NotifySpawnedEnemyDied(gameObject);
        PoolSpawnManager.Instance.OnRelease?.Invoke(gameObject);
    }
}