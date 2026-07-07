using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateVfx : MonoBehaviour
{
    public GameObject explosionFx;
    public float delayDestroy = 0.5f;

    void OnCollisionEnter(Collision other)
    {
        Instantiate(explosionFx, transform.position, transform.rotation);
    }

    void OnCollisionStay(Collision other)
    {
        Destroy(this.gameObject, delayDestroy);
    }
}
