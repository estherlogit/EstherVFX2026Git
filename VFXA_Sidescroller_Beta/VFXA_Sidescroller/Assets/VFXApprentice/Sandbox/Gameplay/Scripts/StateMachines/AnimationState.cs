
public abstract class AnimationState : State
{
    public abstract void OnStateEnter(AnimationStateMachine stateMachine);

    public abstract void OnStateUpdate(AnimationStateMachine stateMachine);
}
