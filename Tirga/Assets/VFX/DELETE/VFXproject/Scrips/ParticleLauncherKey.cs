using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLauncherKey : MonoBehaviour
{
    public ParticleSystem particleLauncher;
    public ParticleSystem splatterParticles;
    public ParticleDecalPool splatDecalPool;


    List<ParticleCollisionEvent> collisionEvents;


    void Start()
    {

        collisionEvents = new List<ParticleCollisionEvent>();
    }
    void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents (particleLauncher, other, collisionEvents);

        for (int i =0; i < collisionEvents.Count; i++)
        {

            EmitAtLocation (collisionEvents[i]);
        }

        var parentcolor = GetComponentInParent<Renderer>().material.GetColor("_color");

        if (other.layer == 12)
        for (int i = 0; i < collisionEvents.Count; i++)
        {
            splatDecalPool.ParticleHit(collisionEvents[i], parentcolor);
        }


    }

    void EmitAtLocation(ParticleCollisionEvent particleCollisionEvent)

    {
        Debug.Log("emitting");
        splatterParticles.transform.position = particleCollisionEvent.intersection;
        splatterParticles.transform.rotation = Quaternion.LookRotation(particleCollisionEvent.normal);
        splatterParticles.Emit(3);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.L))
        {
            particleLauncher.Emit(1);
        }

    }

    public void ChangeParticleColor(Color newColor)
    {
        Debug.Log(newColor);
        GetComponent<ParticleSystemRenderer>().material.SetColor("_color", newColor);
        GetComponent<ParticleSystemRenderer>().trailMaterial.SetColor("_color", newColor);

    }


   
}
