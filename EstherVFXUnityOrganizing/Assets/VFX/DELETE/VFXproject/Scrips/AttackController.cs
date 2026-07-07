using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    
    private Animator anim;

    void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            anim.SetBool("playerAnim", true);
        }
    }

    public void animc_attack_electric_end()
    {
        anim.SetBool("playerAnim", false);
    }
    
}
