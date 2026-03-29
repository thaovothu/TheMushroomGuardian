using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;
using UnityEngine.SocialPlatforms;
// using Utilities;

public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Rigidbody rb;
        [SerializeField] GroundChecker groundChecker;
        [SerializeField] Animator animator;
        [SerializeField] CinemachineFreeLook freeLookVCam;
        [SerializeField] InputReader input;

        [Header("Movement Settings")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float rotationSpeed = 15f;
        [SerializeField] float smoothTime = 0.2f;

        [Header("Jump Settings")]
        [SerializeField] float jumpForce = 5f;
        [SerializeField] float jumpDuration = 0.5f;
        [SerializeField] float jumpCooldown = 0f;
        [SerializeField] float gravityMultiplier = 3f;

        [Header("Dash Settings")]
        [SerializeField] float dashForce = 10f;
        [SerializeField] float dashDuration = 1f;
        [SerializeField] float dashCooldown = 2f;

        [Header("Attack Settings")]
        [SerializeField] float attackCooldown = 0.5f;
        [SerializeField] float attackDistance = 1f;
        [SerializeField] int attackDamage = 10;

        const float ZeroF = 0f;

        Transform mainCam;

        float currentSpeed;
        float velocity;
        float jumpVelocity;
        float dashVelocity = 1f;

        Vector3 movement;
        Quaternion targetRotation;

        List<Timer> timers;
        CountdownTimer jumpTimer;
        CountdownTimer jumpCooldownTimer;
        CountdownTimer dashTimer;
        CountdownTimer dashCooldownTimer;
        CountdownTimer attackTimer;

        StateMachine stateMachine;

        // Animator parameters
        static readonly int Speed = Animator.StringToHash("Speed");

        void Awake()
        {
            mainCam = Camera.main.transform;
            freeLookVCam.Follow = transform;
            freeLookVCam.LookAt = transform;
            // Invoke event when observed transform is teleported, adjusting freeLookVCam's position accordingly
            freeLookVCam.OnTargetObjectWarped(transform, transform.position - freeLookVCam.transform.position - Vector3.forward);

            rb.freezeRotation = true;    

            // controller.freezeRotation = true;
            // targetRotation = transform.rotation;

            SetupTimers();
            SetupStateMachine();
        }

        void Start() => input.EnablePlayerActions();

        void Update()
        {
            movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
            stateMachine.Update();
            HandleTimers();
            UpdateAnimator();

            Debug.Log($"[PlayerController] JumpTimer is running {jumpTimer.IsRunning}, DashTimer is running {dashTimer.IsRunning}, Grounded {groundChecker.IsGrounded}");
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
        // var dashState = new DashState(this, animator);
        // var attackState = new AttackState(this, animator);

        // Define transitions
        At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
        At(locomotionState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
        Any(locomotionState, new FuncPredicate(() => !jumpTimer.IsRunning && groundChecker.IsGrounded));
        // Any(locomotionState, new FuncPredicate(() => !dashTimer.IsRunning)); 
        // At(locomotionState, attackState, new FuncPredicate(() => attackTimer.IsRunning));
        // At(attackState, locomotionState, new FuncPredicate(() => !attackTimer.IsRunning));
        // Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));

        // Set initial state
        stateMachine.SetState(locomotionState);
    }

        void SetupTimers()
        {
            // Setup timers
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);

            // attackTimer = new CountdownTimer(attackCooldown);

            // timers = new(5) { jumpTimer, jumpCooldownTimer, dashTimer, dashCooldownTimer, attackTimer };
            jumpTimer.OnTimerStarted += () => jumpVelocity = jumpForce;
            jumpTimer.OnTimerStopped += () => jumpCooldownTimer.Start();
        

            dashTimer = new CountdownTimer(dashDuration);
            dashCooldownTimer = new CountdownTimer(dashCooldown);
            dashTimer.OnTimerStarted += () => dashVelocity = dashForce;
            dashTimer.OnTimerStopped += () =>
            {
                dashVelocity = 1f;
                dashCooldownTimer.Start();
            };

            timers = new(4) {jumpTimer, jumpCooldownTimer, dashTimer , dashCooldownTimer};
        }


        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void OnEnable()
        {
            input.Jump += OnJump;
            input.Dash += OnDash;
            input.Attack += OnAttack;
        }

        void OnDisable()
        {
            input.Jump -= OnJump;
            input.Dash -= OnDash;
            input.Attack -= OnAttack;
        }

        void OnAttack()
        {
            if (!attackTimer.IsRunning)
            {
                attackTimer.Start();
            }
        }

        public void Attack()
        {
            Vector3 attackPos = transform.position + transform.forward;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackDistance);

            foreach (var enemy in hitEnemies)
            {
                Debug.Log(enemy.name);
                if (enemy.CompareTag("Enemy"))
                {
                    // enemy.GetComponent<Health>().TakeDamage(attackDamage);
                }
            }
        }
        void OnJump(bool performed)
        {
            if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpTimer.Start();
            }
            else if (!performed && jumpTimer.IsRunning)
            {
                jumpTimer.Stop();
            }
        }

        void OnDash(bool performed)
        {
            if (performed && !dashTimer.IsRunning && !dashCooldownTimer.IsRunning)
            {
                dashTimer.Start();
            }
            else if (!performed && dashTimer.IsRunning)
            {
                dashTimer.Stop();
            }
        }

        void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        void UpdateAnimator()
        {   
            animator.SetFloat(Speed, currentSpeed);
            Debug.Log($"[PlayerController] CurrentSpeed {currentSpeed}");
        }

        // void HandleTimers()
        // {
        //     foreach (var timer in timers)
        //     {
        //         timer.Tick(Time.deltaTime);
        //     }
        // }

        public void HandleJump()
        {
            // If not jumping and grounded, keep jump velocity at 0
            if (!jumpTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpVelocity = ZeroF;
                // jumpTimer.Stop();
                return;
            }
            
            if (!jumpTimer.IsRunning)
            {
                // Gravity takes over
                jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }

            // Apply velocity
            rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        }

        public void ResetSpeed()
        {
            currentSpeed = ZeroF;
        }
        public void HandleDash()
        {
            rb.velocity = transform.forward * dashVelocity;
        }

        public void HandleMovement()
        {
            var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;

            if (adjustedDirection.magnitude > ZeroF)
            {
                HandleRotation(adjustedDirection);
                HandleHorizontalMovement(adjustedDirection);
                SmoothSpeed(adjustedDirection.magnitude);
            }
            else
            {
                SmoothSpeed(ZeroF);
                // SmoothSpeed(ZeroF);
                // Reset horizontal velocity for a snappy stop
                rb.velocity = new Vector3(ZeroF, rb.velocity.y, ZeroF);
            }
        }

        void HandleHorizontalMovement(Vector3 adjustedDirection)
        {
            // Move the player
            Vector3 velocity = adjustedDirection * (moveSpeed * dashVelocity * Time.fixedDeltaTime);
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }

        void HandleRotation(Vector3 adjustedDirection)
        {
            // // Only rotate if direction is significant enough to avoid unstable behavior
            // if (adjustedDirection.magnitude < 0.01f)
            //     return;
            
            // // Calculate target rotation only when there's valid input
            // targetRotation = Quaternion.LookRotation(adjustedDirection);

            var targetRotation = Quaternion.LookRotation(adjustedDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            // transform.LookAt(transform.position + adjustedDirection);
        }
        

        void SmoothSpeed(float value)
        {
            // currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
            
            // // Clamp to zero if very small to avoid floating point precision issues
            // if (Mathf.Abs(currentSpeed) < 0.001f)
            // {
            //     currentSpeed = 0f;
            // }
            currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
        }
    }