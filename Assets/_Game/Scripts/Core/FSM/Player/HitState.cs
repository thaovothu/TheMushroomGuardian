using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
    public class HitState : BaseState
{
    public HitState(PlayerController player, Animator animator) : base(player, animator)
    {
    }
    public override void OnEnter()
    {
        Debug.Log("Hit State Entered");
        _animator.CrossFade(HitHash, 0f);
        _playerController.StartHitTimer();
    }
    public override void Update()
    {
        // if (timer <= 0f)
        // {
        //     _playerController.stateMachine.RevertToPreviousState();
        // }
    }
    public override void FixedUpdate()
    {
        _playerController.HandleMovement();
    }
    public override void OnExit()
    {
        _playerController.heathSystem.ClearHit();
        Debug.Log("Exit Hit State");
    }
}
