using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseColor : MonoBehaviour
{
    public Color color_i;
    public Color color_o;
    public Color color_p;

    public float moveSpeed = 10f;
    public float rotationSpeed = 10f;

    private ParticleLauncherKey emitter;
    public ParticleSplatterColor emitter_splatter;
    public ParticleDecalPool emitter_splash;

    // Start is called before the first frame update
    void Start()
    {
        emitter = GetComponentInChildren<ParticleLauncherKey>();
    }

    // Update is called once per frame
    void Update()
    {
        //Choose color in object, emitter and emitter splatter part

        if (Input.GetKeyDown(KeyCode.I))
        {

            this.gameObject.GetComponent<Renderer>().material.SetColor("_color", color_i);
            emitter.ChangeParticleColor(color_i);
            emitter_splatter.ChangeParticleColor(color_i);
            //emitter_splash.ChangeParticleColor(color_i);

        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            this.gameObject.GetComponent<Renderer>().material.SetColor("_color", color_o);
            emitter.ChangeParticleColor(color_o);
            emitter_splatter.ChangeParticleColor(color_o);
            //emitter_splash.ChangeParticleColor(color_o);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            this.gameObject.GetComponent<Renderer>().material.SetColor("_color", color_p);
            emitter.ChangeParticleColor(color_p);
            emitter_splatter.ChangeParticleColor(color_p);
            //emitter_splash.ChangeParticleColor(color_p);
        }

        //Move and rotate object part

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
