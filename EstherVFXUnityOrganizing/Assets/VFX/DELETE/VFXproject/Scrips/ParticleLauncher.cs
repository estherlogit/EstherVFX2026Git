using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLauncher : MonoBehaviour
{
	public ParticleSystem particleLauncher;
    public ParticleSystem splatterParticles;
    public int emitParticles;
    
    List<ParticleCollisionEvent> collisionEvents;
    
    
    // Start is called before the first frame update
    void Start()
    {
        collisionEvents = new List<ParticleCollisionEvent>();
    }
    
    private void OnParticleCollision(GameObject gameObjectName)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(particleLauncher, gameObjectName, collisionEvents);

        for (int i = 0; i < collisionEvents.Count; i++)
        {
    	    EmitAtLocation(collisionEvents[i]);

        }
    }

    
    void EmitAtLocation(ParticleCollisionEvent particleCollisionEvent)
    {
        splatterParticles.transform.position = particleCollisionEvent.intersection;
        //splatterParticles.transform.rotation = Quaternion.LookRotation(particleCollisionEvent.normal);
        splatterParticles.Emit(emitParticles);
    }

    private void Update()
    {
        if (Input.GetButton("Fire1"))
        {
    	    particleLauncher.Emit(1);
        }
    }
}
