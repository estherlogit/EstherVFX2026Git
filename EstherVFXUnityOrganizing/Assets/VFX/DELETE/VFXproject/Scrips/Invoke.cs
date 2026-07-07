using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invoke : MonoBehaviour
{
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnObject", 2, 1);
    }

    void SpawnObject ()
    {
        float x = Random.Range (-10.0f, 10.0f);
        float z = Random.Range(-10.0f, 10.0f);
        Instantiate(target, new Vector3(x, 8, z), Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
