using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingBallLocation : MonoBehaviour
{
    public ParticleSystem smoke1;
    public ParticleSystem smoke2;
    public Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
       rb = gameObject.GetComponent<Rigidbody>();
      
    }

    void OnCollisionEnter(Collision other)
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
