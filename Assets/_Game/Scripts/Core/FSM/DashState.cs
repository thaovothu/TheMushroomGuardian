using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Features.Player;
namespace Game.Core.FSM
{
    public class DashState : BaseState
{
    //ParticleSystem dashParticle;
    float afterImageDuration = 0.2f;
    int afterImageCount = 5;
    float timeBetweenAfterImages = 0.02f;
    public DashState(PlayerController player, Animator animator, Material dashMaterial) : base(player, animator)
    {

    }
    public override void OnEnter()
    {
        Debug.Log("Dash State Entered");

        //Play dash particle effect
        //dashParticle?.Play();
        //TodoLater
        // _playerController.PlayAfterImage(afterImageDuration, afterImageCount, timeBetweenAfterImages);

        _animator.CrossFade(JumpHash, crossFadeDuration);
    }
    public override void FixedUpdate()
    {
        //Call player's dash logic and move logic
        _playerController.HandleMovement();
    }
    public override void OnExit()
    {
        Debug.Log("Exit Dash State");
    }
}

}