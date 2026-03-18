using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Features.Core.FSM
{
    public abstract class BaseState : IState
    {
        protected readonly Game.Features.Player.Controllers.PlayerController _playerController;
        protected readonly Animator _animator;
        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");

        protected const float crossFadeDuration = 0.1f;    
        protected BaseState (Game.Features.Player.Controllers.PlayerController player, Animator animator)
        {
            this._playerController = player;
            this._animator = animator;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
    }

}