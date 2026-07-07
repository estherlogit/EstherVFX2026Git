using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeProjectile : MonoBehaviour
{
    public float duration = 0.15f;
    public float magnitude = 0.45f;

    void OnCollisionEnter(Collision other)
    {

        // camera shake
        StartCoroutine(Shake(duration, magnitude));


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

    }
}
