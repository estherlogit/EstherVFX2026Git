using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public GameObject destroyedVersion;
    public GameObject nonDestroyedVersion;

    
    public void Destroy()       
    {
        Instantiate(destroyedVersion, transform.position, transform.rotation);
        Vector3 scaleNonDest = nonDestroyedVersion.transform.localScale;
        destroyedVersion.transform.localScale = scaleNonDest;
        Destroy(nonDestroyedVersion.gameObject);
    }

}
    