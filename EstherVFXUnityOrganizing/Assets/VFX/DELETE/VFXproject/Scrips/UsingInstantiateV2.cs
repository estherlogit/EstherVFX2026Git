using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsingInstantiateV2 : MonoBehaviour
{
    public Rigidbody bulletPrefab;
    public Transform bulletEmptyRef;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody bulletInstance;
            bulletInstance = Instantiate(bulletPrefab, bulletEmptyRef.position, bulletEmptyRef.rotation) as Rigidbody;
            bulletInstance.AddForce(bulletEmptyRef.forward * 5000);
        } 
    }
}
