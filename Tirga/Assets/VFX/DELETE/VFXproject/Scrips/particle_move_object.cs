using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle_move_object : MonoBehaviour
{
    public float hitIncrement;
    void Start()
    {
        this.GetComponent<ParticleSystem>();
    }
    private void OnParticleCollision(GameObject other)
    {
        if (other.layer==8)
        {
            Vector3 newposition = new Vector3(other.transform.position.x + hitIncrement, other.transform.position.y, other.transform.position.z);
            other.transform.position=newposition;
        }

    }    
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
