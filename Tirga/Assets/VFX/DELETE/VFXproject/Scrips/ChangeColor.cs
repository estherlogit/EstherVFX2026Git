using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public Color color_i;
    public Color color_o;
    public Color color_p;

    public CollisionSplash emitter_splash;
    private ParticleLauncherColor emitter;
    public CollisionParticleColor emitter_collision;
    void Start()
    {
        emitter = GetComponentInChildren<ParticleLauncherColor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            this.gameObject.GetComponent<Renderer>().material.SetColor("_color", color_i);
            emitter.ChangeParticleColor(color_i);
            emitter_collision.ChangeParticleColor(color_i);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            this.gameObject.GetComponent<Renderer>().material.SetColor("_color", color_o);
            emitter.ChangeParticleColor(color_o);
            emitter_collision.ChangeParticleColor(color_o);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            this.gameObject.GetComponent<Renderer>().material.SetColor("_color", color_p);
            emitter.ChangeParticleColor(color_p);
            emitter_collision.ChangeParticleColor(color_p);
        }

    }



    
}
