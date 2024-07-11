using UnityEngine;

public class WizardController : MonoBehaviour
{
    public Animator WizardAnimation;
    void Start()
    {
        WizardAnimation.SetFloat("Speed", 0);
    }


    public void DoSpeed(float _Speed)
    {
        WizardAnimation.SetFloat("Speed", _Speed);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
