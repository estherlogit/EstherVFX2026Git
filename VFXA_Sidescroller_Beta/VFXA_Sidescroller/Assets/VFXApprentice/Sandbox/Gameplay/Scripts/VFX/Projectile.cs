using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Projectile : MonoBehaviour
{
    // 2D controller of this movable
    internal Spellbook spellbook;

    // Speed of the projectile
    private float moveSpeed = 15;
    // Acceleration 
    private float accelerationTime = 0.05f;
    // Projectile's velocity
    internal Vector2 velocity;

    // Horizontal movement smoothing
    private float velocityXSmoothing;
    // Direction
    private Vector2 directionalInput;
    // Right = 1 , Left = -1
    public float currentHorizontalDirection;

    // Projectile 
    private BoxCollider projectileCollider;
    private float projectileDirection;
    private float distanceTravelled;
    private CastSpellScriptableObject spell;

    void Start()
    {
        spellbook = GameObject.Find("Create Spells").GetComponent<Spellbook>();

        projectileDirection = spellbook._movementHandler.currentHorizontalDirection;
        spell = spellbook.castSpells[spellbook._currentSpellData.spellID];

        distanceTravelled = 0;
        velocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        CalculateVelocity();
        Move(velocity * Time.deltaTime);
    }

    private void CalculateVelocity()
    {
        // Calculates horizontal speed
        float targetVelocityX = projectileDirection * moveSpeed;
        // Sets horizontal speed
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTime);
    }

    private void Move(Vector2 deltaMovement)
    {
        transform.Translate(deltaMovement);
        Physics.SyncTransforms();
        distanceTravelled += deltaMovement.x;
        
        if (Mathf.Abs(distanceTravelled) >= spell._projectileDestroyDistance)
        { 
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        foreach(ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>())
        {
            if(particleSystem.main.ringBufferMode == ParticleSystemRingBufferMode.LoopUntilReplaced ||
               particleSystem.main.ringBufferMode == ParticleSystemRingBufferMode.PauseUntilReplaced)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        
        Destroy(gameObject, spell._projectileDestroyDelay);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            DestroyProjectile();
        }
    }
}
