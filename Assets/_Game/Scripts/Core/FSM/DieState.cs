using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Features.Player;
namespace Game.Core.FSM
{
    public class DieState : BaseState
    {
    public DieState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        Debug.Log("Die State Entered");
        _animator.CrossFade(dieHash, crossFadeDuration);
    }
}
}