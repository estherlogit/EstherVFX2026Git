using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClick : MonoBehaviour
{
    public float force = 500f;
    void OnMouseDown()
      
    
    {
       GetComponent<Rigidbody> () .AddForce(-transform.forward * force);
       GetComponent<Rigidbody> () .useGravity = true;
    }

   
}
