using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Features.Player;

namespace Game.Core.FSM
{
    public class RunState : BaseState
    {
        public RunState(PlayerController player, Animator animator) : base(player, animator)
        {
        }
        public override void OnEnter()
        {
            SafeCrossFade(RunHash);
        }

        public override void FixedUpdate()
        {
            _playerController.HandleMovement();
        }
    }
}