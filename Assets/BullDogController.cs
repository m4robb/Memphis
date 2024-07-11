using UnityEngine;
using UnityEngine.Events;

public class BullDogController : MonoBehaviour
{
    public Animator AnimationClip;
    public UnityEvent ExecuteActions;
    public float Distance = 3;

    public void StartAnimation()
    {
        AnimationClip.SetFloat("Speed", 1);

    }

    public void ExecuteAction()
    {
        if (ExecuteActions != null) ExecuteActions.Invoke();
    }


    Transform MainCamera;

    bool Trigger;
    void Update()
    {
        if (!MainCamera)
        {
            MainCamera = Camera.main.transform;
            return;
        }

        if(Vector3.Distance(transform.position, MainCamera.position) < Distance && !Trigger)
        {
            Trigger = true;
            StartAnimation();
        }


    }
}
