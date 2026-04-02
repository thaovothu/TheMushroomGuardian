using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Features.Player;

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
            //animator.CrossFade(AttackHash, 0f); //Set crossFadeDuaration = 0 to play fullfill attack animation
            // player.Attack();
        }

        public override void FixedUpdate()
        {
            //player.HandleMovement();
        }

        public override void OnExit()
        {

        }
    }
