using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLauncherColor : MonoBehaviour
{
    public ParticleSystem particleLauncher;
    public ParticleSystem collisionParticles;
    public CollisionSplash splashParticles;

    List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {

        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(particleLauncher, other, collisionEvents);

        for (int i = 0; i < collisionEvents.Count; i++)
        {

            EmitAtLocation(collisionEvents[i]);
        }

        var parentcolor = GetComponentInParent<Renderer>().material.GetColor("_color");

        if (other.layer == 12)
            for (int i = 0; i < collisionEvents.Count; i++)
            {
                splashParticles.ParticleHit(collisionEvents[i], parentcolor);
            }

    }

    void EmitAtLocation(ParticleCollisionEvent particleCollisionEvent)

    {
        collisionParticles.transform.position = particleCollisionEvent.intersection;
        collisionParticles.transform.rotation = Quaternion.LookRotation(particleCollisionEvent.normal);
        collisionParticles.Emit(3);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.L))
        {
            particleLauncher.Emit(1);
        }  
    }

    public void ChangeParticleColor(Color newColor)
    {
        GetComponent<ParticleSystemRenderer>().material.SetColor("_color", newColor);
        GetComponent<ParticleSystemRenderer>().trailMaterial.SetColor("_color", newColor);

    }
}
