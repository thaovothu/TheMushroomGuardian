using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

    public class CombatState : BaseState
    {
        //Todo later add weapon
        // WeaponSO currentWeapon;
        public CombatState(PlayerController player, Animator animator) : base(player, animator)
        {
        }

        public override void OnEnter()
        {
            //Debug.Log("Enter Combat State");
            _animator.CrossFade(AttackWeaponHash, 0f);
            _playerController.CombatAttack();
        }

        public override void FixedUpdate()
        {
            //player.HandleMovement();
        }

        public override void OnExit()
        {

        }
    }
