using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class UsingInstantiate : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject bullet;

    private GameObject bulletInstance;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bulletInstance = Instantiate(bullet);
            bulletInstance.GetComponent<Rigidbody>().useGravity = true;
        }


    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(other.gameObject);
    }
}