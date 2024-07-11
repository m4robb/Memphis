using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit;
using MxMGameplay;
using UnityEngine.AI;
using MxM;
using static Unity.Burst.Intrinsics.X86;
using Unity.VisualScripting;
using Valve.VR;
using static UnityEngine.Rendering.DebugUI;

public class KeeperManager : MonoBehaviour
{
    public LookAtIK LAIK;
    public AimIK AIK;

    public bool DoAim;
    public AIDestinationSetter AIDS;
    public LineOfSight LOS;
    public GameObject RealTarget;
    public GameObject DummyTarget;
    public Transform ViewTarget;
    public Transform Player;
    public Renderer Shutter;


    public NavMeshAgent Agent;

    public float DistanceThreshold = 1;
    public float PatrolRadius= 30;
    public float NoiseThreshold = 2;
    public float TargetSpeed = 0;

    public AudioSource ASDryStep;
    public AudioSource ASWetStep;
    public AudioSource KeeperTheme;
    public float FootstepsFrequency;
    public UnityEvent OnReachedTarget;
    public UnityEvent OnSeePlayer;
    public UnityEvent OnRestartLevel;
    public UnityEvent OnAnimateEvent;
    public ActionBasedContinuousMoveProvider ABCMP;

    public MxMAnimator MMA;
    public MxMTrajectoryGenerator MXMTG;

    public Transform[] Waypoints;
   
    int WaypointIndex = 0;
    bool AcquiredTarget = false;
    public bool HasReachedTarget;

    public bool HasSeenTarget;

    public float DistanceToNavTarget;
    public KeeperInput KI;
    float FootstepsTimer = 0;
    float FootStepTimingPadding = .1f;
    public float DistanceToTarget, DistanceBetween, DistanceToRealTarget;

    Vector3 StoredPosition, StartPositionKeeper, StartPositionPlayer;

    Animator KeeperAnimationController;

    float PlayerMoveSpeed;
    void OnEnable()
    {
        if(!MXMTG) MXMTG =GetComponent<MxMTrajectoryGenerator>();
        if(!LAIK) LAIK = GetComponent<LookAtIK>();
        if (!AIK) AIK = GetComponent<AimIK>();
        if (!AIDS) AIDS = GetComponent<AIDestinationSetter>();
        if (!MMA) MMA =GetComponent<MxMAnimator>();
        if (!KI) KI = GetComponent<KeeperInput>();
        if (!Agent) Agent = GetComponent<NavMeshAgent>();

        KeeperAnimationController = GetComponent<Animator>();

        DummyTarget = new GameObject();

        StartPositionKeeper = transform.localPosition;
        StartPositionPlayer = Player.position;
       
        AIDS.m_destinationTransform = DummyTarget.transform;
        LAIK.solver.target = ViewTarget;
        StoredPosition = RealTarget.transform.position;
        PlayerMoveSpeed = 1.1f;
        SetUpOnStart();  


      
        //StartCoroutine(CalcVelocity());
   }
    bool LoopIsOver;
    void SetUpOnStart()
    {
        MMA.ClearAllTags();

        ASDryStep.enabled = false; 

        ASWetStep.enabled = false;

        AIDS.enabled = false;

        //ABCMP.moveSpeed = PlayerMoveSpeed;


        WaypointIndex = 0;

        AIK.solver.IKPositionWeight = 0;
        KeeperAnimationController.SetLayerWeight(1, 1);
        KeeperAnimationController.SetLayerWeight(2, 1);
        //MMA.SetRequiredTag("Searching");
        //MXMTG.MaxSpeed = 1;
        HasReachedTarget = false;

        HasSeenTarget = false;

        LoopIsOver = false;

        ViewTarget.position = DummyTarget.transform.position;

        if (Waypoints[0])
        {
            DummyTarget.transform.position = Waypoints[0].position;
        }

        else
        {
            DummyTarget.transform.position = RealTarget.transform.position;
        }

        //gameObject.SetActive(false);

        Invoke("ActivateAnimationLayer", 1);
    }

