
public class SlashState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._movementHandler.isAttacking = true;
        stateMachine._movementHandler.isLocked = true;
        stateMachine._animator.SetBool(stateMachine._isAttackingHash, true);
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        //if (!stateMachine._movementHandler.isGrounded) return;

        // Transition: Once the animation has finished
        if (stateMachine._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            stateMachine._movementHandler.isAttacking = false;
            stateMachine._animator.SetBool(stateMachine._isAttackingHash, false);
            
            // Transition: Slash -> Jump
            if (stateMachine._movementHandler.velocity.y != 0)
            {
                stateMachine.SwitchState(stateMachine.JumpState);
            }

            // Transition: Slash -> Run
            if (stateMachine._movementHandler.isGrounded && stateMachine._movementHandler.directionalInput.x != 0)
            {
                stateMachine.SwitchState(stateMachine.RunState);
            }

            // Transition: Slash -> Idle 
            if (stateMachine._movementHandler.directionalInput.x == 0)
            {
                stateMachine.SwitchState(stateMachine.IdleState);
            }
        }
    }
}
