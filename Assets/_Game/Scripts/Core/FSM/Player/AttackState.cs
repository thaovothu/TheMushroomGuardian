using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

    public class AttackState : BaseState
    {
        //Todo later add weapon
        // WeaponSO currentWeapon;
        public AttackState(PlayerController player, Animator animator) : base(player, animator)
        {
        }

        public override void OnEnter()
        {
            _animator.CrossFade(AttackHash, 0f);
            _playerController.Attack();
        }

        public override void FixedUpdate()
        {
            //player.HandleMovement();
        }

        public override void OnExit()
        {

        }
    }
