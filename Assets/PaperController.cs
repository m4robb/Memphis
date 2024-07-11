using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperController : MonoBehaviour
{
    // Start is called before the first frame update

    public Cloth Paper;
    public Rigidbody RB;
    public Vector3 RandomAccelerationStart;
    public Vector3 ExternalAccelerationStart;
    public Vector3 RandomAccelerationRelease;

    public Vector3 ExternalAccelerationRelease;

    Vector3 ExternalAccelerationInternal;
    Vector3 RandomAccelerationInternal;

    void Start()
    {
        RandomAccelerationInternal = RandomAccelerationStart;
        ExternalAccelerationInternal = ExternalAccelerationStart;
    }

    public void RemoveCoefficients()
    {


        ClothSkinningCoefficient[] coefficients = Paper.coefficients;
        for (int i = 0, iend = coefficients.Length; i < iend; ++i)
        {
            if(i != Mathf.CeilToInt(coefficients.Length / 2))
                coefficients[i].maxDistance = float.MaxValue;

        }

        coefficients[0].maxDistance = 0;
        Paper.coefficients = coefficients;
        Paper.enabled = false;
        Paper.enabled = true;






    }


    public void Drop()
    {
        RB.isKinematic = false;

        Paper.useGravity = true;

        ClothSkinningCoefficient[] coefficients = Paper.coefficients;

        for (int i = 0, iend = coefficients.Length; i < iend; ++i)
        {
            coefficients[i].maxDistance =0.4f;

        }


        coefficients[0].maxDistance = 0;

  



        RandomAccelerationInternal = RandomAccelerationRelease;
        ExternalAccelerationInternal = ExternalAccelerationRelease;

        Paper.coefficients = coefficients;
        Paper.enabled = false;
        Paper.enabled = true;


    }
    // Update is called once per frame
    void Update()
    {
        Paper.randomAcceleration = RandomAccelerationInternal;
        Paper.externalAcceleration = ExternalAccelerationInternal;
    }
}
