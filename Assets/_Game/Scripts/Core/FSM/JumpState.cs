using System.Collections;
using System.Collections.Generic;
using Game.Features.Core.FSM;
using UnityEngine;
namespace Game.Features.Core.FSM
{
    public class JumpState : BaseState 
    {
        public JumpState(Game.Features.Player.Controllers.PlayerController player, Animator animator) : base(player, animator)
        {
        }
        public  override void OnEnter()
        {
            _animator.CrossFade(JumpHash, crossFadeDuration);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
        }
    }
}
