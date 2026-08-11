using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cast Spell", menuName = "VFX Apprentice/Spells/New Cast Spell")]
public class CastSpellScriptableObject : SpellScriptableObject
{
    [Header("Spell Prefabs")] 
    [SerializeField] internal GameObject buildupPrefab;
    [SerializeField] internal GameObject castPrefab;
    [SerializeField] internal GameObject projectilePrefab;
    [SerializeField] internal GameObject impactPrefab;

    [Header("Spell Parameters")] 
    // TODO: Bind
    //[SerializeField] internal Vector3 missileVelocity = new Vector3(1000, 0, 0);
    [SerializeField] internal float _missileSpawnDelay = 0.4f;
    //[SerializeField] internal float impactCollisionRadius = 25;
    //[SerializeField] internal bool missileDestroyOnImpact = true;
    [SerializeField] internal float _projectileDestroyDelay = 0.0f;
    [SerializeField] internal float _projectileDestroyDistance = 0;
    //[SerializeField] internal float ragdollForce = 113.5f;
    //[SerializeField] internal float globalSpellLifetime = 16.35f;
    
    internal enum CastSpellState
    {
        Delayed,
        Charged,
        Casted,
        Released
    }

    private CastSpellState _currentSpellState = CastSpellState.Delayed;

    private GameObject _activeSpell;
    private GameObject _missileSpell;

    private float _spellDuration;
    private float _delayElapsed;
    private float _remainingSpellLife;
    
    private ParticleSystem _particleSystem;

    private float _activeMissileTime;
    private bool _windupCasting = false;
    private bool _casted = false;
    private bool _missileCasted = false;

    private float _spellDirection;
    private Transform _spawnTransform;
    private BoxCollider _projectileCollider;
    
    public override void UpdateSpell(Spellbook spellbook)
    {
        switch (_currentSpellState)
        {
            case CastSpellState.Delayed:
                
                base.UpdateSpell(spellbook);
                
                DestroySpell(_activeSpell);

                _spawnTransform = spellbook._castSpawnTransform;
                _currentSpellState = CastSpellState.Charged;
                
                break;
            case CastSpellState.Charged:

                if (_activeSpell == null && !_windupCasting)
                {
                    _casted = false;
                    _missileCasted = false;
                    _activeMissileTime = 0;
                    
                    // Instantiate builupPrefab at hand location
                    InstantiateSpell(spellbook, buildupPrefab, ref _activeSpell, 1);
                    _windupCasting = true;
                }

                // Transition: Build Up -> Cast
                if (!spellbook._isAttackButtonPressed)
                {
                    DestroySpell(_activeSpell);
                    _windupCasting = false;
                    _currentSpellState = CastSpellState.Casted;
                }
                
                break;
            case CastSpellState.Casted:

                if (!_casted)
                {
                    InstantiateSpell(spellbook, castPrefab, ref _activeSpell, 2);
                    _casted = true;
                }

                if (_activeMissileTime >= _missileSpawnDelay && !_missileCasted)
                {
                    InstantiateSpell(spellbook, projectilePrefab, ref _missileSpell, 3);

                    if (_missileSpell != null && _missileSpell.GetComponent<Projectile>() == null)
                    {
                        _missileSpell.AddComponent<Projectile>();
                    }

                    _activeMissileTime = 0;
                    _missileCasted = true;
                    _currentSpellState = CastSpellState.Released;
                }
                else
                {
                    _activeMissileTime += Time.deltaTime;
                }
                
                break;
            case CastSpellState.Released:

                if (_remainingSpellLife > 0)
                {
                    _remainingSpellLife -= Time.deltaTime;
                }
                else
                {
                    DestroySpell(_activeSpell);
                    _currentSpellState = CastSpellState.Delayed;
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

    private void InstantiateSpell(Spellbook spellbook, GameObject spell, ref GameObject holder, int spellID)
    {
        if (holder == null && spell != null)
        {
            Vector3 buildupSpawnPosition;
            Vector3 castSpawnPosition;
            Vector3 missileSpawnPosition;
            var position = _spawnTransform.position;
            _spellDirection = spellbook._movementHandler.currentHorizontalDirection;
            
            if (_spellDirection > 0)
            {
                buildupSpawnPosition = position + spellbook._castBuildupOffsetRight;
                castSpawnPosition = position + spellbook._castOffsetRight;
                missileSpawnPosition = position + spellbook._castProjectileOffsetRight;
            }
            else
            {
                buildupSpawnPosition = position + spellbook._castBuildupOffsetLeft;
                castSpawnPosition = position + spellbook._castOffsetLeft;
                missileSpawnPosition = position + spellbook._castProjectileOffsetLeft;
            }

            switch (spellID)
            {
                case 1: // Buildup
                    holder = Instantiate(spell, buildupSpawnPosition, Quaternion.identity, _spawnTransform);
                    break;
                case 2: // Cast
                    holder = Instantiate(spell, castSpawnPosition, Quaternion.identity);
                    break;
                case 3: // Projectile
                    holder = Instantiate(spell, missileSpawnPosition, Quaternion.identity);
                    break;
                default:
                    break;
            }

            var localScale = holder.transform.localScale;
            localScale = new Vector3(localScale.x * spellbook._movementHandler.currentHorizontalDirection, localScale.y, localScale.z);
            holder.transform.localScale = localScale;

            // Get Particle System
            _particleSystem = holder.GetComponent<ParticleSystem>();
            // Get particle system duration
            var main = _particleSystem.main;
            _spellDuration = main.duration; // + main.startLifetimeMultiplier;
            // Set _remainingSpellLife
            _remainingSpellLife = _spellDuration;
            // Play Particle System
            _particleSystem.Play();
            
            if (spellID == 2)
            {
                // Destroy once the spellDuration has elapsed
                Destroy (holder, _spellDuration);
            }
        }
    }
}
