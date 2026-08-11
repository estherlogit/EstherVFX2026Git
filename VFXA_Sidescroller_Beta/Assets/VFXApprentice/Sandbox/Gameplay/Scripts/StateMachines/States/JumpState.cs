
public class JumpState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._movementHandler.isAttacking = false;
        stateMachine._animator.SetBool(stateMachine._isJumpingHash, true);
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        if (stateMachine._movementHandler.isGrounded)
        {
            stateMachine._animator.SetBool(stateMachine._isJumpingHash, false);
            
            if (stateMachine._movementHandler.velocity.x != 0)
            {
                // Transition: Jump -> Run
                stateMachine.SwitchState(stateMachine.RunState);
            }
            else
            {
                // Transition: Jump -> Idle
                stateMachine.SwitchState(stateMachine.IdleState);
            }
        }
    }
}
