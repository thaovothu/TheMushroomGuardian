using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Features.Player;

namespace Game.Core.FSM
{
    public class IdleState : BaseState
    {
        public IdleState(PlayerController player, Animator animator) : base(player, animator)
        {
        }
        public override void OnEnter()
        {
            SafeCrossFade(IdleHash);
        }

        public override void FixedUpdate()
        {
            _playerController.HandleMovement();
        }
    }
}