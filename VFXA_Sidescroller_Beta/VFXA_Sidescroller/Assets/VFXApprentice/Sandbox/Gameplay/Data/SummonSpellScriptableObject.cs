using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Summon Spell", menuName = "VFX Apprentice/Spells/New Summon Spell")]
public class SummonSpellScriptableObject : SpellScriptableObject
{[Header("Spell Prefabs")] 
    [SerializeField] internal GameObject buildupPrefab;
    [SerializeField] internal GameObject castPrefab;
    [SerializeField] internal GameObject AOEPrefab;

    [Header("Spell Parameters")] 
    [SerializeField] internal float _spawnDelay = 0.35f;
    [SerializeField] internal float _cooldownTime = 0.8f;
    // TODO: Bind
    //[SerializeField] internal float impactRadius = 50.0f;
    //[SerializeField] internal float ragdollForce = 3.0f;
    
    internal enum SummonSpellState
    {
        Delayed,
        Charged,
        Casted,
        Released
    }

    internal SummonSpellState _currentSpellState = SummonSpellState.Delayed;
    
    private GameObject _activeSpell;
    private GameObject _secondActiveSpell;
    
    private float _spellDuration;
    private float _delayElapsed;
    private float _remainingSpellLife;
    
    private ParticleSystem _particleSystem;
    
    private float _spellDirection;
    private Transform _spawnTransform;

    public override void UpdateSpell(Spellbook spellbook)
    {
        switch (_currentSpellState)
        {
            case SummonSpellState.Delayed:
                
                base.UpdateSpell(spellbook);
                
                DestroySpell(_activeSpell);

                _spawnTransform = spellbook._summonSpawnTransform;
                _currentSpellState = SummonSpellState.Charged;
                
                break;
            case SummonSpellState.Charged:
                
                if (_activeSpell == null)
                {
                    // Instantiate builupPrefab at hand location
                    InstantiateSpell(spellbook, buildupPrefab, ref _activeSpell, 1);
                }
                
                // Transition: WindUp -> Cast
                if (!spellbook._isAttackButtonPressed)
                {
                    // Destroy windUp_FX
                    DestroySpell(_activeSpell);
                    _currentSpellState = SummonSpellState.Casted;
                }
                
                break;
            case SummonSpellState.Casted:
                
                InstantiateSpell(spellbook, castPrefab, ref _activeSpell, 2);
                InstantiateSpell(spellbook, AOEPrefab, ref _secondActiveSpell, 3);
                
                _currentSpellState = SummonSpellState.Released;
                
                break;
            case SummonSpellState.Released:
                
                if (_remainingSpellLife > 0)
                {
                    _remainingSpellLife -= Time.deltaTime;
                }
                else
                {
                    DestroySpell(_activeSpell);
                    _currentSpellState = SummonSpellState.Delayed;
                    spellbook._currentSpellbookState = Spellbook.SpellbookState.Cooldown;
                }
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public override void DestroySpell(GameObject spellToDestroy)
    {
        Destroy(spellToDestroy);
    }
    
    public void InstantiateSpell(Spellbook spellbook, GameObject spell, ref GameObject holder, int spellID)
    {
        if (holder == null && spell != null)
        {
            // Apply offset to the spawnPoint
            Vector3 buildupSpawnPosition;
            Vector3 castSpawnPosition;
            Vector3 AOESpawnPosition;
            Vector3 AOESpawnLocation;
            
            var position = _spawnTransform.position;
            _spellDirection = spellbook._movementHandler.currentHorizontalDirection;

            if (_spellDirection > 0)
            {
                buildupSpawnPosition = position + spellbook._summonBuildupOffsetRight;
                castSpawnPosition = position + spellbook._summonCastOffsetRight;
                AOESpawnPosition = position + spellbook._summonAOEOffsetRight;
            }
            else
            {
                buildupSpawnPosition = position + spellbook._summonBuildupOffsetLeft;
                castSpawnPosition = position + spellbook._summonCastOffsetLeft;
                AOESpawnPosition = position + spellbook._summonAOEOffsetLeft;
            }
            
            switch (spellID)
            {
                case 1: // Buildup
                    holder = Instantiate(spell, buildupSpawnPosition, Quaternion.identity);//, spawnPoint);
                    break;
                case 2: // Cast
                    holder = Instantiate(spell, castSpawnPosition, Quaternion.identity);//, spawnPoint);
                    break;
                case 3: // AoE
                    AOESpawnLocation = GetMousePosition();
                    // TODO: refactor to allow free AoE spawn
                    AOESpawnLocation.x = spellbook._dummyTransform.position.x;
                    AOESpawnLocation.y = AOESpawnPosition.y;
                    AOESpawnLocation.z = AOESpawnPosition.z;
                    holder = Instantiate(spell, AOESpawnLocation, Quaternion.identity);//, spawnPoint);
                    break;
                default:
                    break;
            }

            // Get Particle System
            _particleSystem = holder.GetComponent<ParticleSystem>();
            // Get particle system duration
            var main = _particleSystem.main;
            _spellDuration = main.duration; // + main.startLifetimeMultiplier;
            // Set activeSpellTime
            _remainingSpellLife = _spellDuration;
            // Play Particle System
            _particleSystem.Play();
            
            if (spellID != 1)
            {
                // Destroy once the spellDuration has elapsed
                Destroy(holder, _spellDuration);
            }
        }
        

        Vector3 GetMousePosition()
        {
            Vector2 mousePosition = Utility._inputProvider.MousePositionAction();
            return Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));
        }
        
    }
}
