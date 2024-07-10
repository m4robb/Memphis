using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Beetle : MonoBehaviour
{
    private Animator beetle;
    public CharacterController characterController;
    public LayerMask CollisionMask;
    public LayerMask InteractorMask;
    public float gravity = 2.0f;
    private Vector3 moveDirection = Vector3.zero;
    private bool Speed1 = true;
    private bool Speed2 = false;
    Vector3 StartRotation;
    // Start is called before the first frame update
    void Start()
    {
        beetle = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        beetle.SetBool("idle", false);
        beetle.SetBool("walk", true);

        StartRotation = transform.localEulerAngles;
    }

    bool Trigger, CanGrab;

    private void OnCollisionEnter(Collision collision)

        
    {

        if ((InteractorMask.value & (1 << collision.transform.gameObject.layer)) != 0)
        {
            CanGrab = true;
            Trigger = false;
        }

        if (!CanGrab) return;

        if (Trigger) return;

        if ((CollisionMask.value & (1 << collision.transform.gameObject.layer)) != 0)
        {
            Debug.Log("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            Trigger = true;
            transform.DOLocalRotate(StartRotation, 1);

        }
    }
    void FixedUpdate()
    {
        // characterController.SimpleMove(transform.forward * 2);
        // characterController.SimpleMove(new Vector3(1, 0, 1));
        //characterController.Move(moveDirection * Time.deltaTime);


         //characterController.Move(moveDirection);

         //moveDirection.z -= gravity * Time.deltaTime;

        //if (beetle.GetCurrentAnimatorStateInfo(0).IsName("idle"))
        //{
        //    beetle.SetBool("attack3", false);
        //    beetle.SetBool("attack2", false);
        //    beetle.SetBool("attack1", false);
        //    beetle.SetBool("walk", false);
        //    beetle.SetBool("run", false);
        //    beetle.SetBool("preen", false);
        //    beetle.SetBool("hit", false);
        //}
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Speed1 = !Speed1;
            Speed2 = false;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Speed2 = !Speed2;
            Speed1 = false;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            beetle.SetBool("idle", false);
            beetle.SetBool("backward", true);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            beetle.SetBool("idle", true);
            beetle.SetBool("backward", false);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            beetle.SetBool("turnleft", true);
            beetle.SetBool("idle", false);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            beetle.SetBool("turnleft", false);
            beetle.SetBool("idle", true);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            beetle.SetBool("turnright", true);
            beetle.SetBool("idle", false);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            beetle.SetBool("turnright", false);
            beetle.SetBool("idle", true);
        }
        if ((Input.GetKeyDown(KeyCode.W))&&(Speed1==true))
        {
            beetle.SetBool("idle", false);
            beetle.SetBool("walk", true);
        }
        if ((Input.GetKeyUp(KeyCode.W)) && (Speed1 == true))
        {
            beetle.SetBool("idle", true);
            beetle.SetBool("walk", false);
        }
        if ((Input.GetKeyDown(KeyCode.A)) && (Speed1 == true))
        {
            beetle.SetBool("walkleft", true);
            beetle.SetBool("walk", false);
        }
        if ((Input.GetKeyUp(KeyCode.A)) && (Speed1 == true))
        {
            beetle.SetBool("walkleft", false);
            beetle.SetBool("walk", true);
        }
        if ((Input.GetKeyDown(KeyCode.D)) && (Speed1 == true))
        {
            beetle.SetBool("walkright", true);
            beetle.SetBool("walk", false);
        }
        if ((Input.GetKeyUp(KeyCode.D)) && (Speed1 == true))
        {
            beetle.SetBool("walkright", false);
            beetle.SetBool("walk", true);
        }
        if ((Input.GetKeyDown(KeyCode.W)) && (Speed2 == true))
        {
            beetle.SetBool("idle", false);
            beetle.SetBool("run", true);
        }
        if ((Input.GetKeyUp(KeyCode.W)) && (Speed2 == true))
        {
            beetle.SetBool("idle", true);
            beetle.SetBool("run", false);
        }
        if ((Input.GetKeyDown(KeyCode.A)) && (Speed2 == true))
        {
            beetle.SetBool("runleft", true);
            beetle.SetBool("run", false);
        }
        if ((Input.GetKeyUp(KeyCode.A)) && (Speed2 == true))
        {
            beetle.SetBool("runleft", false);
            beetle.SetBool("run", true);
        }
        if ((Input.GetKeyDown(KeyCode.D)) && (Speed2 == true))
        {
            beetle.SetBool("runright", true);
            beetle.SetBool("run", false);
        }
        if ((Input.GetKeyUp(KeyCode.D)) && (Speed2 == true))
        {
            beetle.SetBool("runright", false);
            beetle.SetBool("run", true);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            beetle.SetBool("attack1", true);
            beetle.SetBool("idle", false);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            beetle.SetBool("attack2", true);
            beetle.SetBool("idle", false);
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            beetle.SetBool("attack3", true);
            beetle.SetBool("idle", false);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            beetle.SetBool("idle", false);
            beetle.SetBool("preen", true);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            beetle.SetBool("hit", true);
            beetle.SetBool("idle", false);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            beetle.SetBool("die", true);
            beetle.SetBool("idle", false);
        }
    }
}
