using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class BouncingBall : MonoBehaviour
{
    public ParticleSystem smoke;
    public ParticleSystem smoke2;
    public Rigidbody rb;
    public float yforce = -20000;
    public float delayDestroy = 0.1f;
    public GameObject sphereSmash;
    public GameObject collisionPoint;
 
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        rb.AddRelativeForce(0, yforce, 0);
    }
    void OnCollisionEnter(Collision other)
    {
        smoke.Emit(30);
        smoke2.Emit(30);
        {
    //Instantiate(sphereSmash, collisionPoint.transform.position, collisionPoint.transform.rotation);
    //Instantiate(smoke, collisionPoint.transform.position, collisionPoint.transform.rotation);


        }
    }

    void OnCollisionStay(Collision other)
    {
        Destroy(this.gameObject, delayDestroy);
        yforce = 0;
        Debug.Log(yforce);
    }
  

    }



