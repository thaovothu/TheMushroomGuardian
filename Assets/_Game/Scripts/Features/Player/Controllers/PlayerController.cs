using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using UnityEngine;
using UnityEngine.SocialPlatforms;
public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Rigidbody rb;
        [SerializeField] GroundChecker groundChecker;
        [SerializeField] Animator animator;
        [SerializeField] CinemachineFreeLook freeLookVCam;
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
        const float ZeroF = 0f;

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

            // Debug.Log($"[PlayerController] JumpTimer is running {jumpTimer.IsRunning}, DashTimer is running {dashTimer.IsRunning}, Grounded {groundChecker.IsGrounded}");
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

        // Define transitions
        At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
        Debug.Log($"[PlayerController] JumpTimer is running {jumpTimer.IsRunning}");
        At(locomotionState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
        Debug.Log($"[PlayerController] DashTimer is running {dashTimer.IsRunning}");
        Any(locomotionState, new FuncPredicate(() => !jumpTimer.IsRunning && groundChecker.IsGrounded && !dashTimer.IsRunning)); 
  
        Debug.Log($"[PlayerController] JumpTimer is running {jumpTimer.IsRunning}, Grounded {groundChecker.IsGrounded}");
        
        // Set initial state
        stateMachine.SetState(locomotionState);
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

        timers = new List<Timer> {jumpTimer, jumpCooldownTimer, dashTimer, dashCooldownTimer};
        }


        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        void OnEnable()
        {
            input.Jump += OnJump;
            input.Dash += OnDash;
        }

        void OnDisable()
        {
            input.Jump -= OnJump;
            input.Dash -= OnDash;
        }

        void OnJump(bool performed)
        {
            if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpTimer.Start();
            }
            // else if (!performed && jumpTimer.IsRunning)
            // {
            //     jumpTimer.Stop();
            // }
            
        }

        void OnDash(bool performed)
        {
            if (performed && !dashTimer.IsRunning && !dashCooldownTimer.IsRunning)
            {
                dashTimer.Start();
            }
            // else if (!performed && dashTimer.IsRunning)
            // {
            //     dashTimer.Stop();
            // }
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
    }

    public void HandleMovement()
    {
        var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;

        if (adjustedDirection.magnitude > ZeroF)
        {
            // Xoay player snap theo hướng di chuyển
            HandleRotation(adjustedDirection);
            SmoothSpeed(adjustedDirection.magnitude);

            // Di chuyển theo hướng player đang nhìn, không theo camera
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
            Vector3 velocity = adjustedDirection * (moveSpeed  * Time.fixedDeltaTime);
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
        if (dashTimer.progress >= 0.95f) // gần 1f = vừa mới start
        {
            rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);
        }
    }


}

    