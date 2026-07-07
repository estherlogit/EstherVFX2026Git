using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOther : MonoBehaviour
{
    public GameObject other;
    public float destroyDelay = 2f;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Destroy(other, destroyDelay);
        }
    }
}
