using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderDebug : MonoBehaviour
{
    private Mesh mesh;

    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldUp = new Vector3(0f, 1f, 0f);
        Vector3 worldUp2 = new Vector3(1f, 1f, 0f);
        
        mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;
        
        Debug.DrawRay(worldUp, normals[2], Color.red); 
    }
}
