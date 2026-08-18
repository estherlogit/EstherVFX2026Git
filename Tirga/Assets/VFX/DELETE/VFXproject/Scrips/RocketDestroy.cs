using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketDestroy : MonoBehaviour
{
    public float destroyDelay = 0.2f;
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(this.gameObject);
        Destroy(this.gameObject, destroyDelay);
        
    }
}
