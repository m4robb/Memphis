using UnityEngine;

public class CameraPositionSwitcher : MonoBehaviour
{
    public Transform[] Positions;
    public Transform CameraRig;

    Vector3 StoredPosition;
    Quaternion StoredRotation;

    void Start()
    {
        
    }

    public void SetCameraRestorePoint()
    {
        StoredPosition = CameraRig.transform.position;
        StoredRotation = CameraRig.transform.rotation;
    }

    public void RestoreCameraTranform()
    {
        CameraRig.position = StoredPosition;
        CameraRig.rotation = StoredRotation;

    }
     public void SwitchPosition(int _PositionIndex)
    {
        CameraRig.position = Positions[_PositionIndex].position;
    }


}
