using UnityEngine;
using DG.Tweening;
public class ModifyBoneInFixedUpdate : MonoBehaviour
{
    public bool animatePhysics;
    public Animator anim; //Reference of the animator
    public Transform bone;      //Bone to modify
                                //
    public Transform bone2;      //Bone to modify 
    public Vector3 offset = new Vector3(0, 90, 0); //offset to apply to the bone
    public Vector3 PositionOffset = new Vector3(0, 0, 0); //offset to apply to the bone
    [Range(0, 1)]
    public float Weight = 1;


    Quaternion BaseRotation;
    Vector3 BasePosition, BasePosition2;

    public float Interval = 8;

    float TotalTime;
    private void Start()
    {
        BaseRotation = bone.rotation;

        BasePosition = bone.localPosition;
        BasePosition2 = bone2.localPosition;



    }

    public void WeightCycle()
    {

        Weight = 0;

        float _Duration = 60 / (float)AudioGridTick.AudioGridInstance.bpm * Interval / 2;

        DOTween.To(() => Weight, x => Weight = x,1 , _Duration).OnComplete(() =>
        {
            DOTween.To(() => Weight, x => Weight = x, 0, _Duration);
        });
       
    }

    void LateUpdate()
    {
        //TotalTime += Time.deltaTime * Speed;

        //Weight = Mathf.PingPong(TotalTime, 1);
        if (animatePhysics)
            anim.Update(0);

        //bone.rotation = Quaternion.Lerp(BaseRotation, BaseRotation * Quaternion.Euler(offset), Weight); 
        bone2.localPosition = Vector3.Lerp(BasePosition2, BasePosition2 + PositionOffset, Weight);
        bone.localPosition = Vector3.Lerp(BasePosition, BasePosition + PositionOffset, Weight);
    }
}

 