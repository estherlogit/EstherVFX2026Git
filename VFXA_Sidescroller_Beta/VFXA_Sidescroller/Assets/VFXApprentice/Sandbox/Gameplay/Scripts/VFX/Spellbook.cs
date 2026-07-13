using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spellbook : MonoBehaviour
{
    [Header("Spells")]
    [SerializeField] [NonReorderable]
    public SlashSpellScriptableObject[] slashSpells;
    [SerializeField] [NonReorderable] 
    public CastSpellScriptableObject[] castSpells;
    [SerializeField] [NonReorderable]
    public SummonSpellScriptableObject[] summonSpells;

    [Header("Global Slash Spell Spawn Parameters")]
    [SerializeField] internal Transform _slashSpawnTransform;
    [SerializeField] internal Vector3 _slashOffsetRight;
    [SerializeField] internal Vector3 _slashOffsetLeft;

    [Header("Global Cast Spell Spawn Parameters")]
    [SerializeField] internal Transform _castSpawnTransform;
    [SerializeField] internal Vector3 _castBuildupOffsetRight;
    [SerializeField] internal Vector3 _castBuildupOffsetLeft;
    [SerializeField] internal Vector3 _castOffsetRight;
    [SerializeField] internal Vector3 _castOffsetLeft;
    [SerializeField] internal Vector3 _castProjectileOffsetRight;
    [SerializeField] internal Vector3 _castProjectileOffsetLeft;

    [Header("Global Summon Spell Spawn Parameters")]
    [SerializeField] internal Transform _summonSpawnTransform;
    [SerializeField] internal Vector3 _summonBuildupOffsetRight;
    [SerializeField] internal Vector3 _summonBuildupOffsetLeft;
    [SerializeField] internal Vector3 _summonCastOffsetRight;
    [SerializeField] internal Vector3 _summonCastOffsetLeft;
    [SerializeField] internal Vector3 _summonAOEOffsetRight;
    [SerializeField] internal Vector3 _summonAOEOffsetLeft;

    [Header("External References")] 
    [SerializeField] internal CurrentSpellScriptableObject _currentSpellData;
    [SerializeField] internal MovementHandler _movementHandler;
    [SerializeField] internal Transform _dummyTransform;
    [SerializeField] internal BoxCollider _weaponCollider;

    private SpellScriptableObject _currentSpell;
    internal float _coolDownTime;
    internal bool _isAttackButtonPressed = false;
    
    internal enum SpellbookState
    {
        Ready, 
        Active,
        Cooldown
    }

    internal SpellbookState _currentSpellbookState = SpellbookState.Ready;

    private void Awake()
    {
        SetCurrentSpellData(1, 0);
    }

    private void Update()
    {
        switch (_currentSpellbookState)
        {
            case SpellbookState.Ready:
                InitializeSpell();
                
                // Ready -> Active
                if (_movementHandler.isGrounded && _isAttackButtonPressed && _movementHandler.isLocked)
                {
                    SwitchSpellbookState(SpellbookState.Active);
                }
                
                break;
            case SpellbookState.Active:
                _currentSpell.UpdateSpell(this);
                break;
            case SpellbookState.Cooldown:
                if (_coolDownTime > 0)
                {
                    _coolDownTime -= Time.deltaTime;
                }
                else
                {
                    // Cooldown -> Ready
                    SwitchSpellbookState(SpellbookState.Ready);
                }
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal void SwitchSpellbookState(SpellbookState newState)
    {
        // Guard
        if(_currentSpellbookState == newState) return;

        _currentSpellbookState = newState;
    }

    internal void InitializeSpell()
    {
        // TODO: refactor
        switch (_currentSpellData.spellTypeID)
        {
            case 1:
                _currentSpell = slashSpells[_currentSpellData.spellID];
                break;
            case 2:
                _currentSpell = castSpells[_currentSpellData.spellID];
                break;
            case 3:
                _currentSpell = summonSpells[_currentSpellData.spellID];
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetCurrentSpellData(int spellTypeID, int spellID)
    {
        SetCurrentSpellCategory(spellTypeID);
        SetCurrentSpellCategoryName(spellTypeID);
        SetCurrentSpellName(spellTypeID, spellID);
        SetCurrentSpellNumber(spellID);
        SetCurrentSpellMaxNumber(spellTypeID);
        
        InitializeSpell();
    }

    public void SetCurrentSpellCategory(int spellTypeID)
    {
        _currentSpellData.spellTypeID = spellTypeID;
    }

    public void SetCurrentSpellCategoryName(int spellTypeID)
    {
        switch (spellTypeID)
        {
            case 1:
                _currentSpellData.spellTypeName = "Slash";
                break;
            case 2:
                _currentSpellData.spellTypeName = "Cast";
                break;
            case 3:
                _currentSpellData.spellTypeName = "Summon";
                break;
            default:
                Debug.Log("Invalid spell type ID.");
                break;
        }
    }

    public void SetCurrentSpellName(int spellTypeID, int spellID)
    {
        switch (spellTypeID)
        {
            case 1:
                _currentSpellData.spellName = slashSpells[spellID].name;
                break;
            case 2:
                _currentSpellData.spellName = castSpells[spellID].name;
                break;
            case 3:
                _currentSpellData.spellName = summonSpells[spellID].name;
                break;
            default:
                Debug.Log("Invalid category.");
                break;
        }
    }

    public void SetCurrentSpellNumber(int spellID)
    {
        _currentSpellData.spellID = spellID;
    }
    
    public int GetCurrentSpellNumber()
    {
        return _currentSpellData.spellID;
    }

    public void SetCurrentSpellMaxNumber(int spellTypeID)
    {
        switch (spellTypeID)
        {
            case 1:
                _currentSpellData.spellMaxID = slashSpells.Length;
                break;
            case 2:
                _currentSpellData.spellMaxID = castSpells.Length;
                break;
            case 3:
                _currentSpellData.spellMaxID = summonSpells.Length;
                break;
            default:
                Debug.Log("Invalid category.");
                break;
        }
    }
    
    public int GetCurrentSpellMaxNumber()
    {
        return _currentSpellData.spellMaxID;
    }
}
