using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Slash Spell", menuName = "VFX Apprentice/Spells/New Slash Spell")]
public class SlashSpellScriptableObject : SpellScriptableObject
{
    [Header("Spell Prefabs")] 
    [SerializeField] internal GameObject slashPrefab;
    [SerializeField] internal GameObject burstPrefab;

    [Header("Spell Parameters")] 
    [SerializeField] internal float _spawnDelay = 0.35f;
    [SerializeField] internal float _cooldownTime = 0.8f;
    // TODO: Bind
    //[SerializeField] internal float impactRadius = 50.0f;
    //[SerializeField] internal float ragdollForce = 3.0f;
    
    internal enum SlashSpellState
    {
        Delayed,
        Charged,
        Casted,
        Released
    }

    private SlashSpellState _currentSpellState = SlashSpellState.Delayed;
    
    private GameObject _activeSpell;
    
    private float _spellDuration;
    private float _delayElapsed;
    private float _remainingSpellLife; // TODO: refactor name

    private ParticleSystem _particleSystem;
    
    private float _spellDirection;
    private Transform _spawnTransform;
    private BoxCollider _weaponCollider;
    
    public override void UpdateSpell(Spellbook spellbook)
    {
        if (_weaponCollider == null)
        {
            _weaponCollider = spellbook._weaponCollider;
        }
        
        switch (_currentSpellState)
        {
            case SlashSpellState.Delayed:
                base.UpdateSpell(spellbook);

                _delayElapsed = _spawnDelay;
                
                // Delayed -> Charged
                _currentSpellState = SlashSpellState.Charged;
                
                break;
            case SlashSpellState.Charged:
                
                if (_delayElapsed > 0)
                {
                    _delayElapsed -= Time.deltaTime;
                }
                else
                {
                    // Charged -> Casted
                    _currentSpellState = SlashSpellState.Casted;
                }
                
                break;
            case SlashSpellState.Casted:
                
                _spawnTransform = spellbook._slashSpawnTransform;
                InstantiateSpell(spellbook, slashPrefab, ref _activeSpell);
                WeaponColliderSwitch(true);
                // Casted -> Released
                _currentSpellState = SlashSpellState.Released;
                
                break;
            case SlashSpellState.Released:

                if (_remainingSpellLife > 0)
                {
                    _remainingSpellLife -= Time.deltaTime;

                    if (_remainingSpellLife < 0.2f && _weaponCollider.enabled)
                    {
                        WeaponColliderSwitch(false);
                    }
                }
                else
                {
                    DestroySpell(_activeSpell);
                    _currentSpellState = SlashSpellState.Delayed;
                    spellbook._coolDownTime = _cooldownTime;
                    spellbook.SwitchSpellbookState(Spellbook.SpellbookState.Cooldown); 
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void InstantiateSpell(Spellbook spellbook, GameObject spell, ref GameObject holder)
    {
        if (holder == null && spell != null)
        {
            // TODO: Handle direction offset boost in a cleaner way
            Vector3 spawnPosition;
            float yRotation;
            var position = _spawnTransform.position;

            if (spellbook._movementHandler.currentHorizontalDirection > 0)
            {
                spawnPosition = position + spellbook._slashOffsetRight;
                yRotation = 180;
            }
            else
            {
                spawnPosition = position + spellbook._slashOffsetLeft;
                yRotation = 0;
            }
            
            Vector3 newRotation = _spawnTransform.rotation.eulerAngles;
            newRotation.y = yRotation;
            
            // Instantiate VFX
            holder = Instantiate(spell, spawnPosition, Quaternion.Euler(newRotation));

            // Particle System
            _particleSystem = holder.GetComponent<ParticleSystem>();
            var main = _particleSystem.main;
            _spellDuration = main.duration; // + main.startLifetimeMultiplier;
            _particleSystem.Play();
            // Destroy
            _remainingSpellLife = _spellDuration;
            Destroy (holder, _spellDuration);
        }
    }
    
    public override void DestroySpell(GameObject spellToDestroy)
    {
        if (spellToDestroy != null)
        {
            if (_particleSystem.isPlaying)
            {
                _particleSystem.Stop();
            }
            
            Destroy(spellToDestroy);
        }
    }
    
    void WeaponColliderSwitch(bool value)
    {
        _weaponCollider.enabled = value;
    }
}
