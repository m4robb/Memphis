using UnityEngine;

using UnityEngine.Events;

public class UmbrellaController : MonoBehaviour
{
    public bool CheckIfUpright;
    public float Threshold = .6f;
    public UnityEvent OnUpright;
    public UnityEvent OnDown;
    public Animator UmbrellaAnimator;

    void Start()
    {
        
    }

    bool TriggerUp, TriggerDown;
    public bool IsUpright()
    {
        return transform.up.y > Threshold; 
    }
    float OpenTarget = 0;
    float CurrentTarget = 0;
    void Update()
    {
        if(UmbrellaAnimator != null)
        {
            CurrentTarget = Mathf.Lerp(CurrentTarget,OpenTarget, Time.deltaTime * 4f);
            UmbrellaAnimator.SetLayerWeight(1, CurrentTarget);
        }


        if (!CheckIfUpright) return;

        if(IsUpright())
        {
            OpenTarget = 1;
            //if (!TriggerUp)
            //{
            
            //TriggerDown = false;
            //TriggerUp = true;
            //OnUpright.Invoke();
            //}

            TriggerDown = false;
            if (OnUpright != null && !TriggerUp)
            {
                TriggerUp = true;

                OnUpright.Invoke();
            }
        }

        if (!IsUpright())
        {
   
            OpenTarget = 0;
            TriggerUp = false;
            //Debug.Log("Down");
            //TriggerDown = true;
            //TriggerUp = false;

            if (OnDown != null && !TriggerDown )
            {
                TriggerDown = true;
                OnDown.Invoke();
            }
            //OnDown.Invoke();
        }
    }
}
