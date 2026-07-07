using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public bool StartEffect;
    public Animator ScanAnimator;

    
    private void Start()
    {
        
    }

    private void Update()
    {
        if (StartEffect)
        {
            startScan();
        }
    }
    
    public void startScan()
    {
        ScanAnimator.Play("anim_scan");   
    }
}
