//new 29/6/2024
using UnityEngine;
using UnityEngine.AI;

public enum BossAnimState { Idle, Walk, Run, Attack01, Attack02, Skill, SwitchElement, Hit, Die }

public class BossBlackboard : MonoBehaviour, IPoolSpawned
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;
    public HealthSystem healthSystem;
    private BehaviorTree behaviorTree;
    BaseEnemyData _data;
    BigBossModelSwitcher _bigBoss; // != null → là boss cuối: áp bảng khắc hệ khi đánh player

    [Header("Combat Settings")]
    [SerializeField] public float detectRange = 10f;
    public float attackRange = 2.5f;
    public float skillRange = 12f;
    [Tooltip("Prefab đạn dùng cho RangedAttack03/04 và BurstBulletAction (phase 2 Boss Lửa).")]
    public GameObject rangedProjectilePrefab;
    public float attackCooldown = 2f;
    public float skillCooldown = 5f;
    public float moveSpeed = 15f;     // Tốc độ đi bộ (distance <= 10m)
    public float runSpeed;      // Tốc độ chạy (distance > 10m)
    public int damageBoss;
    [Tooltip("Hệ số nhân damage theo phase (BigBoss: Đất yếu = 0.6...). Mặc định 1 → boss khác không đổi.")]
    public float damageMultiplier = 1f;
    // Runtime state — các node đọc/ghi vào đây
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public bool isPhase2;
    [HideInInspector] public float lastAttackTime = -99f;
    [HideInInspector] public float lastSkillTime = -99f;
    [HideInInspector] public ElementType currentElement = ElementType.Earth;
    [HideInInspector] public BossAnimState currentAnimState = BossAnimState.Idle;
    [HideInInspector] public bool isHit = false;
    [Header("Boss Identity")]
    [SerializeField] public ElementType bossBaseElement = ElementType.Earth; // set cố định trong Inspector/Prefab
    // private float lastHealthValue;
    // private float hitTimeout = 0f;      // Timeout để reset isHit
    // private const float HIT_DURATION = 0.35f; // Hit animation duration
    private bool healthInitialized = false;
    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        if (behaviorTree == null) behaviorTree = GetComponent<BehaviorTree>();
        _bigBoss = GetComponent<BigBossModelSwitcher>();

        // Chỉ auto-fetch animator nếu KHÔNG có switcher
        // (boss có switcher sẽ tự set trong Start)
        if (animator == null && GetComponent<BossModelSwitcher>() == null)
            animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) player = playerGO.transform;
        }
    }


    void OnEnable() { GameEvent.Player.OnSpawned += SetPlayer; }
    void OnDisable() { GameEvent.Player.OnSpawned -= SetPlayer; }
    void SetPlayer(GameObject p) { player = p.transform; }
    void Update()
    {
        // Debug.Log("PositionBoss: " + transform.position);
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

        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        isPhase2 = healthSystem.GetHPPercent() < 0.5f;
    }
    void IPoolSpawned.OnSpawned() { }
    public void OnSpawn(BaseEnemyData data)
    {
        _data = data ?? new BaseEnemyData { hp = 100, damage = 10, moveSpeed = 3.5f };

        agent.speed = moveSpeed;
        runSpeed = _data.moveSpeed;
        healthSystem.Init(_data.hp);
        damageBoss = _data.damage;

        // lastHealthValue = healthSystem.CurrentHealth;
        healthInitialized = true;

        if (behaviorTree == null)
            behaviorTree = GetComponent<BehaviorTree>();

        if (behaviorTree != null && behaviorTree.Root != null)
            Task.Restart(behaviorTree.Root);

        GameEvent.BossEventBus.OnBossSpawned?.Invoke(gameObject);

#if UNITY_EDITOR
        Debug.Log($"[BossBlackboard] OnSpawn complete: {gameObject.name} ");
#endif
    }

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
            case BossAnimState.Attack01:
                animator.SetTrigger("attack");
                //Debug.Log("[Boss] Attack animation triggered.");
                break;
            case BossAnimState.Attack02:
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
    /// Damage cuối cùng boss đánh lên player = damage gốc × hệ-số-phase (damageMultiplier)
    /// × bảng khắc hệ (hệ boss tấn công vs hệ player phòng thủ).
    /// Bảng khắc hệ CHỈ áp cho BigBoss (boss cuối). Boss khác: _bigBoss == null → giữ nguyên
    /// (damageMultiplier mặc định 1) nên KHÔNG bị ảnh hưởng.
    /// </summary>
    public float ResolveDamageVsPlayer(float baseDamage)
    {
        float dmg = baseDamage * damageMultiplier;

        if (_bigBoss != null && player != null)
        {
            var ps = player.GetComponent<PlayerSkillController>();
            if (ps != null)
                dmg *= ElementalSystem.GetMultiplier(currentElement, ps.GetCurrentElement());
        }
        return dmg;
    }
}