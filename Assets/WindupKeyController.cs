using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindupKeyController : MonoBehaviour
{
   
    public Animator KeyAnimator;
    [SerializeField] float KeySpeed = 0;

    float Storedvalue = 0;

    public void SetSpeed(float _Speed)
    {
        KeyAnimator.SetFloat("Speed",_Speed);
    }
    void Update()
    {

    }
}
