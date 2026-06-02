//for player state machine, base state class
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

    public abstract class BaseState : IState
    {
        protected readonly PlayerController _playerController;
        protected readonly Animator _animator;
        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");
        public static readonly int DashHash = Animator.StringToHash("Dash");
        public static readonly int DieHash = Animator.StringToHash("Die");
        public static readonly int AttackHash = Animator.StringToHash("AttackNormal");
        public static readonly int AttackWeaponHash = Animator.StringToHash("AttackWeapon");
        public static readonly int DrawHash = Animator.StringToHash("AttackDraw");
        public static readonly int SheathHash = Animator.StringToHash("AttackSheath");
        public static readonly int EarthDefendHash = Animator.StringToHash("EarthDefend");
        public static readonly int EarthAttackHash = Animator.StringToHash("EarthAttack");

        public static readonly int WaterDefendHash = Animator.StringToHash("WaterDefend");
        public static readonly int WaterAttackHash = Animator.StringToHash("WaterAttack");

        public static readonly int FireDefendHash = Animator.StringToHash("FireDefend");
        public static readonly int FireAttackHash = Animator.StringToHash("FireAttack");

        public static readonly int WindDefendHash = Animator.StringToHash("WindDefend");
        public static readonly int WindAttackHash = Animator.StringToHash("WindAttack");

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

        protected PlayerSkillController GetSkillController()
        {
            return _playerController.GetComponent<PlayerSkillController>();
        }

        /// <summary>
        /// Lấy animation hash dựa trên element cho attack
        /// </summary>
        protected int GetAttackAnimationHash(ElementType element)
        {
            return element switch
            {
                ElementType.Earth => EarthAttackHash,
                ElementType.Water => WaterAttackHash,
                ElementType.Fire => FireAttackHash,
                ElementType.Wind => WindAttackHash,
                _ => EarthAttackHash
            };
        }

        /// <summary>
        /// Lấy animation hash dựa trên element cho defend/shield
        /// </summary>
        protected int GetDefendAnimationHash(ElementType element)
        {
            return element switch
            {
                ElementType.Earth => EarthDefendHash,
                ElementType.Water => WaterDefendHash,
                ElementType.Fire => FireDefendHash,
                ElementType.Wind => WindDefendHash,
                _ => EarthDefendHash
            };
        }
    }
