using UnityEngine;

public class RainShadow : MonoBehaviour
{
    public Transform FollowTransform;


    // Update is called once per frame
    void Update()
    {
        Vector3 TempPosition = transform.position;
        TempPosition.x = FollowTransform.position.x;
        TempPosition.z = FollowTransform.position.z;
        transform.position = TempPosition;
    }
}
