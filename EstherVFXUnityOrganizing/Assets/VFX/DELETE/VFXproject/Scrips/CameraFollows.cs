using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollows : MonoBehaviour
{
    public Transform target;
    public float smothSpeed = 0.5f;

    public Vector3 offset;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 desirePosition = target.position + offset;
        Vector3 smothedPosition = Vector3.Lerp(transform.position, desirePosition, smothSpeed);
        transform.position = smothedPosition;
        transform.LookAt(target);
    }
}
