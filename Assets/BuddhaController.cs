using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuddhaController : MonoBehaviour
{
    public LayerMask layerMask;
    public GameObject Whole;
    public GameObject Pieces;
    public float BreakThreshold = 2;
    public Rigidbody RB;

    float KineticEnergy(Rigidbody rb)
    {
        return 0.5f * rb.mass * Mathf.Pow(rb.linearVelocity.magnitude, 2);
    }

    IEnumerator TurnOnSounds(GameObject _GO)
    {
        yield return new WaitForSeconds(1);
        CollisionSoundMaker[] _CSMArray = GetComponentsInChildren<CollisionSoundMaker>();
        foreach (CollisionSoundMaker _CSM in _CSMArray) _CSM.enabled = true;

    }

    private void OnCollisionEnter(Collision collision)
    {

        if (KineticEnergy(RB) < BreakThreshold) return;
        Pieces.transform.parent = transform.parent;
        Pieces.SetActive(true);
        gameObject.SetActive(false);
      
    }
}
