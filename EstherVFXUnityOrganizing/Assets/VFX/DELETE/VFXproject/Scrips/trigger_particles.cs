using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trigger_particles : MonoBehaviour
{
    public Color hitColor;
    public Color defaultColor;
    public float ForceIncrement = 0.2f;
    public float acumulatedTime = 0.0f;
    private GameObject hitObject;
    private bool collision = false;
    private Material mat;
    private GameObject goCollider;

    private void LateUpdate()
    {
        if(!collision && goCollider)
        {
            mat.SetColor("_color_emission1", defaultColor);
            var pos = goCollider.transform.position;
            Vector3 newPosition = new Vector3(pos.x + ForceIncrement, pos.y, pos.z);
            
            acumulatedTime += Time.deltaTime;
            newPosition.x = Mathf.Lerp(pos.x, newPosition.x, acumulatedTime / 3000);

            goCollider.transform.position = newPosition;
        }
    }

    private void OnParticleCollision(GameObject gameObjectCollider)
    {
        if(gameObjectCollider.tag == "reciver")
        {
            mat = gameObjectCollider.GetComponent<Renderer>().material;
            mat.SetColor("_color_emission1", hitColor);
            hitObject = gameObjectCollider;
            collision = true;
            goCollider = gameObjectCollider;
        }
        collision = false;
    }

    
}
