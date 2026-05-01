// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace Game.Core.FSM
// {
//     public class SkillState : StateBase
// {
//     private ISkill skill;

//     public SkillState(PlayerController player, Animator animator, ISkill skill)
//         : base(player, animator)
//     {
//         this.skill = skill;
//     }

//     public override void OnEnter()
//     {
//         if (player.mana.HasEnoughMana(skill.ManaCost) && skill.CanCast(player))
//         {
//             player.mana.UseMana(skill.ManaCost);
//             //Debug.Log("Player use " + skill.Name + " skill, mana cost: " + skill.ManaCost);
//             animator.CrossFade(skill.AnimationHash, crossFadeDuration);
//             skill.Activate(player);
//         }
//         else
//         {
//             player.StopSkillTimer(skill);
//             //player.stateMachine.RevertToPreviousState();
//         }
//     }


//     public override void OnFixedUpdate()
//     {
//         //Call player's skill logic and move logic
//         if (skill.Name == "Q")
//         {
//             player.HandleMovement();
//         }
//     }

//     public override void OnExit()
//     {
//         skill.Deactivate();
//     }
// }
// }