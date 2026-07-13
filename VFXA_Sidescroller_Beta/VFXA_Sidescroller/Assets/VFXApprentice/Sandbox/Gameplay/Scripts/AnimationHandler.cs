using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementHandler))]
public class AnimationHandler : MonoBehaviour
{
    [SerializeField]
    internal Animator _animator;

    [SerializeField] 
    internal MovementHandler _movementHandler;

    private int _isMovingHash;
    private int _isJumpingHash;
    private int _isAttackingHash;
    private int _attackTypeHash;
    private int _isGroundedHash;

    private void Start()
    {
        _movementHandler = GetComponent<MovementHandler>();
        
        _isMovingHash = Animator.StringToHash("isMoving");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _isAttackingHash = Animator.StringToHash("isAttacking");
        _attackTypeHash = Animator.StringToHash("attackType");
        _isGroundedHash = Animator.StringToHash("isGrounded");
    }

    private void Update()
    {
        // isMoving
        if (_movementHandler.directionalInput.x != 0)
        {
            _animator.SetBool(_isMovingHash, true);
        }
        else
        {
            _animator.SetBool(_isMovingHash, false);
        }
        
        // isJumping
        if (_movementHandler.directionalInput.y != 0)
        {
            _animator.SetBool(_isJumpingHash, true);
        }
        else
        {
            _animator.SetBool(_isJumpingHash, false);
        }
        
        // isAttacking
        if (_movementHandler.isAttacking)
        {
            _animator.SetBool(_isAttackingHash, true);
        }
        else
        {
            _animator.SetBool(_isAttackingHash, false);
        }
        
        // isGrounded
        if (_movementHandler.isGrounded)
        {
            _animator.SetBool(_isGroundedHash, true);
        }
        else
        {
            _animator.SetBool(_isGroundedHash, false);
        }
        
        // attackType
        _animator.SetInteger(_attackTypeHash, _movementHandler._attackType);
    }

}
