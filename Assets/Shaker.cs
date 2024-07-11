using UnityEngine;
using UnityEngine.Events;

public class Shaker : MonoBehaviour
{
    public UnityEvent OnShake;
    public float TriggerThreshold;
    public float TimeToReset = 1;
    public Rigidbody RB;

    private float ResetTimer;
    
    void Start()
    {
        ResetTimer = TimeToReset;
        if (RB != null) RB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (RB.linearVelocity.sqrMagnitude > TriggerThreshold && ResetTimer > TimeToReset && OnShake != null)
        {
            OnShake.Invoke();
            ResetTimer = 0;
        }

        ResetTimer += Time.deltaTime;
    }
}
