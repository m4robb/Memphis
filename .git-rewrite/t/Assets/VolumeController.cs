using UnityEngine;
using DG.Tweening;
using com.zibra.common;
using com.zibra.liquid.Manipulators;

public class VolumeController : MonoBehaviour
{
    public float VolumeStartY, VolumeEndY;
    public float Duration = 55f;
    public Transform Waterline;
    public BoxCollider BC;
    public float LDBBHeight;
    public ZibraLiquidDetector LiquidDetector;

    public int NumberOfParticles;

    public float FloodPercentage;

    Vector3 BCCenter;
    void Start()
    {
        VolumeStartY = transform.localPosition.y;

       

    
        //DOTween.To(() => tValue, x => tValue = x, VolumeEndY, Duration).OnUpdate(() =>
        //{
        //    VFXS.SetFloat("CollisionHeight", tValue);
        //});
    }

    // Update is called once per frame
    void Update()
    {
        
        LDBBHeight = LiquidDetector.BoundingBoxMax.y - LiquidDetector.BoundingBoxMin.y;
        BCCenter = BC.center;
        BCCenter.y = LiquidDetector.BoundingBoxMin.y;
        BC.center = BCCenter;
        NumberOfParticles = LiquidDetector.ParticlesInside;
        FloodPercentage = (float)LiquidDetector.ParticlesInside /3600f;
        Vector3 _TempSize = BC.size;
        _TempSize.y = LDBBHeight * 2;
        BC.size = _TempSize;

    }
}
