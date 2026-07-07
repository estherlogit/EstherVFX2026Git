using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDestruction : MonoBehaviour
{
    public Rigidbody bullet; 
    public float destructionDelay = 2f;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Destroy(bulletPrefab, destructionDelay);
    }
}
