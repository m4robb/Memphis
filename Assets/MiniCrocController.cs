using UnityEngine;

public class MiniCrocController : MonoBehaviour
{
    public float MinZ, MaxZ;
    Vector3 LocalPosition;
    float Direction = 1;
    public float ClockSpeed = .1f;
    void Start()
    {
        LocalPosition = transform.localPosition;
        gameObject.SetActive(false);
    }


    public void Activate(GameObject _GO)
    {
        gameObject.SetActive(true);
        Vector3 TempPostion = gameObject.transform.position;
        //  TempPostion.z = _GO.transform.position.z;

    }
    // Update is called once per frame
    void Update()
    {

        Vector3 TempAngle = transform.localEulerAngles;

        if (transform.localPosition.z > MaxZ)
        {


            TempAngle.y =180;
            Direction = -1;


        }

        if (transform.localPosition.z < MinZ)
        {
            TempAngle.y = 0;
            Direction = 1;

        }

        LocalPosition.z += ClockSpeed * Time.deltaTime * Direction;
        transform.localPosition = LocalPosition;
        transform.localEulerAngles = TempAngle;
    }
}
