using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDecalPool : MonoBehaviour
{
    public int maxDecals = 100;
    public float decalSizeMin = .5f;
    public float decalSizeMax = 2;
   

    private ParticleSystem decalParticleSystem;
    private int particleDecalDataIndex;
    private ParticleDecalData[] particleData;
    private ParticleSystem.Particle[] particles;
    private ParticleLauncherKey emitter;
    void Start ()
    {
        decalParticleSystem = GetComponent<ParticleSystem> ();
        particles = new ParticleSystem.Particle[maxDecals];
        particleData = new ParticleDecalData[maxDecals];
        for (int i = 0; i < maxDecals; i++)
        {
            particleData [i] = new ParticleDecalData();
        }
    }

    public void ParticleHit(ParticleCollisionEvent particleCollisionEvent, Color parentcolor)
    {
        SetParticleData (particleCollisionEvent,  parentcolor);
        DisplayParticles ();
    }

 

    public void SetParticleData(ParticleCollisionEvent particleCollisionEvent, Color parentcolor)
    {
        if (particleDecalDataIndex >= maxDecals)
        {
            particleDecalDataIndex = 0;
        }

        particleData [particleDecalDataIndex].position = particleCollisionEvent.intersection;
        Vector3 particleRotationEuler = Quaternion.LookRotation(particleCollisionEvent.normal).eulerAngles;
        particleRotationEuler.z = Random.Range (0, 360);
        particleData [particleDecalDataIndex].rotation = particleRotationEuler;
        particleData [particleDecalDataIndex].size = Random.Range(decalSizeMin, decalSizeMax);    
        particleData[particleDecalDataIndex].color = parentcolor; 
       



        particleDecalDataIndex++;

        }

   public void ChangeParticleColor(Color newColor)
    {
        Debug.Log("splash " + newColor);
        GetComponent<ParticleSystemRenderer>().material.SetColor("_color", newColor);
        particleDecalDataIndex++;
    }

    public void DisplayParticles()
    {
        for (int i = 0; i < particleData.Length; i++)
        {
            
            particles [i].position = particleData [i].position;
            particles[i].rotation3D = particleData[i].rotation;
            particles[i].startSize = particleData [i].size;
            particles[i].startColor = particleData [i].color;
        }
        Debug.Log("display");
        decalParticleSystem.SetParticles (particles, particles.Length);
       


    }

}
