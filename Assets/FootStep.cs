using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStep : MonoBehaviour
{
    // Start is called before the first frame update

    public AudioSource FootStepSound;

    void Start()
    {
        
    }
    bool step = true;

    float StepLength = .55f;

    IEnumerator WaitForFootSteps(float stepsLength) {
        step = false;
        yield return new WaitForSeconds(stepsLength);
        step = true;
    } /////////////////////////////////// CONCRETE //////////////////////////////////////// void WalkOnConcrete() {



private void OnCollisionEnter(Collision collision)
    {

        Debug.Log(transform.name);
        if(collision.transform.tag == "Ground" && step)
        {
            FootStepSound.pitch = 1.5f + Random.Range(0, 0.07f);
            FootStepSound.PlayOneShot(FootStepSound.clip);
            StartCoroutine(WaitForFootSteps(StepLength));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
