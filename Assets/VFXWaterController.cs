using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

public class VFXWaterController : MonoBehaviour
{
    public VisualEffect VFXS;
    public float Duration, StartValue, FinishValue;
    void Start()
    {
        VFXS.SetFloat("CollisionHeight", StartValue);


        float tValue = StartValue;

        DOTween.To(() => tValue, x => tValue = x, FinishValue, Duration).OnUpdate(() =>
        {
            VFXS.SetFloat("CollisionHeight", tValue);
        });
        //float _StartValue = VFXS.GetFloat("CollisionHeight");
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
