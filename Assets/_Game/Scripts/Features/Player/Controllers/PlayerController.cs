using UnityEngine;
using Game.Core.FSM;
using System.Threading;
using Unity.VisualScripting;

namespace Game.Features.Player
{
[RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private PlayerMovementMotor movementMotor = new PlayerMovementMotor();
        [SerializeField] private PlayerGravityMotor gravityMotor;
        [SerializeField] private StateMachine stateMachine;
        [SerializeField] private Animator animator;
        [SerializeField] private float jumpForce = 10f;
        private CharacterController _controller;
        private Transform _cameraTransform;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _cameraTransform = Camera.main != null ? Camera.main.transform : null;

            if (inputHandler == null)
            {
                inputHandler = GetComponent<PlayerInputHandler>();
            }

            movementMotor.Initialize(_controller, transform, _cameraTransform);

            // Ensure gravity motor is a component (MonoBehaviour) — don't construct with 'new'
            if (gravityMotor == null)
            {
                gravityMotor = gameObject.GetComponent<PlayerGravityMotor>() ?? gameObject.AddComponent<PlayerGravityMotor>();
            }

            gravityMotor.Initialize(_controller);
            gravityMotor.SetupTimers();
            SetUpStateMachine();
        }

        void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        void Any (IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        private void Update()
        {
            stateMachine?.Update();
        }

        private void FixedUpdate()
        {
            stateMachine?.FixedUpdate();
        }

        public void HandleMovement()
        {
            movementMotor.Tick(inputHandler.MoveInput, Time.fixedDeltaTime);
            gravityMotor.Tick(Time.fixedDeltaTime);
        }

        public void HandleJump()
        {
            gravityMotor.ApplyJumpVelocity(jumpForce);
        }

        public void ConsumeJumpInput()
        {
            inputHandler.ConsumeJump();
        }

        private void SetUpStateMachine()
        {
            stateMachine = new StateMachine();
            var idleState = new IdleState(this, animator);
            var jumpState = new JumpState(this, animator);
            var runState = new RunState(this, animator);

            // Movement transitions (idle <-> run)
            At(idleState, runState, new FuncPredicate(() => inputHandler.MoveInput.sqrMagnitude > 0.01f));
            At(runState, idleState, new FuncPredicate(() => inputHandler.MoveInput.sqrMagnitude <= 0.01f));

            // Jump can be triggered from any state when space pressed and grounded
            Any(jumpState, new FuncPredicate(() => inputHandler.JumpPressed && gravityMotor.IsGrounded));

            // Exit jump: always go to idle first when grounded, then check movement
            // Jump → Idle when grounded and not moving
            At(jumpState, idleState, new FuncPredicate(() => gravityMotor.IsGrounded && inputHandler.MoveInput.sqrMagnitude <= 0.01f));
            
            // Jump → Run when grounded and moving
            At(jumpState, runState, new FuncPredicate(() => gravityMotor.IsGrounded && inputHandler.MoveInput.sqrMagnitude > 0.01f));

            // Set initial state
            stateMachine.SetState(idleState);
        }
    }
}