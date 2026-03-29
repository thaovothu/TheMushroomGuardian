using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Features.Player;

    public class JumpState : BaseState
    {
        public JumpState(PlayerController player, Animator animator) : base(player, animator)
        { }
        public override void OnEnter()
        {
            Debug.Log("FSM:Jump State Entered");
            _animator.CrossFade(JumpHash, crossFadeDuration);
            
        }
        public override void FixedUpdate()
        {
            _playerController.HandleJump();
            _playerController.HandleMovement();
        }
        public override void OnExit()
        {
            Debug.Log("Exit Jump State");
            // _playerController.ResetSpeed();
        }
    }
