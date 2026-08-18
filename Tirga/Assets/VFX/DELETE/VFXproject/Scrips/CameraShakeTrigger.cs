using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeTrigger : MonoBehaviour
{

    // public GameObject cameraShaked;


    void OnColisionEnter (Collision col)
    {
        GetComponent<Camera>().GetComponent<CameraShake>().ShakeNow(0.15f, 0.4f);

        Debug.Log("CollisionShake");
    }

    /*void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(Shake(0.15f, 0.4f));
            Debug.Log("Shake");
        }
    }*/

   /* public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = cameraShaked.transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)

        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraShaked.transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
            Debug.Log($"cameraShake {elapsed}");
        }

        cameraShaked.transform.localPosition = originalPos;

    }*/



}
