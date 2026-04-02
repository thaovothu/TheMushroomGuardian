using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Features.Player;

    public class LocomotionState : BaseState
    {
        public LocomotionState(PlayerController player, Animator animator) : base(player, animator)
        {
        }
        public override void OnEnter()
        {
            // Debug.Log("FSM: Locomotion State Entered");
            _animator.CrossFade(LocomotionHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            _playerController.HandleMovement();
        }
    }
