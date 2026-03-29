// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System;
// using Game.Features.Player;
// namespace Game.Core.FSM
// {
//     public class HitState : StateBase
// {
//     float hitDuration = 0.5f;
//     float timer;
//     public HitState(PlayerController player, Animator animator) : base(player, animator)
//     {
//     }
//     public override void OnEnter()
//     {
//         Debug.Log("Hit State Entered");
//         _animator.CrossFade(HitHash, 0f);
//         timer = hitDuration;
//     }
//     public override void OnUpdate()
//     {
//         timer -= Time.deltaTime;
//         if (timer <= 0f)
//         {
//             _playerController.stateMachine.RevertToPreviousState();
//         }
//     }
//     public override void FixedUpdate()
//     {
//         // Handle any movement or logic while in the hit state if necessary
//         _playerController.HandleMovement();
//     }
//     public override void OnExit()
//     {
//         Debug.Log("Exit Hit State");
//     }
// }
// }