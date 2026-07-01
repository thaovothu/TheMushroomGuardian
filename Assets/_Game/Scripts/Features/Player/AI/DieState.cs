using UnityEngine;

    public class DieState : BaseState
    {
    public DieState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        //Debug.Log("Die State Entered");
        _animator.CrossFade(DieHash, crossFadeDuration);
    }
}
