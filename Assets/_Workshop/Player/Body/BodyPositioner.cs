using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BodyPositioner : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float positionBlend = 0.5f;

    // private XROrigin xrRig = null;
    
    

    private void Awake()
    {
        //xrRig = GetComponentInParent<XRRig>();
    }

    private void Update()
    {
        // UpdatePosition();
    }

    private void UpdatePosition()
    {
        // Vector3 headPosition = xrRig.cameraGameObject.transform.position;
        // Vector3 floorPosition = new Vector3(headPosition.x, xrRig.transform.position.y, headPosition.z);
        // transform.position = Vector3.Lerp(floorPosition, headPosition, positionBlend);
    }
}
