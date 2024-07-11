using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using DG.Tweening;
using RealisticEyeMovements;

public class SleepingManController : MonoBehaviour
{
    // Start is called before the first frame update

    public Animator MainAnimator;
    public PuppetMaster PM;
    public FullBodyBipedIK FBBIK;
    public LookAtIK LAIK;
    public EyeAndHeadAnimator EHA;

    bool HasWokenUp;

    void Start()
    {
        StartCoroutine(GoToSleep(0));   
    }

    public void HearsRinger(GameObject _GO)
    {
      
        CollisionSound _CS = _GO.GetComponent<CollisionSound>();

        if (_CS != null && _CS.SoundVolume > 0.01f)
        {

            if (!HasWokenUp)
            {
                StartCoroutine(WakeUp(2f));
                //HasWokenUp = true;
            }
           

        }
    }

    IEnumerator GoToSleep(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        PM.state = PuppetMaster.State.Dead;
    }

    IEnumerator WakeUp(float _Delay)
    {
        yield return new WaitForSeconds(_Delay);
        PM.state = PuppetMaster.State.Alive;
        float FadeInterval = 0;

        MainAnimator.CrossFadeInFixedTime("Reaching", 1f, 3);






        DOTween.To(() => EHA.controlData.eyeWidenOrSquint, x => EHA.controlData.eyeWidenOrSquint = x, 1, 2f);

        DOTween.To(() => LAIK.solver.IKPositionWeight, x => LAIK.solver.IKPositionWeight = x, 1, 2f).OnComplete(() =>
        {

            DOTween.To(() => FadeInterval, x => FadeInterval = x, 1f, 6f).OnUpdate(() =>
            {
                MainAnimator.SetLayerWeight(1, FadeInterval);

            });
            DOTween.To(() => FBBIK.solver.rightHandEffector.positionWeight, x => FBBIK.solver.rightHandEffector.positionWeight = x, .8f, 6f).OnComplete(() =>
            {
                Invoke("Subside", 5);
            });
        });


    }

    void Subside()
    {
        float FadeInterval = 1;

        DOTween.To(() => FBBIK.solver.rightHandEffector.positionWeight, x => FBBIK.solver.rightHandEffector.positionWeight = x, 0f, 6f);

        DOTween.To(() => FadeInterval, x => FadeInterval = x, 0f, 3f).OnUpdate(() =>
        {
            MainAnimator.SetLayerWeight(1, FadeInterval);

        }).OnComplete(() => {
            StartCoroutine(GoToSleep(0));
        });
    }

    //public void GetUpFrom

    // Update is called once per frame
    void Update()
    {
        
    }
}
