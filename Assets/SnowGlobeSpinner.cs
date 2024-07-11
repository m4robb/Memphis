using UnityEngine;
using Pathfinding;
using DG.Tweening;

public class SnowGlobeSpinner : MonoBehaviour
{

    public float Speed = 1;

    public RichAI RAI;
    Vector3 Angles;
    void Start()
    {
        Angles = transform.localEulerAngles;
    }

    public void StartRolling()
    {
        float _Speed = 0;

        DOTween.To(() => _Speed, x => _Speed = x, 1, 15).OnUpdate(() =>
        {

            RAI.maxSpeed = _Speed;
          
        });
    }
    void Update()
    {
        Angles.x += Speed * Time.deltaTime;
        transform.localEulerAngles = Angles;
        
    }
}
