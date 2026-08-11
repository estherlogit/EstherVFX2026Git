using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MovementHandler))]
public class InputTarget : MonoBehaviour
{
    
    internal MovementHandler _movementHandler;
    internal Spellbook _spellbook;
    internal UIHandler _uiHandler;

    private InputProvider _inputProvider;

    private void OnEnable()
    {
        _inputProvider = new InputProvider();
        // TODO: refactor
        _spellbook = GameObject.Find("Create Spells").GetComponent<Spellbook>();
        _uiHandler = GameObject.Find("UI").GetComponent<UIHandler>();

        _inputProvider.JumpAction += Jump;
        _inputProvider.AttackAction += Attack;
        _inputProvider.SelectAttack1Action += SelectAttack1;
        _inputProvider.SelectAttack2Action += SelectAttack2;
        _inputProvider.SelectAttack3Action += SelectAttack3;
        _inputProvider.SelectNextAttack += SelectNextAttack;
        
        _inputProvider.EnableActions();
    }

    private void OnDisable()
    {
        _inputProvider.JumpAction -= Jump;
        _inputProvider.AttackAction -= Attack;
        _inputProvider.SelectAttack1Action -= SelectAttack1;
        _inputProvider.SelectAttack2Action -= SelectAttack2;
        _inputProvider.SelectAttack3Action -= SelectAttack3;
        _inputProvider.SelectNextAttack -= SelectNextAttack;
        
        _inputProvider.DisableActions();
    }

    void Start()
    {
        _movementHandler = GetComponent<MovementHandler>();
    }

    private void Update()
    {
        _movementHandler.SetHorizontalInput(_inputProvider.MoveAction());
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && _movementHandler.isGrounded && !_movementHandler.isLocked)
        {
            _movementHandler.OnJumpInputDown();
        }
        
        if (context.canceled)
        {
            _movementHandler.OnJumpInputUp();
        }
    }
    
    
    private void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && _movementHandler.isGrounded)
        {
            _movementHandler.OnAttackInputDown();
            _spellbook._isAttackButtonPressed = true;
        }
        
        if (context.canceled)
        {
            _movementHandler.OnAttackInputUp();
            _spellbook._isAttackButtonPressed = false;
        }
    }
    
    private void SelectAttack1(InputAction.CallbackContext context)
    {
        _movementHandler._attackType = 1;
        _spellbook.SetCurrentSpellData(1, 0);
        _uiHandler.UpdateUI();
    }
    
    private void SelectAttack2(InputAction.CallbackContext context)
    {
        _movementHandler._attackType = 2;
        _spellbook.SetCurrentSpellData(2, 0);
        _uiHandler.UpdateUI();
    }
    
    private void SelectAttack3(InputAction.CallbackContext context)
    {
        _movementHandler._attackType = 3;
        _spellbook.SetCurrentSpellData(3, 0);
        _uiHandler.UpdateUI();
    }
    
    private void SelectNextAttack(InputAction.CallbackContext context)
    {
        var currentSpellID = _spellbook._currentSpellData.spellID;
        var newSpellID = 0;
        
        if (_spellbook._currentSpellData.spellID < _spellbook._currentSpellData.spellMaxID - 1)
        {
            newSpellID = currentSpellID + 1;
        }
        
        _spellbook.SetCurrentSpellData(_spellbook._currentSpellData.spellTypeID, newSpellID);
        _uiHandler.UpdateUI();
    }
}
