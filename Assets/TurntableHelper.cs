using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurntableHelper : MonoBehaviour
{
    public LayerMask LayerMask;

    public Transform CurrentDisc;

    public TurntableArmController TAC;

    public void DoSwitch()
    {

        if(CurrentDisc)
            TAC.CurrentDisc = CurrentDisc;

        if (CurrentDisc.GetComponent<Animator>())
            TAC.Turntable = CurrentDisc.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if ((LayerMask.value & (1 << other.transform.gameObject.layer)) != 0)
        {

            CurrentDisc = other.transform;
             
        };
    }
}

