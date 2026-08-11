
public class IdleState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._movementHandler.isAttacking = false;
        stateMachine._movementHandler.isLocked = false;
        stateMachine._animator.SetBool(stateMachine._isIdlingHash, true);
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        //if(!stateMachine._movementHandler.isGrounded) return;

        // Transition: Idle -> Jump
        if (stateMachine._movementHandler.velocity.y != 0)
        {
            stateMachine._animator.SetBool(stateMachine._isIdlingHash, false);
            stateMachine.SwitchState(stateMachine.JumpState);
        }
        
        // Transition: Idle -> Run
        if (stateMachine._movementHandler.isGrounded && stateMachine._movementHandler.directionalInput.x != 0)
        {
            stateMachine._animator.SetBool(stateMachine._isIdlingHash, false);
            stateMachine.SwitchState((stateMachine.RunState));
        }
        
        // Transition: Idle -> Attack
        if (stateMachine._movementHandler.isAttacking)
        {
            stateMachine._animator.SetBool(stateMachine._isIdlingHash, false);
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
