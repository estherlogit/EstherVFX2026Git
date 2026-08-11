using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MovementHandler))]
public class AnimationStateMachine : StateMachine
{

    public Animator _animator;
    internal MovementHandler _movementHandler;
    internal AnimationState _currentState;

    //===================
    // LOCOMOTION STATES
    //===================

    public readonly AnimationState IdleState = new IdleState();
    public readonly AnimationState JumpState = new JumpState();
    public readonly AnimationState RunState = new RunState();
    public readonly AnimationState RunToIdleState = new RunToIdleState();
    
    internal readonly string IDLE = "Idle";
    internal readonly string RUN = "Run";
    internal readonly string JUMP = "Jump";
    internal readonly string RUN_TO_IDLE = "RunToIdle";
    
    //===============
    // ATTACK STATES
    //===============
    
    public readonly AnimationState SlashState = new SlashState();
    public readonly AnimationState SummonChargeState = new SummonChargeState();
    public readonly AnimationState SummonLoopState = new SummonLoopState();
    public readonly AnimationState SummonReleaseState = new SummonReleaseState();
    public readonly AnimationState CastChargeState = new CastChargeState();
    public readonly AnimationState CastLoopState = new CastLoopState();
    public readonly AnimationState CastReleaseState = new CastReleaseState();
    
    internal readonly string SLASH = "SlashOverhead";
    internal readonly string SUMMON_CHARGE = "SummonIn";
    internal readonly string SUMMON_LOOP = "SummonLoop";
    internal readonly string SUMMON_RELEASE = "SummonCast";
    internal readonly string CAST_CHARGE = "CastForwardIn";
    internal readonly string CAST_LOOP = "CastForwardLoop";
    internal readonly string CAST_RELEASE = "CastForward";
    
    //========
    // HASHES
    //========

    internal int _isGroundedHash;
    internal int _isLockedHash;
    internal int _attackTypeHash;
    
    
    internal int _isIdlingHash;
    internal int _isRunningHash;
    internal int _isRunToIdlingHash;
    internal int _isJumpingHash;
    
    internal int _isChargingAttackHash;
    internal int _isAttackingHash;
    internal int _isReleasingAttackHash;

    private void Start()
    {
        _movementHandler = GetComponent<MovementHandler>();

        _isGroundedHash = Animator.StringToHash("isGrounded");
        _isLockedHash = Animator.StringToHash("isLocked");
        _attackTypeHash = Animator.StringToHash("attackType");
        
        _isIdlingHash = Animator.StringToHash("isIdling");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isRunToIdlingHash = Animator.StringToHash("isRunToIdling");
        _isJumpingHash = Animator.StringToHash("isJumping");

        _isChargingAttackHash = Animator.StringToHash("isChargingAttack");
        _isAttackingHash = Animator.StringToHash("isAttacking");
        _isReleasingAttackHash = Animator.StringToHash("isReleasingAttack");
        
        _currentState = IdleState;
        _currentState.OnStateEnter(this);
    }

    private void Update()
    {
        // TODO: refactor
        _animator.SetBool(_isGroundedHash, _movementHandler.isGrounded);
        _animator.SetBool(_isLockedHash, _movementHandler.isLocked);
        _animator.SetInteger(_attackTypeHash, _movementHandler._attackType);

        _currentState.OnStateUpdate(this);
    }

    internal void SwitchState(AnimationState newState)
    {
        // Guard
        if(_currentState == newState) return;

        _currentState = newState;
        _currentState.OnStateEnter(this);
    }
}
