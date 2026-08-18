using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] GroundChecker groundChecker;
    [SerializeField] public HealthSystem healthSystem;
    [SerializeField] Animator animator;
    // private CinemachineFreeLook freeLookVCam;
    [SerializeField] InputReader input;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 300f;
    [SerializeField] float dashSpeed = 200f;
    [SerializeField] float rotationSpeed = 180f;
    [SerializeField] float smoothTime = 0.1f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 4f;
    [SerializeField] float jumpDuration = 0.3f;
    [SerializeField] float jumpCooldown = 0f;
    [SerializeField] float gravityMultiplier = 4f;


    [Header("Dash Settings")]
    [SerializeField] float dashForce = 8f;
    [SerializeField] float dashDuration = 0.3f;
    [SerializeField] float dashCooldown = 0f;
    [SerializeField] float dashManaCost = 20f;

    [Header("Attack Settings")]
    [SerializeField] float attackCooldown = 0.3f;
    [SerializeField] float attackDistance = 1f;
    [SerializeField] int attackDamage = 10;

    private float _attackBuffMultiplier = 1f;
    private float _attackBuffEndTime = 0f;

    [Header("Skill Settings")]
    [SerializeField] float skillAttackDuration = 1f;
    [SerializeField] float skillDefendDuration = 1f;
    [Header("Die")]
    [SerializeField] float dieDuration = 2f;

    public bool IsInWindZone { get; set; } = false;
    const float ZeroF = 0f;

    public float DieDuration => dieDuration;

    // Trạng thái né đòn — dùng cho BossFireSweep (quét lửa): đang bay (nhảy) hoặc đang dash thì né được.
    public bool IsGrounded => groundChecker != null && groundChecker.IsGrounded;
    public bool IsDashing => dashTimer != null && dashTimer.IsRunning;

    Transform mainCam;

    float currentSpeed;
    float velocity;
    float jumpVelocity;
    float dashVelocity = 1f;

    Vector3 movement;

    List<Timer> timers;
    CountdownTimer jumpTimer;
    CountdownTimer jumpCooldownTimer;
    CountdownTimer dashTimer;
    CountdownTimer dashCooldownTimer;
    CountdownTimer attackCooldownTimer;
    CountdownTimer skillAttackTimer;
    CountdownTimer skillDefendTimer;
    CountdownTimer dieTimer;

    StateMachine stateMachine;
    PlayerSkillController skillController;

    // Animator parameters
    static readonly int Speed = Animator.StringToHash("Speed");

    void Awake()
    {
#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#endif
        mainCam = Camera.main.transform;
        skillController = GetComponent<PlayerSkillController>();

        CameraManager.Instance.SetTarget(transform);

        rb.freezeRotation = true;

        // Tắt mọi va chạm vật lý giữa Player và Enemy/Boss → boss không thể đẩy player
        // (matrix trong Project Settings là defense layer 1; cái này là defense layer 2,
        //  enforce ở runtime kể cả khi child collider của boss/enemy ở layer khác hay
        //  khi matrix bị thay đổi nhầm).
        IgnoreCollisionWithLayer("Enemy");
        IgnoreCollisionWithLayer("Boss");

        SetupTimers();
        SetupStateMachine();
    }

    void IgnoreCollisionWithLayer(string layerName)
    {
        int otherLayer = LayerMask.NameToLayer(layerName);
        if (otherLayer < 0) return;
        Physics.IgnoreLayerCollision(gameObject.layer, otherLayer, true);
    }

    void Start() => input.EnablePlayerActions();

    void Update()
    {
        movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
        stateMachine.Update();
        HandleTimers();
        UpdateAnimator();
        // //Debug.Log($"[PlayerController] JumpTimer is running {jumpTimer.IsRunning}, DashTimer is running {dashTimer.IsRunning}, Grounded {groundChecker.IsGrounded}");
    }

    private void HandleTimers()
    {
        foreach (Timer timer in timers)
        {
            timer.Tick(Time.deltaTime);
        }
    }

    void SetupStateMachine()
    {
        // State Machine
        stateMachine = new StateMachine();

        // Declare states
        var locomotionState = new LocomotionState(this, animator);
        var jumpState = new JumpState(this, animator);
        var dashState = new DashState(this, animator);
        var attackState = new AttackState(this, animator);
        var combatState = new CombatState(this, animator);
        var dieState = new DieState(this, animator);
        var skillAttackState = new CastAttackState(this, animator);
        var skillDefendState = new CastShieldState(this, animator);
        // var dieState = new DieState(this, animator);

        // Define transitions
        At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
        //Debug.Log($"[PlayerController] JumpTimer is running {jumpTimer.IsRunning}");
        At(locomotionState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
        //Debug.Log($"[PlayerController] DashTimer is running {dashTimer.IsRunning}");
        At(locomotionState, attackState, new FuncPredicate(() => attackCooldownTimer.IsRunning && EquipmentSystem.Instance.IsAttackNormal()));
        At(locomotionState, combatState, new FuncPredicate(() => attackCooldownTimer.IsRunning && !EquipmentSystem.Instance.IsAttackNormal()));
        At(locomotionState, skillAttackState, new FuncPredicate(() => skillAttackTimer.IsRunning));
        At(locomotionState, skillDefendState, new FuncPredicate(() => skillDefendTimer.IsRunning));
        At(locomotionState, dieState, new FuncPredicate(() => dieTimer.IsRunning));
        Any(locomotionState, new FuncPredicate(ReturnToLocomotion));
        //Debug.Log($"[PlayerController] JumpTimer is running {jumpTimer.IsRunning}, Grounded {groundChecker.IsGrounded}");

        // Set initial state
        stateMachine.SetState(locomotionState);
    }

    bool ReturnToLocomotion()
    {
        return !jumpTimer.IsRunning
        && groundChecker.IsGrounded
        && !dashTimer.IsRunning
        && !attackCooldownTimer.IsRunning
        && !skillAttackTimer.IsRunning
        && !skillDefendTimer.IsRunning
        && !dieTimer.IsRunning;
    }

    void SetupTimers()
    {
        // Setup timers
        jumpTimer = new CountdownTimer(jumpDuration);
        jumpCooldownTimer = new CountdownTimer(jumpCooldown);
        dashTimer = new CountdownTimer(dashDuration);
        dashCooldownTimer = new CountdownTimer(dashCooldown);

        jumpTimer.OnTimerStarted += () => jumpVelocity = jumpForce;
        jumpTimer.OnTimerStopped += () => jumpCooldownTimer.Start();
        dashTimer.OnTimerStarted += () => dashVelocity = dashForce;
        dashTimer.OnTimerStopped += () =>
        {
            dashVelocity = 1f;
            dashCooldownTimer.Start();
            // Giảm velocity sau dash để không trượt quá xa
            rb.velocity = new Vector3(rb.velocity.x * 0.3f, rb.velocity.y, rb.velocity.z * 0.3f);
        };
        attackCooldownTimer = new CountdownTimer(attackCooldown);
        skillAttackTimer = new CountdownTimer(skillAttackDuration);
        skillDefendTimer = new CountdownTimer(skillDefendDuration);
        dieTimer = new CountdownTimer(dieDuration);

        timers = new List<Timer> { jumpTimer, jumpCooldownTimer, dashTimer, dashCooldownTimer, attackCooldownTimer, skillAttackTimer, skillDefendTimer, dieTimer };
    }


    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    void OnEnable()
    {
        GameEvent.Combat.OnDeath += OnHealthSystemDeath;
        input.Jump += OnJump;
        input.Dash += OnDash;
        input.Attack += OnAttack;
        input.SkillAttack += OnSkillAttack;
        input.SkillDefend += OnSkillDefend;
    }

    void OnDisable()
    {
        GameEvent.Combat.OnDeath -= OnHealthSystemDeath;
        input.Jump -= OnJump;
        input.Dash -= OnDash;
        input.Attack -= OnAttack;
        input.SkillAttack -= OnSkillAttack;
        input.SkillDefend -= OnSkillDefend;
    }

    void OnHealthSystemDeath(HealthSystem deadEntity)
    {
        if (deadEntity != healthSystem) return;
        OnDie();
    }

    void OnSkillAttack(bool performed)
    {
        if (performed && !skillAttackTimer.IsRunning && skillController != null && skillController.HasUnlockedSkills())
        {
            Debug.Log("[PlayerController] SkillAttack input received, starting skill attack timer.");
            skillAttackTimer.Start();
        }
    }

    void OnSkillDefend(bool performed)
    {
        if (performed && !skillDefendTimer.IsRunning && skillController != null && skillController.HasUnlockedSkills())
        {
            skillDefendTimer.Start();
        }
    }
    void OnDie()
    {
        if (!dieTimer.IsRunning)
        {
            dieTimer.Start();
        }
    }

    /// <summary>
    /// Legacy method - skill attack now triggered via input event
    /// </summary>
    public void SkillAttack()
    {
        if (!skillAttackTimer.IsRunning && skillController != null && skillController.HasUnlockedSkills())
        {
            skillAttackTimer.Start();
        }
    }

    /// <summary>
    /// Legacy method - skill defend now triggered via input event
    /// </summary>
    public void SkillDefend()
    {
        if (!skillDefendTimer.IsRunning && skillController != null && skillController.HasUnlockedSkills())
        {
            skillDefendTimer.Start();
        }
    }

    void OnJump(bool performed)
    {
        if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.IsGrounded)
        {
            jumpTimer.Start();
        }

    }

    void OnDash(bool performed)
    {
        if (performed && !dashTimer.IsRunning && !dashCooldownTimer.IsRunning
            && skillController != null && skillController.HasMana(dashManaCost))
        {
            skillController.ConsumeMana(dashManaCost);
            dashTimer.Start();
            GameEvent.Player.OnDashUsed?.Invoke(); // ← thêm dòng này
        }
    }

    void OnAttack()
    {
        if (!attackCooldownTimer.IsRunning)
        {
            // Snap player yaw theo hướng crosshair (cam) trước khi vung kiếm
            // → sword sweep đi đúng theo hướng người chơi đang ngắm.
            AimYawAtCamera();
            attackCooldownTimer.Start();
        }
    }

    void AimYawAtCamera()
    {
        if (mainCam == null) return;
        Vector3 dir = mainCam.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    /// <summary>
    /// Snap yaw của player về hướng bất kỳ (dùng cho bow auto-aim).
    /// Chỉ xoay trên trục Y — không nghiêng player.
    /// </summary>
    public void RotateToward(Vector3 direction)
    {
        Vector3 flat = new Vector3(direction.x, 0f, direction.z);
        if (flat.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(flat);
    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    void UpdateAnimator()
    {
        animator.SetFloat(Speed, currentSpeed);

    }
    public void HandleJump()
    {
        if (!jumpTimer.IsRunning && groundChecker.IsGrounded)
        {
            jumpVelocity = ZeroF;
            return;
        }

        if (!jumpTimer.IsRunning)
        {
            jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
        }

        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        //Debug.Log($"[PlayerController] JumpVelocity {jumpVelocity}, Grounded {groundChecker.IsGrounded}");
    }

    public void ApplyGravity()
    {
        // // Nếu grounded, không apply gravity
        // Debug.Log("groundChecker.IsGrounded"+ groundChecker.IsGrounded);
        if (groundChecker.IsGrounded)
            return;

        // Nếu đang jump, HandleJump() đã xử lý, không cần apply lại
        if (jumpTimer.IsRunning)
            return;

        // Apply gravity khi không grounded và không jump (trên slope)
        jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
    }

    // public void HandleMovement()
    // {
    //     // if (dashTimer.IsRunning) return;
    //     var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;

    //     if (adjustedDirection.magnitude > ZeroF)
    //     {
    //         // Xoay player snap theo hướng di chuyển
    //         HandleRotation(adjustedDirection);
    //         SmoothSpeed(adjustedDirection.magnitude);

    //         // Di chuyển theo hướng player đang nhìn, không theo camera
    //         HandleHorizontalMovement(transform.forward);
    //     }
    //     else
    //     {
    //         SmoothSpeed(ZeroF);
    //         rb.velocity = new Vector3(ZeroF, rb.velocity.y, ZeroF);
    //     }
    // }
    public void HandleMovement()
    {
        var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;

        if (adjustedDirection.magnitude > ZeroF)
        {
            HandleRotation(adjustedDirection);
            SmoothSpeed(adjustedDirection.magnitude);

            // Nếu trong wind zone, không override velocity — để WindZone tự xử lý
            if (!IsInWindZone)
                HandleHorizontalMovement(transform.forward);
        }
        else
        {
            SmoothSpeed(ZeroF);
            rb.velocity = new Vector3(ZeroF, rb.velocity.y, ZeroF);
        }
    }

    void HandleHorizontalMovement(Vector3 adjustedDirection)
    {
        Vector3 velocity = adjustedDirection * (moveSpeed * Time.fixedDeltaTime);
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }

    void HandleRotation(Vector3 adjustedDirection)
    {
        if (adjustedDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(adjustedDirection);
        }
    }


    void SmoothSpeed(float value)
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
    }

    public void HandleDash()
    {
        if (!dashTimer.IsRunning) return;

        // Chỉ apply force ở frame đầu tiên của dash
        if (dashTimer.progress >= 0.95f)
        {
            rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);
            return;
        }

        // Thoát dash ngay khi đã dừng lại
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude < 0.5f)
            dashTimer.Stop();
    }

    readonly Collider[] attackHitBuffer = new Collider[16];

    public void Attack()
    {
        Vector3 attackPos = transform.position + transform.forward;
        int count = Physics.OverlapSphereNonAlloc(attackPos, attackDistance, attackHitBuffer);

        for (int i = 0; i < count; i++)
        {
            var enemy = attackHitBuffer[i];
            if (enemy.CompareTag("Enemy") || enemy.CompareTag("Boss"))
            {
                //Debug.Log($"[Player.Attack] ✓ Hit ENEMY/BOSS {enemy.name}");
                var health = enemy.GetComponent<HealthSystem>();
                if (health != null)
                {
                    float finalDamage = _attackBuffEndTime > Time.time
                        ? attackDamage * _attackBuffMultiplier
                        : attackDamage;
                    health.TakeDamage(finalDamage);
                    //Debug.Log($"[Player.Attack] ✓✓ Damage applied: {attackDamage}, new health: {health.CurrentHealth}");
                }
                else
                {
                    //Debug.LogError($"[Player.Attack] ✗ {enemy.name} has no HealthSystem!");
                }
            }
        }
    }


    // public void CombatAttack()
    // {
    //     equipmentSystem.StartDealDamage();
    // }

    public void CombatAttack()
    {
        StartCoroutine(DoCombatAttackWindow(0.15f));
    }

    IEnumerator DoCombatAttackWindow(float window)
    {
        EquipmentSystem.Instance.StartDealDamage();
        yield return new WaitForSeconds(window);
        EquipmentSystem.Instance.EndDealDamage();
    }

    public void AddAttackBuff(float buffPercent, float duration)
    {
        _attackBuffMultiplier = 1f + buffPercent / 100f;
        _attackBuffEndTime = Time.time + duration;
        Debug.Log($"[PlayerController] ✓ Attack buff: +{buffPercent}% for {duration}s");
    }

}