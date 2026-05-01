//new 29/6/2024
using UnityEngine;
using UnityEngine.AI;

public enum BossAnimState { Idle, Walk, Run, Attack, Skill, SwitchElement, Hit, Die }

public class BossBlackboard : MonoBehaviour, IPoolSpawned
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;
    public HealthSystem healthSystem;
    private BehaviorTree behaviorTree;
    BaseEnemyData _data;

    [Header("Combat Settings")]
    [SerializeField] public float detectRange = 10f;
    public float attackRange = 2.5f;
    public float attackCooldown = 2f;
    public float skillCooldown = 5f;
    public float moveSpeed = 15f;     // Tốc độ đi bộ (distance <= 10m)
    public float runSpeed ;      // Tốc độ chạy (distance > 10m)
    public int damageBoss;
    // Runtime state — các node đọc/ghi vào đây
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public bool isPhase2;
    [HideInInspector] public float lastAttackTime = -99f;
    [HideInInspector] public float lastSkillTime = -99f;
    [HideInInspector] public ElementType currentElement = ElementType.Earth;
    [HideInInspector] public BossAnimState currentAnimState = BossAnimState.Idle;
    [HideInInspector] public bool isHit = false;
    private float lastHealthValue;
    private float hitTimeout = 0f;      // Timeout để reset isHit
    private const float HIT_DURATION = 1f; // Hit animation duration
    private bool healthInitialized = false;

    void Awake()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (behaviorTree == null) behaviorTree = GetComponent<BehaviorTree>();
        
        //Debug.Log($"[BossBlackboard.Awake] Initialized - HealthSystem: {(healthSystem != null ? "✓" : "✗ NULL")}, Agent: {(agent != null ? "✓" : "✗ NULL")}, BehaviorTree: {(behaviorTree != null ? "✓" : "✗ NULL")}");
    }

    void Start()
    {
        // Don't initialize health here - HealthSystem might not be ready yet
        // Will initialize lazily in Update()
    }

    void Update()
    {
        // Skip if not initialized yet
        if (!healthInitialized)
            return;

        // Safety check: verify BossBlackboard is active
        if (healthSystem == null)
        {
            return;
        }

        // Retry tìm player nếu vẫn null
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogWarning("[BossBlackboard] Player still not found!");
                return;
            }
            else
            {
                Debug.Log("[BossBlackboard] ✓ Player found!");
            }
        }
        
        // Cập nhật distance mỗi frame — dùng chung cho mọi node
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Debug.Log($"[BossBlackboard.Update] Distance updated: {distanceToPlayer}");
        
        // Phase check
        isPhase2 = healthSystem.GetHPPercent() < 0.5f;
        
        // Detect if Boss took damage
        if (healthSystem.CurrentHealth < lastHealthValue)
        {
            isHit = true;
            hitTimeout = HIT_DURATION;
            lastHealthValue = healthSystem.CurrentHealth;
        }

        // Reset isHit sau HIT_DURATION
        if (isHit && hitTimeout > 0)
        {
            hitTimeout -= Time.deltaTime;
            if (hitTimeout <= 0)
            {
                isHit = false;
            }
        }
    }

    /// <summary>
    /// IPoolSpawned: Initialize boss with data from PoolSpawnManager
    /// </summary>
    void IPoolSpawned.OnSpawned() { }

    public void OnSpawn(BaseEnemyData data)
    {
        _data = data ?? new BaseEnemyData { hp = 100, damage = 10, moveSpeed = 3.5f };

        // Apply stats từ data
        agent.speed = moveSpeed;
        runSpeed = _data.moveSpeed;
        healthSystem.Init(_data.hp);
        damageBoss = _data.damage;

        // Initialize health tracking ngay sau Init()
        lastHealthValue = healthSystem.CurrentHealth;
        healthInitialized = true;
        Debug.Log($"[BossBlackboard.OnSpawn] ✓ Health tracking initialized: {lastHealthValue}");

        // Restart BehaviorTree - retry nếu không ready
        if (behaviorTree == null)
        {
            behaviorTree = GetComponent<BehaviorTree>();
        }

        if (behaviorTree != null && behaviorTree.Root != null)
        {
            Task.Restart(behaviorTree.Root);
        }
    }

    // void Update()
    // {
    //     // Lazy initialization: wait until HealthSystem has valid health value
    //     if (!healthInitialized && healthSystem != null && healthSystem.CurrentHealth > 0)
    //     {
    //         lastHealthValue = healthSystem.CurrentHealth;
    //         healthInitialized = true;
    //         //Debug.Log($"[BossBlackboard] ✓ LAZY INIT! lastHealthValue initialized to: {lastHealthValue}");
    //     }

    //     // Skip if not initialized yet
    //     if (!healthInitialized)
    //         return;

    //     // Safety check: verify BossBlackboard is active
    //     if (healthSystem == null)
    //     {
    //         //Debug.LogError("[BossBlackboard.Update] ✗✗✗ HealthSystem is NULL! Not tracking damage this frame!");
    //         return;
    //     }

    //     if (player == null)
    //         //Debug.LogWarning("[BossBlackboard] Player not found! Tag 'Player'?");
        
    //     // Cập nhật distance mỗi frame — dùng chung cho mọi node
    //     if (player != null)
    //         distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
    //     // Phase check
    //     isPhase2 = healthSystem.GetHPPercent() < 0.5f;
        
    //     // Detect if Boss took damage
    //     if (healthSystem.CurrentHealth < lastHealthValue)
    //     {
    //         isHit = true;
    //         hitTimeout = HIT_DURATION;
    //         lastHealthValue = healthSystem.CurrentHealth;
    //         //Debug.Log($"[BossBlackboard] ✓✓✓ DAMAGE DETECTED! NewHealth: {healthSystem.CurrentHealth}, isHit = TRUE");
    //     }

    //     // Reset isHit sau HIT_DURATION
    //     if (isHit && hitTimeout > 0)
    //     {
    //         hitTimeout -= Time.deltaTime;
    //         if (hitTimeout <= 0)
    //         {
    //             isHit = false;
    //             //Debug.Log($"[BossBlackboard] ✓ Hit timeout! isHit = false");
    //         }
    //     }
    // }

    /// <summary>
    /// Centralized animation state management - prevent redundant SetTrigger calls
    /// </summary>
    public void PlayAnimation(BossAnimState state)
    {
        //Debug.Log("anim" + state);
        if (currentAnimState == state) return;  // Avoid calling trigger multiple times

        currentAnimState = state;

        switch (state)
        {
            case BossAnimState.Walk:
                animator.SetBool("isMoving", true);
                animator.SetBool("isRunning", false);
                //Debug.Log("[Boss] Walking animation triggered.");
                break;
            case BossAnimState.Run:
                animator.SetBool("isMoving", true);
                animator.SetBool("isRunning", true);
                //Debug.Log("[Boss] Running animation triggered.");
                break;
            case BossAnimState.Idle:
                animator.SetBool("isMoving", false);
                animator.SetBool("isRunning", false);
                //Debug.Log("[Boss] Idle animation triggered.");
                break;
            case BossAnimState.Attack:
                animator.SetTrigger("attack");
                //Debug.Log("[Boss] Attack animation triggered.");
                break;
            case BossAnimState.Skill:
                animator.SetTrigger("skill");
                //Debug.Log("[Boss] Skill animation triggered.");
                break;
            case BossAnimState.SwitchElement:
                animator.SetTrigger("switchElement");
                //Debug.Log("[Boss] Switch Element animation triggered.");
                break;
            case BossAnimState.Hit:
                animator.SetTrigger("hit");
                //Debug.Log("[Boss] Hit animation triggered.");
                break;
            case BossAnimState.Die:
                animator.SetTrigger("die");
                //Debug.Log("[Boss] Die animation triggered.");
                break;
        }
    }

    /// <summary>
    /// Player detection - similar to PlayerDetector.CanDetectPlayer()
    /// </summary>
    public bool CanDetectPlayer()
    {
        if (player == null) return false;
        return distanceToPlayer <= detectRange;
    }

    /// <summary>
    /// Attack range check - similar to PlayerDetector.CanAttackPlayer()
    /// </summary>
    public bool CanAttackPlayer()
    {
        if (player == null) return false;
        return distanceToPlayer <= attackRange;
    }

    /// <summary>
    /// Check if player is too far away
    /// </summary>
    public bool IsPlayerTooFar()
    {
        return distanceToPlayer > detectRange;
    }
}
