
public class RunToIdleState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._movementHandler.isAttacking = false;
        stateMachine._movementHandler.isLocked = false;
        stateMachine._animator.SetBool(stateMachine._isRunToIdlingHash, true);
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        //if(!stateMachine._movementHandler.isGrounded) return;
        
        // Transition: RunToIdle -> Jump
        if (stateMachine._movementHandler.velocity.y != 0)
        {
            stateMachine._animator.SetBool(stateMachine._isRunToIdlingHash, false);
            stateMachine.SwitchState(stateMachine.JumpState);
        }
        
        // Transition: RunToIdle -> Run
        if(stateMachine._movementHandler.isGrounded && stateMachine._movementHandler.directionalInput.x != 0)
        {
            stateMachine._animator.SetBool(stateMachine._isRunToIdlingHash, false);
            stateMachine.SwitchState(stateMachine.RunState);
        }
        
        // Transition: RunToIdle -> Idle once the animation has finished
        if (stateMachine._movementHandler.directionalInput.x == 0) // && stateMachine._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
        {
            stateMachine._animator.SetBool(stateMachine._isRunToIdlingHash, false);
            stateMachine.SwitchState(stateMachine.IdleState);
        }
        
        // Transition: RunToIdle -> Attack
        if (stateMachine._movementHandler.isAttacking)
        {
            stateMachine._animator.SetBool(stateMachine._isRunToIdlingHash, false);
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
