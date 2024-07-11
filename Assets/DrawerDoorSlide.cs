using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerDoorSlide : MonoBehaviour
{
    public AudioSource AS;

    Rigidbody RB;

    bool CanMakeNoise, Trigger;

    public AudioClip[] AudioClipArray;

    public ConfigurableJoint CFJ;

    public float Multix;

    int ClipIndex = 0;

    float ClipDuration = 0;

    public bool IsDoor;

    float OpenCloseMagnitude;

    void Start()
    {
        
    }

    IEnumerator PlayOneShot(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);

        AS.PlayOneShot(AudioClipArray[ClipIndex]);

    }

    float Timer = 0;

    Vector3 StoredPosition;

    private void FixedUpdate()
    {

        //if (CFJ == null)
        //{
        //    CFJ = GetComponent<ConfigurableJoint>();
        //    return;
        //}

        //if(transform.localPosition.z > .29f)
        //{
        //    CFJ.zDrive = new JointDrive()
        //    {
        //        positionSpring = 0f,
        //        maximumForce = 0f
        //    };

        //} else
        //{
        //    CFJ.zDrive = new JointDrive()
        //    {
        //        positionSpring =160f,
        //        maximumForce =60f
        //    };
        //}

        //if (!IsDoor)
        //{


        //    OpenCloseMagnitude = Mathf.Abs(RB.velocity.magnitude) * Multix;


        //}

        //if (IsDoor) OpenCloseMagnitude = Mathf.Abs(RB.angularVelocity.magnitude) * Multix;
        //Debug.Log(OpenCloseMagnitude);
        //AS.volume = OpenCloseMagnitude;
    }
    void Update()

    {

       

        if (Trigger)
        {
            Timer += Time.deltaTime;
            if(Timer > ClipDuration)
            {
                Timer = 0;
                Trigger = false;
            }
        }

        if (AudioGridTick.AudioGridInstance != null && !CanMakeNoise)
        {
            StoredPosition = transform.position;
            CanMakeNoise = true;
        }

        //if (RB == null)
        //{
        //    RB = GetComponent<Rigidbody>();
        //} else
        //{
            //if (!IsDoor)
            //{

            //    Vector3 VelocityVector = transform.position - StoredPosition;
            //    OpenCloseMagnitude = Mathf.Abs(VelocityVector.magnitude) * Multix;
            //    StoredPosition = transform.position;
            //}

            

            //if (OpenCloseMagnitude > .0001f)
            //{
               
            //     AS.Play();
            //}
            //else
            //{
            //   AS.Pause();
            //}




            //if (Mathf.Abs(RB.velocity.magnitude) > 0.001f || Mathf.Abs(RB.angularVelocity.magnitude) > 0.0001f)
            //{

               

            //    if (!Trigger)
            //    {
            //        Debug.Log("Play");
            //        AS.Play();
            //        Trigger = true;
            //    }
               
            //}
            //else
            //{
            //    Debug.Log("Pause");
            //    AS.Pause();
            //    Trigger = false;
            //}
        //}
        
    }
}
