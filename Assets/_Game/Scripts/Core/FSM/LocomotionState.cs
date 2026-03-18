using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Core.FSM
{
public class LocomotionState : BaseState
{
    public LocomotionState(Game.Features.Player.Controllers.PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        _animator.CrossFade(LocomotionHash, crossFadeDuration);
    }

    public override void FixedUpdate()
    {
    }
}

}
