using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorKeydown : MonoBehaviour
{
    public Color color_1;
    public Color color_2;
    public Color color_3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            this.gameObject.GetComponent<Renderer>().material.SetColor("_BaseColor", color_1);
        } 
        if(Input.GetKeyDown(KeyCode.G))
        {
            this.gameObject.GetComponent<Renderer>().material.SetColor("_BaseColor", color_2);
        }
        if (Input.GetKeyDown(KeyCode.B))
            this.gameObject.GetComponent< Renderer>().material.SetColor("_BaseColor", color_3);
    }
}
