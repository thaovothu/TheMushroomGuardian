// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System;
// using Game.Features.Player;

// namespace Game.Core.FSM
// {
//     public class AttackState : BaseState
//     {
//         //Todo later add weapon
//         // WeaponSO currentWeapon;
//         // public AttackState(PlayerController player, Animator animator, WeaponCollider weapon) : base(player, animator)
//         // {
//         //     if (weapon != null)
//         //     {
//         //         currentWeapon = weapon.weaponData;
//         //     }
//         // }

//         public override void OnEnter()
//         {
//             // if (currentWeapon == null) return;
//             // if (currentWeapon.AnimationHash == 0) return;

//             // _animator.CrossFade(currentWeapon.AnimationHash, 0f);
//             //animator.CrossFade(AttackHash, 0f); //Set crossFadeDuaration = 0 to play fullfill attack animation
//             //player.Attack();
//         }

//         public override void FixedUpdate()
//         {
//             //player.HandleMovement();
//         }

//         public override void OnExit()
//         {

//         }
//     }
// }