    Vector3 RandomNavmeshLocation(float radius, Vector3 _StartPosition)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    void LevelLoopIsOver()
    {
        Agent.ResetPath();

        transform.localPosition = StartPositionKeeper;

        Player.position = StartPositionPlayer;
        HasReachedTarget = false;
        SetUpOnStart();
        OnRestartLevel.Invoke();
        //yield return new WaitForSeconds(3);
        Shutter.material.DOFade(0, 2).SetDelay(3).OnComplete(()=> {
            Shutter.gameObject.SetActive(false);  
        });

    }

    private bool SeeTargetTrigger;
    void SeeTarget()
    {
    if(SeeTargetTrigger) return;
    SeeTargetTrigger = true;
        float tValue = 1;

        DOTween.To(() => tValue, x => tValue = x,0, .5f).OnUpdate(() =>
        {
            KeeperAnimationController.SetLayerWeight(1, tValue);
        });

        float tValue2 = 0;
        float tValue3 = 0;





        DOTween.To(() => tValue2, x => tValue2 = x, 1, .5f).OnUpdate(() =>
        {

            KeeperAnimationController.SetLayerWeight(2, tValue2);
        });


 


        //KeeperAnimationController.SetTrigger("Scream");
        
        Debug.Log("RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR");

        OnSeePlayer.Invoke();

        //MMA.SetRequiredTag("Jogging");
    }


    IEnumerator Hunt()
    {
        yield return new WaitForSeconds(1);
        Vector3 destination = RandomNavmeshLocation(10, Player.position);
        //DummyTarget.transform.position = RealTarget.transform.position;

        DummyTarget.transform.DOMove(destination, 1f);
    }

    IEnumerator ResetHasSeen()
    {
        Debug.Log("ResetHasSee");
        yield return new WaitForSeconds(4);
        HasReachedTarget = false;
    }
    IEnumerator WaitOnIdle()
    {
        
        yield return new WaitForSeconds(3);

        Debug.Log("Change Dest");


        if (Waypoints.Length > 0)
        {
            WaypointIndex++;
            if (WaypointIndex >= Waypoints.Length)
            {
                //transform.parent.gameObject.SetActive(false);
            }
            else
            {
                DummyTarget.transform.DOMove(Waypoints[WaypointIndex].position, 6f);
            };
           
            //DummyTarget.transform.position = Waypoints[WaypointIndex].position;
        }
        else
        {
            Vector3 destination = RandomNavmeshLocation(PatrolRadius, transform.position);
            DummyTarget.transform.position = destination;
        }

        //HasReachedTarget = false;




    }

    public void ReachedTarget()
    {
        Debug.Log("Reached " + LOS.IsHidden);

 

        if (LOS.IsHidden)
        {
            Debug.Log("I know youre there " + LOS.IsHidden);

            if (DistanceToRealTarget < DistanceThreshold)
            {

               // DummyTarget.transform.position = RealTarget.transform.position;
               DummyTarget.transform.position = Vector3.Lerp(DummyTarget.transform.position, RealTarget.transform.position, .5f);

            }
            else
            {

                if (HasSeenTarget)
                {
                    Debug.Log("Ive seen you before " + LOS.IsHidden);
                    StartCoroutine(Hunt());
                    return;
                }
                else
                {
                    Debug.Log("will keep looking " + LOS.IsHidden);
                    StartCoroutine(WaitOnIdle());
                }
            }
        }

    }    

    public void AcquireTarget()
    {
       // AIDS.m_destinationTransform = RealTarget.transform;
    }
    public void LoseTarget()
    {
       // DummyTarget.transform.position = RealTarget.transform.position;
        //AIDS.m_destinationTransform = DummyTarget.transform;
    }

    public void DoLookAtWeight(float _Weight)
    {

    }

    public void AnimateEvent()
    {
        if(OnAnimateEvent != null) { OnAnimateEvent.Invoke(); };
    }

    void ActivateAnimationLayer()
    {
        MMA.BlendInController(.5f);
    }

    void DeactivateAnimationLayer()
    {

    }



    public void IdleStart()
    {

        if (!AIDS.enabled) return;
        if (LoopIsOver) return;

        StopAllCoroutines();
        StartCoroutine( WaitOnIdle());
        Debug.Log("Idle Start");
    }

