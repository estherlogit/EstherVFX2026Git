using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAnimation : MonoBehaviour
{
    private Animator anim;
    public ParticleSystem collissionParticles;



    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }
  

    void OnParticleCollision(GameObject collisionParticles)
    {
    
      
    }
}
