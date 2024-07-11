using UnityEngine;

public class BetweenTwoPoints : MonoBehaviour
{
    public Transform Point1;
    public Transform Point2;
    [Range(0, 1)]
    public float PositionPercentage = .5f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(Point1.position, Point2.position, PositionPercentage);
    }
}