    public void IdleEnd()
    {
        Debug.Log("Idle End");

    }

    public void Step(int _Foot)
    {


    }

    public void Step2(int _Foot)
    {
        DistanceToSound = Vector3.Distance(transform.position, RealTarget.transform.position) + .001f;
        FootStepTimingPadding = Random.Range(0, 0.2f);
        ASWetStep.pitch = Random.Range(.9f, 1.1f);
        ASDryStep.pitch = Random.Range(.9f, 1.1f);

        ASWetStep.volume = (Mathf.Clamp((DistanceToSound - 6), 0.2f, 1f));
        ASDryStep.volume =   .5f - Mathf.Clamp(DistanceToSound / 12, 0f, .4f);

        ASDryStep.Play();
        ASWetStep.Play();

   
    }


    float CurrentSpeed;

    public float SelfSpeed;

    Vector3 SelfStoredPosition;
    int counter = 0;

    float DistanceToSound;

    bool AcquiredTargetSound;
    void FixedUpdate()
    {
        counter++;

        if(counter > 5)
        {
            counter = 0;

            


            TargetSpeed = Vector3.Distance(StoredPosition, Vector3.Lerp(StoredPosition,RealTarget.transform.position, 2 * Time.deltaTime)) * 1000f;
            SelfSpeed = Vector3.Distance(SelfStoredPosition, Vector3.Lerp(SelfStoredPosition,  transform.position, 2 * Time.deltaTime)) * 1000f;

            StoredPosition = RealTarget.transform.position;
            SelfStoredPosition = transform.position;

           
        }

        

    }


    float AcquireTimer = 100;
    void ScanLineOfSight()
    {

        if(!LOS.IsHidden)
        {
            Debug.Log("In sight");

            AcquireTimer += Time.deltaTime;

            HasSeenTarget = true;

            DummyTarget.transform.position = RealTarget.transform.position; // Vector3.Lerp(DummyTarget.transform.position, RealTarget.transform.position, .1f);


            ABCMP.moveSpeed = 0;


            if (LAIK.solver.IKPositionWeight < 1)
            {
                LAIK.solver.IKPositionWeight += 1f * Time.deltaTime;
            }


        }


        if (!LOS.IsHidden && !AcquiredTarget)
        {
            HasReachedTarget = false;
            AcquiredTarget = true;
            Debug.Log("Locked On");
            StopAllCoroutines();
            SeeTarget();
        }

        if (LOS.IsHidden)
        {

           // ViewTarget.parent.position = Vector3.Lerp(ViewTarget.parent.position, transform.forward * 2, .1f);
            //if (LAIK.solver.IKPositionWeight > 0)
            //{
            //    LAIK.solver.IKPositionWeight -= .4f * Time.deltaTime;
            //}
        }

        if (LOS.IsHidden && AcquiredTarget)
        {
            AcquiredTarget = false;
        }
    }

    bool Active;



    void Update()
    {
        if (!AIDS.enabled) return;

        if (LoopIsOver) return;
 
        ASDryStep.enabled = true;
        ASWetStep.enabled = true;

        DistanceToNavTarget = Agent.remainingDistance;
        DistanceToTarget = Vector3.Distance(transform.position, AIDS.m_destinationTransform.position);
        DistanceToRealTarget = Vector3.Distance(transform.position, RealTarget.transform.position);
        DistanceBetween = Vector3.Distance(DummyTarget.transform.position, RealTarget.transform.position);

        ViewTarget.position = Vector3.Lerp(ViewTarget.position, DummyTarget.transform.position, .1f);

        if (LOS.IsHidden)
        {

        }
        

        //ScanLineOfSight();

        //if (!LOS.IsHidden && DistanceToRealTarget < 2 && !LoopIsOver)
        //{

        //    OnRestartLevel.Invoke();

        //    LoopIsOver = true;

        //    Debug.Log("Gotcha");

        //    //KI.Search01();

           

        //    return;
        //}






        FootstepsTimer += Time.deltaTime;

        if (FootstepsTimer > FootstepsFrequency / SelfSpeed + FootStepTimingPadding)
        {
            Step2(0);
            FootstepsTimer = 0;
        }


    }
}
