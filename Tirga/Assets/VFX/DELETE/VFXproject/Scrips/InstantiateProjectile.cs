using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateProjectile : MonoBehaviour
{
    public GameObject projectilePrefab;
    private GameObject projectileInstance;
    public Transform projectileWayout;
    public float speedForward = 100f;
    public float speedUp = 100f;
    public float xAngle, yAngle, zAngle;
    public float rotationAmount = 1f;

    void Update()
    { 
        if (Input.GetKeyDown(KeyCode.Space))
            
        {
        projectileInstance = Instantiate(projectilePrefab, projectileWayout.position, projectileWayout.rotation);     
        projectileInstance.GetComponent<Rigidbody>().AddForce(projectileWayout.forward * speedForward);
        projectileInstance.GetComponent<Rigidbody>().AddForce(projectileWayout.up * speedUp);
        projectileInstance.GetComponent<Rigidbody>().AddRelativeTorque(Vector3.left * rotationAmount);

        }
    }

}
