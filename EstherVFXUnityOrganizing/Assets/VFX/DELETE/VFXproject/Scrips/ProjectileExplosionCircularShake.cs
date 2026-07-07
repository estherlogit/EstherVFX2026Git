using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileExplosionCircularShake : MonoBehaviour
{

    public float delayDestroy = 3f;
    public GameObject explosionFx;
    public float radius = 3f;
    public float force = 700f;

    public float Strenght = 0.15f;
    public float Duration = 0.45f;

    private Vector3 _initialCameraPosition;
    private float _remainingShakeTime;

    public void Shake()
    {
        _remainingShakeTime = Duration;
        enabled = true;
    }

    private void Awake()
    {
        _initialCameraPosition = Camera.main.transform.localPosition;
        enabled = false;
    }

    void OnCollisionEnter(Collision other)
    {

        Instantiate(explosionFx, transform.position, transform.rotation);

        Collider[] collidersToDestroy = Physics.OverlapSphere(transform.position, radius);

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
        if (_remainingShakeTime <= 0)
        {
        Camera.main.transform.localPosition = _initialCameraPosition;
            enabled = false;
        }
        Camera.main.transform.Translate(Random.insideUnitCircle * Strenght);

        _remainingShakeTime -= Time.deltaTime;

        Debug.Log("CircularShake");


        //Destroy projectile
        Destroy(this.gameObject, delayDestroy);

    }
}


