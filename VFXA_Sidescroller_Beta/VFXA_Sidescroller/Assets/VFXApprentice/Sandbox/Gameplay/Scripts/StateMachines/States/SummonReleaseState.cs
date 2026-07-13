
public class SummonReleaseState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._animator.SetBool(stateMachine._isReleasingAttackHash, true);
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        // Transition: Once the animation has finished
        if (stateMachine._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.75f)
        {
            stateMachine._movementHandler.isAttacking = false;
            stateMachine._movementHandler.isLocked = false;
            stateMachine._animator.SetBool(stateMachine._isReleasingAttackHash, false);
            
            // Transition: SummonRelease -> Jump
            if (stateMachine._movementHandler.velocity.y != 0)
            {
                stateMachine.SwitchState(stateMachine.JumpState);
            }
        
            // Transition: SummonRelease -> Run
            if(stateMachine._movementHandler.isGrounded && stateMachine._movementHandler.directionalInput.x != 0)
            {
                stateMachine.SwitchState(stateMachine.RunState);
            }
        
            // Transition: SummonRelease -> Idle 
            if (stateMachine._movementHandler.directionalInput.x == 0)
            {
                stateMachine.SwitchState(stateMachine.IdleState);
            }
        }
    }
}
