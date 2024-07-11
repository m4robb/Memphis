//using UnityEngine;
//using System.Collections;
//using RootMotion.Dynamics;
//using RealisticEyeMovements;
//using RootMotion.FinalIK;
//using DG.Tweening;


//public class KeeperBrain : MonoBehaviour
//{
//    public BehaviourPuppet puppet;
//    public PuppetMaster PM;
//    public UnityEngine.AI.NavMeshAgent KeeperAgent;
//    public Transform MovingTarget;
//    public Transform PlayerTarget;
//    public PlayerController PC;
//    public float DistanceToPlayer;
//    public Animator animator;
//    public Transform[] POIArray;
//    public float BaseTargetTime = 10;
//    public LookTargetController LTC;
//    public bool IsChasing;
//    int CurrentMovingTargetIndex = 0;

//    public AudioSource LeftFoot;
//    public AudioSource RightFoot;
//    public AudioSource HuntingDrone;
//    public Transform KeeperHead;
//    public FullBodyBipedIK FBIK;
//    public LookAtIK LAIK;


//    private void Start()
//    {
//        KeeperAgent.updatePosition = false;
//        for (int i = 0; i > POIArray.Length; i++)
//        {
//            LTC.pointsOfInterest[i] = POIArray[i];
//        }

//        LTC.lookAtPlayerRatio = 0;
//    }

//    bool RightStep = true;
//    bool LeftStep = true;
//    IEnumerator WaitForFootRight(float stepsLength)
//    {
//        RightStep= false;
//        yield return new WaitForSeconds(stepsLength);
//        RightStep = true;
//    }
//    IEnumerator WaitForFootLeft(float stepsLength)
//    {
//       LeftStep = false;
//        yield return new WaitForSeconds(stepsLength);
//        LeftStep = true;
//    }

//    //void OnAnimatorMove()
//    //{
//    //     //agent.speed = animator.deltaPosition / Time.deltaTime;

//    //     agent.speed = (animator.deltaPosition / Time.deltaTime).magnitude;
//    //}
//    private RaycastHit _mHitInfo;   // allocating memory for the raycasthit

//    float ChaseTimer = 0;
//    bool HasFoundSomeThing;

//    public void DisableKeeperPuppet()
//    {
//        PM.state = PuppetMaster.State.Dead;
//    }
//    void CheckForPlayer()
//    {

//        Vector3 targetDir = PlayerTarget.position - KeeperHead.position;

//        float angleToPlayer = (Vector3.Angle(targetDir, KeeperHead.forward));



//        DistanceToPlayer = Vector3.Distance(transform.position, PlayerTarget.position);

//        if (DistanceToPlayer < 20 && PC.MovementValue > PC.NoiseThreshold)  // Player Makes a sound. 
//        {
//            IsChasing = true;
//            return;
//        }

//        if (DistanceToPlayer < 20 && DistanceToPlayer > 2 && PC.MovementValue < PC.MovementDetectionThreshold) // Player Freezes!
//        {

//            ChaseTimer += Time.deltaTime;

//            if(ChaseTimer > 4)
//            {
//                if (IsChasing)
//                {
//                    AgentTimer = 0;
//                    Vector3 Kpos = KeeperAgent.transform.position + KeeperAgent.transform.forward * 2;
//                    Kpos.y = 1.5f;
//                    MovingTarget.position = Kpos;
//                }
//                ChaseTimer = 0;
//                IsChasing = false;
//                LTC.lookAtPlayerRatio = 0;
//            }

//            return;
//        }



//        if (angleToPlayer >= -120 && angleToPlayer <= 120) // 180° FOV
//        {
//            ChaseTimer = 0;
//            if (DistanceToPlayer < 2)
//            {
//                IsChasing = true;
//            }

//            if (Physics.Raycast(KeeperHead.position, targetDir, out _mHitInfo, 100)){
//                if (_mHitInfo.transform.CompareTag("Player"))
//                {
//                    IsChasing = true;
//                } else
//                {
//                    IsChasing = false;
//                }
//            } 
//        } else
//        {
//            IsChasing = false;
//        }


//    }

//    public void MakeStep(int LR)
//    {
//        if (LR == 1 && LeftStep)
//        {
//            LeftFoot.pitch = 1.5f + Random.Range(0, 0.07f);
//            LeftFoot.PlayOneShot(LeftFoot.clip);
//            WaitForFootLeft(.55f);
//        }
//        if (LR == 0 && RightStep)
//        {
//            RightFoot.pitch = 1.5f + Random.Range(0, 0.07f);
//            RightFoot.PlayOneShot(RightFoot.clip);
//            WaitForFootRight(.55f);
//        }
//    }

