using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Features.Player;
    public class DashState : BaseState
    {
        //ParticleSystem dashParticle;
        float afterImageDuration = 0.2f;
        int afterImageCount = 5;
        float timeBetweenAfterImages = 0.02f;
        public DashState(PlayerController player, Animator animator) : base(player, animator)
        {

        }
        public override void OnEnter()
        {
            Debug.Log("Dash State Entered");

            //Play dash particle effect
            //dashParticle?.Play();
            //TodoLater
            // _playerController.PlayAfterImage(afterImageDuration, afterImageCount, timeBetweenAfterImages);

            _animator.CrossFade(DashHash, crossFadeDuration);
        }
        public override void FixedUpdate()
        {
            //Call player's dash logic and move logic
            _playerController.HandleDash();
        }
        public override void OnExit()
        {
            Debug.Log("Exit Dash State");
        }
    }

