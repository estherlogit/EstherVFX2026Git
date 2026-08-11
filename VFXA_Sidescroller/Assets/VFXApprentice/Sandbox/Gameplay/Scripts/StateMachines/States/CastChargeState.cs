
public class CastChargeState : AnimationState
{
    public override void OnStateEnter(AnimationStateMachine stateMachine)
    {
        stateMachine._movementHandler.isAttacking = true;
        stateMachine._movementHandler.isLocked = true;
        stateMachine._animator.SetBool(stateMachine._isChargingAttackHash, true);
    }

    public override void OnStateUpdate(AnimationStateMachine stateMachine)
    {
        // Transition: CastCharge -> CastLoop once the animation has finished
        if (stateMachine._animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            stateMachine._animator.SetBool(stateMachine._isChargingAttackHash, false);
            stateMachine.SwitchState(stateMachine.CastLoopState);
        }
    }
}
