using PhysicsTools.Rigidbodies;
using UnityEngine;

public class SnowglobeWaterFollow : MonoBehaviour
{
    [SerializeField]
    private Transform ForceFollowTransform;
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = ForceFollowTransform.position;
    }
}
