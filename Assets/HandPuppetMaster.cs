using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using DG.Tweening;

public class HandPuppetMaster : MonoBehaviour
{
   
    public GameObject SelectPuppet;
    public GameObject RightHandPuppet;
    public GameObject LeftHandPuppet;
    public GameObject SignalPuppet;
    public GameObject StartPuppet;

    public GameObject AllToys;
    public GameObject PostObjects;

    public Transform RightHand;
    public Transform LeftHand;

    public Transform RCurtain;
    public Transform LCurtain;
    public HeightController HC;
    public Volume PostPost;
    public Volume StagePost;
    public Renderer Shutter;

    public Animator PunchAnimator;

    public AudioSource WrongPuppetAS;
    public AudioClip[] WrongPuppetArray;
    

    public Transform SpotFollow;

    public Transform StartPuppetPosition;

    public float EnterStageDelayTime = 3;

    public MonkeyPianoPlayer MPP;

    public bool DoSignal = true;

    bool PunchIsEaten;

    public void EnterStage()
    {
        StartCoroutine(EnterStageComplete());
    }



    public void EatPunch()
    {


        LCurtain.gameObject.SetActive(true);
        RCurtain.gameObject.SetActive(true);
        RCurtain.DOScaleZ(1, 4).SetDelay(2);
        LCurtain.DOScaleZ(1, 4).SetDelay(2).OnComplete(() =>
        {
            PunchIsEaten = true;
            //PostPost.enabled = true;
            //float tValue = 0;
            //DOTween.To(() => tValue, x => tValue = x, 1f, 5f).OnUpdate(() =>
            //{
            //    PostPost.weight = tValue;
            //});

            float tValue2 = 1;
            DOTween.To(() => tValue2, x => tValue2 = x, 0, 5f).OnUpdate(() =>
            {
                StagePost.weight = tValue2;
            });

            AllToys.SetActive(false);
            PostObjects.SetActive(true);
        });

       
    }


    int WrongIndex = 0;
    public void WrongPuppet()
    {
        WrongPuppetAS.PlayOneShot(WrongPuppetArray[WrongIndex]);
        WrongIndex++;
        if (WrongIndex == WrongPuppetArray.Length) WrongIndex = 0;
    }

    public void ExitStage()
    {

        HC.TargetDistance = .5f;

        //if (PunchIsEaten)
        //{

        //    AllToys.SetActive(false);

        //    PostPost.enabled = true;
        //    float tValue = 0;
        //    DOTween.To(() => tValue, x => tValue = x, 1f, 3f).OnUpdate(() =>
        //    {
        //        PostPost.weight = tValue;
        //    });
        //}

    }

    List<GameObject> HaveSpoken = new List<GameObject>();

    public Animator SignalAnimator;

    public void AdjustHeight(float _Height) {

         HC.StartHeight = _Height;
    }

   

    IEnumerator EnterStageComplete()
    {
        yield return new WaitForSeconds(EnterStageDelayTime);

        if (StartPuppet)
        {
            //StartPuppet.transform.position = StartPuppetPosition.position;
            //StartPuppet.transform.rotation = StartPuppetPosition.rotation;
            StartPuppet = null;
        }

   
        //MPP.PlayerPiano();
        //_GO.SetActive(true);
    }

    public void SetSignaller(GameObject _GO)
    {

        if (HaveSpoken.Contains(_GO)) return;
        HaveSpoken.Add(_GO);
        //DOTween.KillAll();
       
        SignalPuppet = _GO;
        SignalAnimator = _GO.GetComponent<Animator>();
        Debug.Log("Setting Target " + _GO.GetComponent<PuppetHandController>().ConnectionPoint.position);

       //SpotFollow.DOMove(_GO.GetComponent<PuppetHandController>().ConnectionPoint.position, 5);
        SpotFollow.parent = _GO.GetComponent<PuppetHandController>().ConnectionPoint;
        SpotFollow.localPosition = Vector3.zero;

        DoSignal = true;
    }

    public void SwitchLightTarget(GameObject _GO)
    {
        SpotFollow.parent = _GO.GetComponent<PuppetHandController>().ConnectionPoint;
        SpotFollow.localPosition = Vector3.zero;
        //Debug.Log("Switching Target " + _GO.GetComponent<PuppetHandController>().ConnectionPoint.position);
        ////DOTween.KillAll();
        //SpotFollow.DOMove(_GO.GetComponent<PuppetHandController>().ConnectionPoint.position, 1);
    }



    public void PunchStruggles()
    {
 
        PunchAnimator.Play("Full", 0, 0);
    }

    public void Signal()
    {
        if (!DoSignal) return;
        SignalAnimator.Play("Full", 0, 0);
    }

    public void CheckSignal(GameObject _GO)
    {
        if(_GO == SignalPuppet)
        {
            DoSignal = false;
            if (_GO.GetComponent<PuppetHandController>().Signalled != null) _GO.GetComponent<PuppetHandController>().Signalled.Invoke();
        }
    }


    public void HoverHand(Transform _Hand)
    {

        if(_Hand.tag == "HandPuppet")
        {
        Debug.Log(_Hand);
        }

    }

    public void DoPuppet(GameObject _GO)
    {
        SelectPuppet = _GO;
    }
    void Update()
    {
        
    }
}
