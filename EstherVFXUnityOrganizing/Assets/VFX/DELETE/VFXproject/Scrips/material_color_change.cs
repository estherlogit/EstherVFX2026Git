using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class material_color_change : MonoBehaviour
{ 
    private ParticleSystem ps;
    private Material trailMat;


    void Start()
    {
        var child = this.gameObject.transform.GetChild(0);
        ps = child.GetComponent<ParticleSystem>();

        var trails = ps.trails;
        trailMat = child.GetComponent<ParticleSystemRenderer>().trailMaterial;
    }

    private void OnParticleCollision(GameObject other)
    {

        if(other.layer == 8)
        {
            var collisionMat = other.GetComponent<Renderer>().material;
            var collisionColor = collisionMat.GetColor("_color");

            trailMat.SetColor("_EmissionColor", collisionColor);
        }
    }

 
}
