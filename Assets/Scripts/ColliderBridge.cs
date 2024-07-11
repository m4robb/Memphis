using UnityEngine;

public class ColliderBridge : MonoBehaviour
 {
     PhysicsButton PB;

     public void Initialize(PhysicsButton _PB)
     {
        PB = _PB;
     }

     void OnCollisionEnter(Collision collision)
     {
        PB.OnCollisionEnter(collision);
     }
    void OnCollisionExit(Collision collision)
    {
        PB.OnCollisionExit(collision);
    }
}