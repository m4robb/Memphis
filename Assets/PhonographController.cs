using UnityEngine;
using UnityEngine.Events;

public class PhonographController : MonoBehaviour
{
    public Animator PhonographAnimator;
    public UnityEvent OnPlay;
    public UnityEvent OnStop;

    void Start()
    {
        if(!PhonographAnimator) PhonographAnimator = GetComponent<Animator>();
    }

    public bool IsPlaying;

    bool Trigger;


    public void ResetTrigger()
    {
        Trigger = false;
    }
    public void PlayPhonograph()
    {
        IsPlaying = true;
        OnPlay.Invoke();
    }

    public void StopPhonograph()
    {
        IsPlaying = false;
        OnStop.Invoke();
    }
    public void PhonographInteraction()


    {

        if (Trigger) return;
        Trigger = true;
        if (!IsPlaying)
        {
            IsPlaying = true;
            OnPlay.Invoke();
            return;
        }

        if (IsPlaying)
        {
            IsPlaying =false;
            OnStop.Invoke();
            return;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
