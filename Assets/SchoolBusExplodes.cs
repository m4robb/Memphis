using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchoolBusExplodes : MonoBehaviour
{
    public Animator Anim;
    public float Speed = .1f;


    IEnumerator ExecuteExplosion(float _Delay)
    {
        yield return new WaitForSeconds(_Delay * 60 / (float)AudioGridTick.AudioGridInstance.bpm);

        Debug.Log("EXPLODE");

        Anim.SetFloat("Speed", Speed);
    }

 
    public void ExplodeBus (float _Delay)
    {
        StartCoroutine(ExecuteExplosion(_Delay));
    }
}
