using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRotate : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 10f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.DownArrow))
            transform.Translate(-Vector3.forward * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.RightArrow))
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.R))
            transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.T))
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
