//new 29/6/2024
using UnityEngine;
using UnityEngine.AI;

public enum BossAnimState { Idle, Walk, Run, Attack, Skill, SwitchElement, Hit, Die }

public class BossBlackboard : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;
    public HealthSystem healthSystem;

    [Header("Combat Settings")]
    [SerializeField] public float detectRange = 10f;
    public float attackRange = 2.5f;
    public float attackCooldown = 2f;
    public float skillCooldown = 5f;
    public float moveSpeed = 10f;     // Tốc độ đi bộ (distance <= 10m)
    public float runSpeed = 20f;      // Tốc độ chạy (distance > 10m)

    // Runtime state — các node đọc/ghi vào đây
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public bool isPhase2;
    [HideInInspector] public float lastAttackTime = -99f;
    [HideInInspector] public float lastSkillTime = -99f;
    [HideInInspector] public ElementType currentElement = ElementType.Earth;
    [HideInInspector] public BossAnimState currentAnimState = BossAnimState.Idle;
    [HideInInspector] public bool isHit = false;
    private float lastHealthValue;

    void Awake()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();
        
        // Initialize health tracking
        lastHealthValue = healthSystem.CurrentHealth;
    }

    void Update()
    {
        if (player == null)
            Debug.LogWarning("[BossBlackboard] Player not found! Tag 'Player'?");
        // Cập nhật distance mỗi frame — dùng chung cho mọi node
        if (player != null)
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        // Debug.Log($"[BossBlackboard] Distance to player: {distanceToPlayer}, isPhase2: {isPhase2}");
        // Phase check
        if (healthSystem != null)
            isPhase2 = healthSystem.GetHPPercent() < 0.5f;
        
        // Detect if Boss took damage
        if (healthSystem != null && healthSystem.CurrentHealth < lastHealthValue)
        {
            isHit = true;
            lastHealthValue = healthSystem.CurrentHealth;
            Debug.Log($"[BossBlackboard] Boss took damage! isHit = true");
        }
    }

    /// <summary>
    /// Centralized animation state management - prevent redundant SetTrigger calls
    /// </summary>
    public void PlayAnimation(BossAnimState state)
    {
        Debug.Log("anim" + state);
        if (currentAnimState == state) return;  // Avoid calling trigger multiple times

        currentAnimState = state;

        switch (state)
        {
            case BossAnimState.Walk:
                animator.SetBool("isMoving", true);
                animator.SetBool("isRunning", false);
                Debug.Log("[Boss] Walking animation triggered.");
                break;
            case BossAnimState.Run:
                animator.SetBool("isMoving", true);
                animator.SetBool("isRunning", true);
                Debug.Log("[Boss] Running animation triggered.");
                break;
            case BossAnimState.Idle:
                animator.SetBool("isMoving", false);
                animator.SetBool("isRunning", false);
                Debug.Log("[Boss] Idle animation triggered.");
                break;
            case BossAnimState.Attack:
                animator.SetTrigger("attack");
                Debug.Log("[Boss] Attack animation triggered.");
                break;
            case BossAnimState.Skill:
                animator.SetTrigger("skill");
                Debug.Log("[Boss] Skill animation triggered.");
                break;
            case BossAnimState.SwitchElement:
                animator.SetTrigger("switchElement");
                Debug.Log("[Boss] Switch Element animation triggered.");
                break;
            case BossAnimState.Hit:
                animator.SetTrigger("hit");
                Debug.Log("[Boss] Hit animation triggered.");
                break;
            case BossAnimState.Die:
                animator.SetTrigger("die");
                Debug.Log("[Boss] Die animation triggered.");
                break;
        }
    }
}
