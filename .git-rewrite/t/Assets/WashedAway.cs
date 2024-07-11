using UnityEngine;
using DG.Tweening;

public class WashedAway : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void WashAway()
    {
        Vector3 TargetPosition = transform.position + transform.forward * -1.2f;
        transform.DOMove(TargetPosition, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
