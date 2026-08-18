using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleMoveRotateObject : MonoBehaviour
{
    public float hitIncrement;
    public float rotationAmount;
    public float xAngle, yAngle, zAngle;
    void Start()
    {
        this.GetComponent<ParticleSystem>();
    }
    private void OnParticleCollision(GameObject other)
    {
        if (other.layer == 8)
        {
            Vector3 newposition = new Vector3(other.transform.position.x + hitIncrement, other.transform.position.y, other.transform.position.z);
            other.transform.position = newposition;


        }


          if (other.layer == 8)

        {
            other.transform.Rotate(xAngle + rotationAmount, yAngle, zAngle, Space.Self);

            Debug.Log("rotation");

        }




    }
}
