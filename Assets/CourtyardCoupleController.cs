using UnityEngine;

// Ugly script, just used for demo/testing purposes!
public class CourtyardCoupleController : MonoBehaviour
{

    public Transform Target;
    public bool StartCatch = true;
    public float WalkBackDistanceMin;
    Vector3 StartPos;

    Vector3 TargetPos;

    bool BackTrack = true;

    Quaternion correctRot;



    Animator CoupleAnimator;
    private void Start()
    {
        CoupleAnimator = GetComponent<Animator>();

        StartPos = transform.position;

        TargetPos = Target.position;

        CorrectPositionForeWards();

        if (StartCatch)
        {
            CoupleAnimator.Play("Catch");
        }
        else{
            CoupleAnimator.Play("Throw");
        }
    }

    void CorrectPositionBackWards()
    {
            correctRot = Quaternion.Inverse(Quaternion.LookRotation(transform.position - StartPos ));
            correctRot.x = 0;
            correctRot.z = 0;
            transform.rotation = correctRot;
    }

    void CorrectPositionForeWards()
    {
        correctRot = Quaternion.LookRotation(Target.position - transform.position);
        correctRot.z = 0;
        transform.rotation = correctRot;
    }


    float CorrectCounter = 0;
    void Update()
    {
        CorrectCounter += Time.deltaTime;

        if (Vector3.Distance(transform.position, Target.position) < WalkBackDistanceMin)
        {
            CorrectPositionBackWards();
            CoupleAnimator.CrossFadeInFixedTime("WalkBackwards", .25f);
            BackTrack = false;
        }

        if (Vector3.Distance(transform.position, Target.position) > WalkBackDistanceMin + 1 && !BackTrack)
        {
            BackTrack = true;
            CorrectPositionForeWards();
           
            if (StartCatch)
            {
                CoupleAnimator.CrossFadeInFixedTime("Catch", .25f);
            }
            else
            {
                CoupleAnimator.CrossFadeInFixedTime("Throw", .25f);
            }
        }



        //CoupleAnimator.MatchTarget(Vector3.zero,correctRot,AvatarTarget.Root, new MatchTargetWeightMask(new Vector3(0, 0, 0), 1), CoupleAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime, .2f);



    }
}