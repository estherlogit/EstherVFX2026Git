
public class RunState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._movementHandler.isAttacking = false;
        stateMachine._movementHandler.isLocked = false;
        stateMachine._animator.SetBool(stateMachine._isRunningHash, true);
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        //if (!stateMachine._movementHandler.isGrounded) return;
        
        // Transition: Run -> Jump
        if (stateMachine._movementHandler.velocity.y != 0)
        {
            stateMachine._animator.SetBool(stateMachine._isRunningHash, false);
            stateMachine.SwitchState(stateMachine.JumpState);
        }
        
        // Transition: Run -> RunToIdle
        if (stateMachine._movementHandler.directionalInput.x == 0)
        {
            stateMachine._animator.SetBool(stateMachine._isRunningHash, false);
            stateMachine.SwitchState(stateMachine.RunToIdleState);
        }
        
        // Transition: Run -> Attack
        if (stateMachine._movementHandler.isAttacking)
        {
            stateMachine._animator.SetBool(stateMachine._isRunningHash, false);
            switch (stateMachine._movementHandler._attackType)
            {
                case 1:
                    stateMachine.SwitchState(stateMachine.SlashState);
                    break;
                case 2:
                    stateMachine.SwitchState(stateMachine.CastChargeState);
                    break;
                case 3:
                    stateMachine.SwitchState(stateMachine.SummonChargeState);
                    break;
                default:
                    break;
            }
        }
    }
}
