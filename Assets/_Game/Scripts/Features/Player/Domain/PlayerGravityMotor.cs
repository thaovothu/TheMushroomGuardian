using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Core.Utilities;
// using KBCore.Refs;

namespace Game.Features.Player
{
    public class PlayerGravityMotor : MonoBehaviour
    {
        [SerializeField] private float gravityValue = -9.81f;
        [Header("References")]
        [SerializeField] Rigidbody rb;

        [Header("Jump Settings")]
        [SerializeField] float jumpForce = 10f;
        [SerializeField] float jumpCooldown = 0f;
        [SerializeField] float jumpDuration = 0.5f;
        [SerializeField] float gravityMultiplier = 3f;

        private const float ZeroF = 0f;
        float velocity;
        float jumpVelocity;

        [SerializeField] int maxJumpTimes = 2;
        [SerializeField] public int jumpCount = 0;
        List<Timer> timers;
        public CountdownTimer jumpTimer;
        public CountdownTimer jumpCooldownTimer;

        private CharacterController _controller;
        private Vector3 _playerVelocity;
        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;

        public void Initialize(CharacterController controller)
        {
            _controller = controller;
        }

        public void ApplyJumpVelocity(float jumpForce)
        {
            // Set upward velocity when jump starts
            _playerVelocity.y = jumpForce;
            jumpTimer.Start();
        }
        public bool GetIsRunning()
        {
            return jumpTimer.IsRunning;
        }

        public void SetupTimers()
        {
            //Setup Timers
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);
            jumpTimer.OnTimerStarted += () => jumpVelocity = jumpForce;
            jumpTimer.OnTimerStopped += () => jumpCooldownTimer.Start();

            // dashTimer = new CountdownTimer(dashDuration);
            // dashCooldownTimer = new CountdownTimer(dashCooldown);
            // dashTimer.OnTimerStarted += () => dashVelocity = dashForce;
            // dashTimer.OnTimerStopped += () =>
            // {
            //     dashVelocity = 1f;
            //     dashCooldownTimer.Start();
            // };

            timers = new() { jumpTimer, jumpCooldownTimer };
        }

        public void Tick(float deltaTime)
        {
            if (_controller == null)
                return;

            // Tick timers
            if (timers != null)
            {
                foreach (var timer in timers)
                    timer.Tick(deltaTime);
            }

            _isGrounded = _controller.isGrounded;

            if (_isGrounded && _playerVelocity.y < 0f)
            {
                _playerVelocity.y = -2f;
            }

            _playerVelocity.y += gravityValue * gravityMultiplier * deltaTime;

            _controller.Move(_playerVelocity * deltaTime);
        }
    }
}