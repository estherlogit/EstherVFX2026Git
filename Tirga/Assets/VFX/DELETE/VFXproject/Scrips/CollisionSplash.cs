using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSplash : MonoBehaviour
{
    public int maxSplash = 100;
    public float splashSizeMin = .5f;
    public float splashSizeMax = 2;


    private ParticleSystem splashParticleSystem;
    private int particleSplashlDataIndex;
    private ParticleSplashData[] particleData;
    private ParticleSystem.Particle[] particles;
    private ParticleLauncherKey emitter;


    void Start()
    {
        splashParticleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[maxSplash];
        particleData = new ParticleSplashData[maxSplash];
        for (int i = 0; i < maxSplash; i++)
        {
            particleData[i] = new ParticleSplashData();
        }
    }

    public void ParticleHit(ParticleCollisionEvent particleCollisionEvent, Color parentcolor)
    {
        SetParticleData(particleCollisionEvent, parentcolor);
        DisplayParticles();
    }


    public void SetParticleData(ParticleCollisionEvent particleCollisionEvent, Color parentcolor)
    {
        if (particleSplashlDataIndex >= maxSplash)
        {
            particleSplashlDataIndex = 0;
        }

        particleData[particleSplashlDataIndex].position = particleCollisionEvent.intersection;
        Vector3 particleRotationEuler = Quaternion.LookRotation(particleCollisionEvent.normal).eulerAngles;
        particleRotationEuler.z = Random.Range(0, 360);
        particleData[particleSplashlDataIndex].rotation = particleRotationEuler;
        particleData[particleSplashlDataIndex].size = Random.Range(splashSizeMin, splashSizeMax);
        particleData[particleSplashlDataIndex].color = parentcolor;


        particleSplashlDataIndex++;

    }

    public void ChangeParticleColor(Color newColor)
    {
        GetComponent<ParticleSystemRenderer>().material.SetColor("_color", newColor);
        particleSplashlDataIndex++;
    }

    public void DisplayParticles()
    {
        for (int i = 0; i < particleData.Length; i++)
        {

            particles[i].position = particleData[i].position;
            particles[i].rotation3D = particleData[i].rotation;
            particles[i].startSize = particleData[i].size;
            particles[i].startColor = particleData[i].color;
        }
        splashParticleSystem.SetParticles(particles, particles.Length);



    }

}
