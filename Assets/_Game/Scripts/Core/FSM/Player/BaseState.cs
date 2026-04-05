//for player state machine, base state class
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Features.Player;

    public abstract class BaseState : IState
    {
        protected readonly PlayerController _playerController;
        protected readonly Animator _animator;
        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");
        public static readonly int DashHash = Animator.StringToHash("Dash");
        public static readonly int DieHash = Animator.StringToHash("Die");
        public static readonly int AttackHash = Animator.StringToHash("AttackNormal");
        public static readonly int HitHash = Animator.StringToHash("Hit");
        public static readonly int DefendHash = Animator.StringToHash("Defend");

        protected const float crossFadeDuration = 0.01f;    
        protected BaseState (PlayerController player, Animator animator)
        {
            this._playerController = player;
            this._animator = animator;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
    }
