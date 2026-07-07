using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionParticleColor : MonoBehaviour
{
    public Color parentcolor;
    private ParticleLauncherColor emitter;

    public void ChangeParticleColor(Color newColor)

    {
        GetComponent<ParticleSystemRenderer>().trailMaterial.SetColor("_color", newColor);
        GetComponent<ParticleSystemRenderer>().material.SetColor("_color", newColor);
    }

}
