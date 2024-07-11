using UnityEngine;
using DG.Tweening;

public class WomanSwimController : MonoBehaviour
{

    public Animator Anim;
    public Transform HeadCamStabilized;

    public int DelayTime = 60;

    bool CanStartAnimation1, CanStartAnimation2, Trigger;

    Transform HeadCam;

    private void Start()
    {
        HeadCam = Camera.main.transform;
    }


    public void DoClick()
    {

        if (DoClickTrigger)
        {
        Counter++;

       // Debug.Log(Counter + " " + DelayTime);
        }


    }
    int Counter = 0;
    bool DoClickTrigger, IsLooping;

    private void Update()
    {

        if (AudioGridTick.AudioGridInstance != null && !IsLooping)
        {
            IsLooping = true;
            AudioGridTick.AudioGridInstance.storedAudioAction += DoClick;
           

        }

       

        if (Counter >= DelayTime)
        {
            ChangeAnimationSpeed();

        }


        if (CanStartAnimation1)
        {

            float _Angle = Vector3.Angle(transform.position, HeadCam.forward);



            if (_Angle > 65 && !CanStartAnimation2)
            {
                CanStartAnimation2 = true;
            }

            if (_Angle < 25 && CanStartAnimation2 && !Trigger)
            {
                Trigger = true;

                ChangeAnimationSpeed();
            }




        }
        
    }


    bool HasChangedSpeed;

    void ChangeAnimationSpeed()
    {
        float _Speed = Anim.GetFloat("Speed");

        if (_Speed > 0) return;

        DOTween.To(() => _Speed, x => _Speed = x, 1, 15).OnUpdate(() =>
        {
            Anim.SetFloat("Speed", _Speed);
        });
    }

    private void OnDisable()
    {
        AudioGridTick.AudioGridInstance.storedAudioAction -= DoClick;
    }


    public void StartCounter()
    {

        Debug.Log ("Startíng Counter");
        DoClickTrigger = true;
    }

    public void StartAnimation()
    {
        DoClickTrigger = true;
        CanStartAnimation1 = true;

    }
}
