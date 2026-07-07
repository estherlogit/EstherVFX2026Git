using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSplatterColor : MonoBehaviour
{
    public Color parentcolor;
    private ParticleLauncherKey emitter;

    //new for ParticleDecalPool
    public ParticleSystem splatterParticles;


    public void ChangeParticleColor(Color newColor)
    {
        Debug.Log("splatter");
        GetComponent<ParticleSystemRenderer>().trailMaterial.SetColor("_color", newColor);
        GetComponent<ParticleSystemRenderer>().material.SetColor("_color", newColor);
    }
}
