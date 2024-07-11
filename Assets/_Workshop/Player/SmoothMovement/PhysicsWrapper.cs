using UnityEngine;

public class PhysicsWrapper : Physics
{
    public static int OverlapCapsuleNonAlloc(CapsuleCollider collider, out Collider[] overlapColliders, LayerMask collisionMask, int colliderCount)
    {
        overlapColliders = new Collider[colliderCount];
        Vector3 centerOffset = new Vector3(0, collider.height / 2.0f, 0);

        Vector3 topPosition = collider.transform.position + (collider.center + centerOffset);
        Vector3 bottomPosition = collider.transform.position + (collider.center - centerOffset);

        return OverlapCapsuleNonAlloc(topPosition, bottomPosition, collider.radius, overlapColliders, collisionMask);
    }

    public static bool ComputePenetration(PenetrationObject playerObject, PenetrationObject otherObject, out Vector3 direction, out float distance)
    {
        return ComputePenetration(playerObject.collider, playerObject.position, playerObject.rotation, otherObject.collider, otherObject.position, otherObject.rotation, out direction, out distance);
    }
}

public class PenetrationObject
{
    public Collider collider = null;
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;

    public PenetrationObject(Collider collider, Vector3 position, Quaternion rotation)
    {
        this.collider = collider;
        this.position = position;
        this.rotation = rotation;
    }

    public PenetrationObject(Collider collider)
    {
        this.collider = collider;
        position = collider.transform.position;
        rotation = collider.transform.rotation;
    }
}