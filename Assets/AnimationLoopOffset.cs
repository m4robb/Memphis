using UnityEngine;

public class AnimationLoopOffset : MonoBehaviour
{
    public Animator AnimationController;
    public float Offset;
    void Start()
    {
        if (AnimationController == null) AnimationController = GetComponent<Animator>();
        if (AnimationController != null)
        {
            AnimationController.SetFloat("Offset", Offset );
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
