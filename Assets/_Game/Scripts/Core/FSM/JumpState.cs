using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Features.Player;

namespace Game.Core.FSM
{
    public class JumpState : BaseState
    {
        public JumpState(PlayerController player, Animator animator) : base(player, animator)
        { }
        public override void OnEnter()
        {
            // Force interrupt all animations and jump immediately
            if (_animator != null)
            {
                // Reset animator to clear any lingering animation states
                _animator.Rebind();
                // Instantly transition to Jump state (0f transition time = no fade)
                _animator.CrossFadeInFixedTime(JumpHash, 0f, 0);
            }
            
            // Consume jump input to prevent re-jump on same press
            _playerController.ConsumeJumpInput();
            _playerController.HandleJump();
        }
        public override void FixedUpdate()
        {
            _playerController.HandleMovement();
        }
        public override void OnExit()
        {
            Debug.Log("Exit Jump State");
        }
    }
}