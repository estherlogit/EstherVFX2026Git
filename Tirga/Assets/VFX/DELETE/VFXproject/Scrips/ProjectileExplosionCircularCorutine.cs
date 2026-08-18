using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileExplosionCircularCorutine : MonoBehaviour
{

    public float delayDestroy = 3f;
    public GameObject explosionFx;
    public float radius = 3f;
    public float force = 700f;
 
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.45f;

    void OnCollisionEnter(Collision other)
    {
        
        Instantiate(explosionFx, transform.position, transform.rotation);

        Collider[] collidersToDestroy =  Physics.OverlapSphere(transform.position, radius);

        foreach (Collider nearbyObject in collidersToDestroy)
        {

            Destructible dest = nearbyObject.GetComponent<Destructible>();
            if (dest != null)
            {
                dest.Destroy();
            }
        }
        Debug.Log("colisionPieces");
        Collider[] collidersToMove = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider nearbyObject in collidersToMove)

            {
                Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(force, transform.position, radius);
                }
            }
    // camera shake
        StartCoroutine(Shake(shakeDuration, shakeMagnitude));


    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)

        {

            Camera.main.transform.Translate(Random.insideUnitCircle * magnitude);
          

            elapsed += Time.deltaTime;

            yield return null;
            Debug.Log("cameraShake");
        }

        Camera.main.transform.localPosition = originalPos;

        //Destroy projectile
        Destroy(this.gameObject, delayDestroy);

    }
}
