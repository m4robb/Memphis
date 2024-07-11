using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using Unity.Mathematics;

[CanSelectMultiple(true)]

public class PositionReceiverHelper : Vector3AffordanceReceiver
{
    private Transform RotateThis;


    protected override void OnAffordanceValueUpdated(float3 newValue)
    {
        transform.localPosition = newValue;
        base.OnAffordanceValueUpdated(newValue);
    
    }



  
}
