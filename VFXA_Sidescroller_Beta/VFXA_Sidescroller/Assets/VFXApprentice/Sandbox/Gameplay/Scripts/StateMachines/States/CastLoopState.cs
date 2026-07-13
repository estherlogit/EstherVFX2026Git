
public class CastLoopState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._animator.SetBool(stateMachine._isAttackingHash, true);
        stateMachine._movementHandler.isLocked = true;
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        // Transition: Once the mouse button is released
        if (!stateMachine._movementHandler.isAttacking && stateMachine._movementHandler.isLocked)
        {
            stateMachine._animator.SetBool(stateMachine._isAttackingHash, false);
            // Transition: CastForwardLoop -> CastForward
            stateMachine.SwitchState(stateMachine.CastReleaseState);
        }
    }
}
