using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.HighDefinition;

public class CTAController : MonoBehaviour
{

    public Transform PlayerRig;
    public Transform PlayerTarget;
    public Light BrightLight;
    public float FirstThreshold;
    public float SecondThreshold;
    public float FirstThresholdTimer;
    public float SecondThresholdTimer;
    public UnityEvent OnHmdon;
    public UnityEvent OnFirstThreshold;
    public UnityEvent OnSecondThreshold;
    public HDAdditionalLightData HDALD;
    public SceneChunkLoader SCL;
    bool HasPassedFirstThreshold;
    bool HasPassedSecondThreshold;
    bool HasPickedUpGlobe;
    bool HMDTrigger;


    float Timer;
    float OriginalDistanceToLight;
    float OriginalLightValue;
    public void CTAComplete()
    {
        HasPickedUpGlobe = true;
        
    }

    private void Start()
    {
        OriginalDistanceToLight = Vector3.Distance(PlayerRig.position, PlayerTarget.position);
        OriginalLightValue = BrightLight.intensity;

         
    }

    void Update()
    {
        if(!SCL.HMDOn) { return; }

        float DistanceToTarget = Vector3.Distance(PlayerRig.position, PlayerTarget.position);

        float Multiplier = (DistanceToTarget -1) / OriginalDistanceToLight ;

        BrightLight.intensity = Mathf.Clamp(Multiplier * OriginalLightValue, 0, OriginalLightValue);

        Debug.Log(Multiplier);

        if (SCL.HMDOn && !HMDTrigger) { OnHmdon.Invoke(); HMDTrigger = true; }

        if (HasPickedUpGlobe) return;

        if(!HasPassedFirstThreshold && DistanceToTarget > FirstThreshold)
        {
            Timer += Time.deltaTime;
            if(Timer > FirstThresholdTimer )
            {
                HasPassedFirstThreshold = true;
                Timer = 0;
                OnFirstThreshold.Invoke();
            }
        }

        if (!HasPassedFirstThreshold && DistanceToTarget < FirstThreshold) HasPassedFirstThreshold = true;

        if (HasPassedFirstThreshold && !HasPassedSecondThreshold && DistanceToTarget < SecondThreshold)
        {
            Timer += Time.deltaTime;
            if (Timer > SecondThresholdTimer)
            {
                HasPassedSecondThreshold = true;
                Timer = 0;
                OnSecondThreshold.Invoke();
            }
        }
    }
}
