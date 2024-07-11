using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsButton : MonoBehaviour
{
    public Rigidbody buttonTopRigid;
    public Transform buttonTop;
    public Transform buttonLowerLimit;
    public Transform buttonUpperLimit;
    public float threshHold;
    public float force = 10;
    private float upperLowerDiff;
    public bool isPressed;
    private bool prevPressedState;
    public AudioSource pressedSound;
    public AudioSource releasedSound;
    public Collider[] CollidersToIgnore;
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    // Start is called before the first frame update

    bool IsFinger;

    float KineticEnergy(Rigidbody rb)
    {
        // mass in kg, velocity in meters per second, result is joules
        return 0.5f * rb.mass * Mathf.Pow(rb.linearVelocity.magnitude, 2);
    }
    void Start()
    {
        Collider localCollider = buttonTopRigid.GetComponent<Collider>();
        if (localCollider != null)
        {
            Physics.IgnoreCollision(localCollider, buttonTop.GetComponentInChildren<Collider>());

            foreach (Collider singleCollider in CollidersToIgnore)
            {
                Physics.IgnoreCollision(localCollider, singleCollider);
            }
        }

        ColliderBridge cb = buttonTopRigid.gameObject.AddComponent<ColliderBridge>();
        cb.Initialize(this);

        if (transform.eulerAngles != Vector3.zero){
            Vector3 savedAngle = transform.eulerAngles;
            transform.eulerAngles = Vector3.zero;
            upperLowerDiff = buttonUpperLimit.localPosition.y - buttonLowerLimit.localPosition.y;
            transform.eulerAngles = savedAngle;
        }
        else
            upperLowerDiff = buttonUpperLimit.localPosition.y - buttonLowerLimit.localPosition.y;
    }

    // Update is called once per frame

    public void OnCollisionEnter(Collision collision)
    {
       

        if (collision.gameObject.layer == 9)
        {
            IsFinger = true;
        }
        
    }
    public void OnCollisionExit(Collision collision)
    {
        IsFinger = false;
    }

    void Update()
    {

       

        buttonTop.transform.localPosition = new Vector3(0, buttonTop.transform.localPosition.y, 0);
        buttonTop.transform.localEulerAngles = new Vector3(0, 0, 0);
        //return;
        if (buttonTop.localPosition.y >= buttonUpperLimit.localPosition.y)
        {
            buttonTop.transform.localPosition = new Vector3(buttonUpperLimit.localPosition.x, buttonUpperLimit.localPosition.y, buttonUpperLimit.localPosition.z);
        }

        else
        {

            buttonTopRigid.AddForce(buttonTop.transform.up * force * Time.deltaTime);
        }
           

        //

        if (buttonTop.localPosition.y <= buttonLowerLimit.localPosition.y)
        {
            buttonTop.transform.localPosition = new Vector3(buttonLowerLimit.localPosition.x, buttonLowerLimit.localPosition.y, buttonLowerLimit.localPosition.z);
        }



        if (Vector3.Distance(buttonTop.localPosition, buttonLowerLimit.localPosition) <= 0)
            isPressed = true;
        else
            isPressed = false;

        if (isPressed && prevPressedState != isPressed)
            Pressed();
        if(!isPressed && prevPressedState != isPressed)
            Released();
    }



    void Pressed(){

       
        if (!IsFinger)
        {
            return;
        }
        IsFinger = false;
        Debug.Log("Pressed");
        onPressed.Invoke();
        prevPressedState = isPressed;
       
    }

    void Released(){
        prevPressedState = isPressed;
        onReleased.Invoke();
    }
}