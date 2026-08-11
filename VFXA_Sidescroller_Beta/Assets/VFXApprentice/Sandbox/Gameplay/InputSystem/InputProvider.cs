using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputProvider
{
    private static SidescrollerInputActions _inputActions = new();

    
    public void EnableActions()
    {
        _inputActions.PlayerActionMap.MousePosition.Enable();
        _inputActions.PlayerActionMap.Move.Enable();
        _inputActions.PlayerActionMap.Jump.Enable();
        _inputActions.PlayerActionMap.Attack.Enable();
        _inputActions.PlayerActionMap.SelectAttack1.Enable();
        _inputActions.PlayerActionMap.SelectAttack2.Enable();
        _inputActions.PlayerActionMap.SelectAttack3.Enable();
        _inputActions.PlayerActionMap.SelectNextAttack.Enable();
    }

    public void DisableActions()
    {
        _inputActions.PlayerActionMap.MousePosition.Disable();
        _inputActions.PlayerActionMap.Move.Disable();
        _inputActions.PlayerActionMap.Jump.Disable();
        _inputActions.PlayerActionMap.Attack.Disable();
        _inputActions.PlayerActionMap.SelectAttack1.Disable();
        _inputActions.PlayerActionMap.SelectAttack2.Disable();
        _inputActions.PlayerActionMap.SelectAttack3.Disable();
        _inputActions.PlayerActionMap.SelectNextAttack.Disable();
    }

    // ========
    // Actions
    // ========
    
    public Vector2 MousePositionAction()
    {
        return _inputActions.PlayerActionMap.MousePosition.ReadValue<Vector2>();
    }
    
    public float MoveAction()
    {
        return _inputActions.PlayerActionMap.Move.ReadValue<float>();
    }

    public event Action<InputAction.CallbackContext> JumpAction
    {
        add
        {
            _inputActions.PlayerActionMap.Jump.performed += value;
            _inputActions.PlayerActionMap.Jump.canceled += value;
        }
        remove
        {
            _inputActions.PlayerActionMap.Jump.performed -= value;
            _inputActions.PlayerActionMap.Jump.canceled -= value;
        }
    }

    public event Action<InputAction.CallbackContext> AttackAction
    {
        add
        {
            _inputActions.PlayerActionMap.Attack.performed += value;
            _inputActions.PlayerActionMap.Attack.canceled += value;
        }
        remove
        {
            _inputActions.PlayerActionMap.Attack.performed -= value;
            _inputActions.PlayerActionMap.Attack.canceled -= value;
        }
    }
    
    public event Action<InputAction.CallbackContext> SelectAttack1Action
    {
        add
        {
            _inputActions.PlayerActionMap.SelectAttack1.performed += value;
        }
        remove
        {
            _inputActions.PlayerActionMap.SelectAttack1.performed -= value;
        }
    }
    
    public event Action<InputAction.CallbackContext> SelectAttack2Action
    {
        add
        {
            _inputActions.PlayerActionMap.SelectAttack2.performed += value;
        }
        remove
        {
            _inputActions.PlayerActionMap.SelectAttack2.performed -= value;
        }
    }
    
    public event Action<InputAction.CallbackContext> SelectAttack3Action
    {
        add
        {
            _inputActions.PlayerActionMap.SelectAttack3.performed += value;
        }
        remove
        {
            _inputActions.PlayerActionMap.SelectAttack3.performed -= value;
        }
    }
    
    public event Action<InputAction.CallbackContext> SelectNextAttack
    {
        add
        {
            _inputActions.PlayerActionMap.SelectNextAttack.performed += value;
        }
        remove
        {
            _inputActions.PlayerActionMap.SelectNextAttack.performed -= value;
        }
    }
}
