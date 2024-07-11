using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelBoxGearsController : MonoBehaviour

{
    // Start is called before the first frame update

    public Animator WatchAnimator;
    void Start()
    {
        WatchAnimator.Play("BalanceAxle");
        WatchAnimator.Play("Escapement");
        WatchAnimator.Play("BalanceWheel");
        WatchAnimator.Play("EscapeWheel");

        //Escapement BalanceWheel EscapeWheel
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
