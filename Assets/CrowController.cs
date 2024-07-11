using UnityEngine;
using UnityEngine.Events;

public class CrowController : MonoBehaviour
{

    public Animator Anim;
    public UnityEvent OnAttack;

    public float MinDistance;
    public float MaxDistance;

    public Transform Prey;

    public void Attack()
    {

        if (OnAttack != null) OnAttack.Invoke();
        Anim.CrossFadeInFixedTime("Attack01", .1f);
    }
    public void TakeOff()
    {
       
        Anim.CrossFadeInFixedTime("TakeOff", 1f);

    }

    bool Trigger;

    private void Update()
    {
        float Distance = Vector3.Distance(transform.position, Prey.position);

        if(Distance < MinDistance && !Trigger)
        {
             Trigger = true;
            Attack();
        }

        if (Distance > MaxDistance) Trigger = false;

    }
}
