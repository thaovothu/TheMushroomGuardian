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

        protected void SafeCrossFade(int stateHash)
        {
            if (_animator == null)
            {
                Debug.LogWarning($"Animator is null. Cannot CrossFade state {stateHash}.");
                return;
            }

            int layer = 0;
            if (_animator.layerCount <= layer)
            {
                Debug.LogWarning($"Animator has no layer {layer}. Cannot CrossFade state {stateHash}.");
                return;
            }

            if (!_animator.HasState(layer, stateHash))
            {
                Debug.LogWarning($"Animator does not contain state hash {stateHash} on layer {layer}.");
                return;
            }

            _animator.CrossFade(stateHash, crossFadeDuration, layer);
        }

        // Check if an animator state animation has finished playing
        protected bool IsAnimationFinished(int stateHash)
        {
            if (_animator == null)
                return true;

            int layer = 0;
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
            
            // Check if we're in the target state and animation has played past the normalized time
            return stateInfo.fullPathHash == stateHash && stateInfo.normalizedTime >= 1f;
        }

        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
    }
