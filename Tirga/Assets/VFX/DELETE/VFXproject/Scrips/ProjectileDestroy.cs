using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDestroy : MonoBehaviour
{
    public float delay = 0.2f;


    void OnCollisionEnter(Collision other)
    {
        Debug.Log(this.gameObject);
        Destroy(this.gameObject,delay);
    }
}
