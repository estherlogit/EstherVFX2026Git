using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HideOnPlay : MonoBehaviour
{
    // TODO: refactor c:
    void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
}
