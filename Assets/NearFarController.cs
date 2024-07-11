using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class NearFarController : MonoBehaviour
{
    [SerializeField]

    XRBaseInputInteractor LeftInteractor;

    [SerializeField]

    XRBaseInputInteractor RightInteractor;

    [SerializeField]

    SphereInteractionCaster LeftCaster;

    [SerializeField]

    SphereInteractionCaster RightCaster;


    void Start()
    {
        
    }


    public void SetUpForChess()
    {
        LeftCaster.castRadius = .04f;
        LeftInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;

        RightCaster.castRadius = .04f;
       RightInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;
    }

    public void UnSetUpForChess()
    {
        LeftCaster.castRadius = .1f;
        LeftInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.Sticky;

        RightCaster.castRadius = .1f;
        RightInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.Sticky;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
