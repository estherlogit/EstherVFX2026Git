using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsingInstantiateV3 : MonoBehaviour
{
    public GameObject rocketPrefab;
    private GameObject rocketInstance;
    public Transform barrelEnd;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("instance");
            rocketInstance = Instantiate(rocketPrefab);
            rocketInstance.GetComponent<Rigidbody>().useGravity = true;
            //rocketInstance = Instantiate(rocketPrefab, barrelEnd.position, barrelEnd.rotation);
            //rocketInstance.GetComponent<Rigidbody>().AddForce(barrelEnd.forward * 500);
        }

    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log ("collision");
        Destroy(other.gameObject);
    }


}
