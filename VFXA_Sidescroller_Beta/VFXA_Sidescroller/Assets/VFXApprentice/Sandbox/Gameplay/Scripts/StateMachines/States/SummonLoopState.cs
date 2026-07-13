
public class SummonLoopState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._animator.SetBool(stateMachine._isAttackingHash, true);
        stateMachine._movementHandler.isLocked = true;
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        // Transition: Once the mouse button is released
        if (!stateMachine._movementHandler.isAttacking)
        {
            stateMachine._animator.SetBool(stateMachine._isAttackingHash, false);
            // Transition: SummonLoop -> SummonRelease
            stateMachine.SwitchState(stateMachine.SummonReleaseState);
        }
    }
}