//    int GetMovingTargetIndex()
//    {
//        int RetVal = Random.Range(0, POIArray.Length);
//        if (RetVal == CurrentMovingTargetIndex)
//        {
//            RetVal = GetMovingTargetIndex();
//        }
//        return RetVal;
//    }

//    float AgentTimer = 0;
//    bool ReachTrigger;



//    void Update()
//    {
//        CheckForPlayer();


//        float DelayTime = 0;

//        AgentTimer += Time.deltaTime;

//        if (IsChasing)
//        {

//            MovingTarget.position = PlayerTarget.position;
//            //animator.SetFloat("SpeedAdjust", 2);
//            if(HuntingDrone != null && HuntingDrone.volume < 1)
//            {
//                HuntingDrone.volume += .01f;
//            }
//            LTC.lookAtPlayerRatio = 1;

//            if(DistanceToPlayer <= 1.8f && !ReachTrigger)
//            {
//                //animator.SetLayerWeight(1,1);
//                //FBIK.enabled = true;
//                //LAIK.enabled = true;


//                DOTween.KillAll();
//                DelayTime = 0;
//                DOTween.To(() => FBIK.solver.rightHandEffector.positionWeight, x => FBIK.solver.rightHandEffector.positionWeight = x, 1f, 2f);
//                DOTween.To(() => FBIK.solver.rightArmChain.bendConstraint.weight, x => FBIK.solver.rightArmChain.bendConstraint.weight = x, 1, 2f);
//                DOTween.To(() => DelayTime, x => DelayTime = x, 1, 2f).OnUpdate(() =>
//                {
//                    animator.SetLayerWeight(1, DelayTime);
//                });
//                ReachTrigger = true;
//            }
//            if(DistanceToPlayer > 1.8f)
//            {
//                if (ReachTrigger)
//                {
//                    DOTween.KillAll();
//                    DelayTime = 1;
//                    DOTween.To(() => DelayTime, x => DelayTime = x, 0, .8f).OnUpdate(() =>
//                    {
//                        animator.SetLayerWeight(1, DelayTime);
//                    }).OnComplete(() =>
//                    {
//                        //FBIK.enabled = false;
//                        //LAIK.enabled = false;
//                    });
//                    DOTween.To(() => FBIK.solver.rightArmChain.bendConstraint.weight, x => FBIK.solver.rightArmChain.bendConstraint.weight = x, 0, .8f);
//                    DOTween.To(() => FBIK.solver.rightHandEffector.positionWeight, x => FBIK.solver.rightHandEffector.positionWeight = x, 0, 1f);
//                }

//                ReachTrigger = false;
//                //animator.SetLayerWeight(1, 0);
               
//            }

//        } else { // not chasing

//            animator.SetFloat("SpeedAdjust", 1);
//            if (ReachTrigger)
//            {
//                DelayTime = 1;
//                DOTween.KillAll();
//                DOTween.To(() => FBIK.solver.rightHandEffector.positionWeight, x => FBIK.solver.rightHandEffector.positionWeight = x, 0, .8f);
//                DOTween.To(() => FBIK.solver.rightArmChain.bendConstraint.weight, x => FBIK.solver.rightArmChain.bendConstraint.weight = x, 0, 1f);
//                DOTween.To(() => DelayTime, x => DelayTime = x, 0, .8f).OnUpdate(() =>
//                {
//                    animator.SetLayerWeight(1, DelayTime);
//                }).OnComplete(() =>
//                {
//                    //FBIK.enabled = false;
//                    //LAIK.enabled = false;
//                });
//            }

//            ReachTrigger = false;

//            if (HuntingDrone != null &&  HuntingDrone.volume > 0)
//            {
//                HuntingDrone.volume -= .01f;
//            }
//            LTC.lookAtPlayerRatio = 0;
//        }

//        if (AgentTimer > BaseTargetTime && !IsChasing)
//        {
//            int i = GetMovingTargetIndex();
//            MovingTarget.position = POIArray[i].position;
//            CurrentMovingTargetIndex = i;
//            AgentTimer = 0;
//            KeeperAgent.enabled = true;
//        }

//        // Keep the agent disabled while the puppet is unbalanced.
//        KeeperAgent.enabled = puppet.state == BehaviourPuppet.State.Puppet;
//        KeeperAgent.nextPosition = transform.position;
//        transform.rotation = KeeperAgent.transform.rotation;

//        //agent.updatePosition = false;
//        //agent.updateRotation = true;

//        //agent.nextPosition = transform.position;
//        // Update agent destination and Animator
    
//        if (KeeperAgent.enabled)
//        {
//            KeeperAgent.SetDestination(MovingTarget.position);
//            animator.SetFloat("Forward", KeeperAgent.velocity.magnitude * 0.25f);
//        }
//    }
//}
