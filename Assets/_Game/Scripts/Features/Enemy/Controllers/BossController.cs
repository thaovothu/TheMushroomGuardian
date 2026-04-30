using KBCore.Refs;
using UnityEngine;
using UnityEngine.AI;
using SystemAction = System.Action;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BehaviorTree))]
[RequireComponent(typeof(BossBlackboard))]
[RequireComponent(typeof(HealthSystem))]
public class BossController : MonoBehaviour, IPoolSpawned
{
    [Header("Refs")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [SerializeField] HealthSystem healthSystem;
    [SerializeField] BehaviorTree behaviorTree;
    [SerializeField] BossBlackboard blackboard;
    [SerializeField] PlayerDetector playerDetector;

    [Header("Settings")]
    [SerializeField] float hitDuration = 0.5f;

    // Runtime
    BossData _data;
    CountdownTimer _hitTimer;
    bool _isDead;

    // ── Unity ──────────────────────────────────────────────
    void OnValidate() => this.ValidateRefs();

    void Awake()
    {
        _hitTimer = new CountdownTimer(hitDuration);

        // Auto-find nếu chưa gán trong Inspector
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (behaviorTree == null) behaviorTree = GetComponent<BehaviorTree>();
        if (blackboard == null) blackboard = GetComponent<BossBlackboard>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        HealthSystem.OnHealthChanged += HandleHealthChanged;
        HealthSystem.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        HealthSystem.OnHealthChanged -= HandleHealthChanged;
        HealthSystem.OnDeath -= HandleDeath;
    }

    void Update()
    {
        _hitTimer.Tick(Time.deltaTime);

        // Clear IsHit sau khi hitTimer hết — giống Enemy pattern
        if (!_hitTimer.IsRunning && healthSystem.IsHit)
            healthSystem.ClearHit();

        // HP recover theo giây
        if (_data != null && _data.recover > 0f && !_isDead)
            RecoverHP();
    }

    // ── IPoolSpawned ───────────────────────────────────────
    void IPoolSpawned.OnSpawned() { }

    public void OnSpawn(BossData data)
    {
        _data = data ?? new BossData();
        _isDead = false;

        // Apply stats
        agent.speed = _data.moveSpeed;
        healthSystem.Init(_data.hp);

        // Sync vào Blackboard để BT nodes đọc được
        blackboard.currentElement = _data.element;
        blackboard.player = playerDetector?.Player;

        // Reset hit timer
        _hitTimer = new CountdownTimer(hitDuration);

        // Restart BehaviorTree
        if (behaviorTree.Root != null)
            Task.Restart(behaviorTree.Root);

        Debug.Log($"[BossController] Spawned — " +
                  $"hp:{_data.hp} dmg:{_data.damage} " +
                  $"spd:{_data.moveSpeed} elem:{_data.element}");
    }

    // ── Public API (gọi từ BT Action nodes) ───────────────
    public void StartHitTimer() => _hitTimer.Start();

    public void DieBoss()
    {
        animator.SetTrigger("die");

        // Drop crystal tương ứng nguyên tố
        BossEventBus.OnBossDeath?.Invoke();
        DropCrystal(_data.element);

        // Trả về pool (hoặc Destroy nếu không dùng pool)
        PoolSpawnManager.OnRelease?.Invoke(gameObject);
    }

    public void SwitchElement(ElementType newElement)
    {
        _data.element = newElement;
        blackboard.currentElement = newElement;
        animator.SetTrigger("switchElement");
        BossEventBus.OnElementChanged?.Invoke(newElement);
    }

    public float GetDamage() => _data?.damage ?? 10f;

    // ── Private ────────────────────────────────────────────
    void HandleHealthChanged(HealthSystem hs, float current, float max)
    {
        if (hs != healthSystem) return;

        // Trigger hit state
        _hitTimer.Start();
        animator.SetTrigger("hit");
    }

    void HandleDeath(HealthSystem hs)
    {
        if (hs != healthSystem) return;
        _isDead = true;
        DieBoss();
    }

    void RecoverHP()
    {
        float amount = healthSystem.MaxHealth * _data.recover * Time.deltaTime;
        healthSystem.Recover(amount);
    }

    void DropCrystal(ElementType element)
    {
        Debug.Log($"[BossController] Drop crystal: {element}");
        // TODO: spawn crystal prefab tại vị trí Boss
    }
}


