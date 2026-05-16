using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State cho việc cast shield/defender skill
/// </summary>
public class CastShieldState : BaseState
{
    private float castDuration = 0.5f;
    private float castTimer = 0f;

    public CastShieldState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        castTimer = 0f;

        // Gọi logic cast shield từ PlayerSkillController
        PlayerSkillController skillController = GetSkillController();
        if (skillController != null)
        {
            // Lấy animation hash dựa trên element hiện tại
            ElementType currentElement = skillController.GetCurrentElement();
            int animationHash = GetDefendAnimationHash(currentElement);
            
            _animator.CrossFade(animationHash, crossFadeDuration);
            skillController.TriggerShieldCast();
        }
    }

    public override void Update()
    {
        castTimer += Time.deltaTime;
    }

    public override void FixedUpdate()
    {
        // Không cho phép di chuyển trong khi casting
    }

    public override void OnExit()
    {
        castTimer = 0f;
    }

    /// <summary>
    /// Kiểm tra xem casting có hoàn thành chưa
    /// </summary>
    public bool IsCastComplete()
    {
        return castTimer >= castDuration;
    }
}
    