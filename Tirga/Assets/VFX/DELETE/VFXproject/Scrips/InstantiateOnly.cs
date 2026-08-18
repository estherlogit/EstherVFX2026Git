using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateOnly : MonoBehaviour
{
    public GameObject rocketPrefab;
    private GameObject rocketInstance;
    public Transform barrelEnd;
    public float Speed = 1000f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("instance");
            //rocketInstance = Instantiate(rocketPrefab);
            //rocketInstance.GetComponent<Rigidbody>().useGravity = true;
            rocketInstance = Instantiate(rocketPrefab, barrelEnd.position, barrelEnd.rotation);
            rocketInstance.GetComponent<Rigidbody>().AddForce(barrelEnd.forward * Speed);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
    }

}
