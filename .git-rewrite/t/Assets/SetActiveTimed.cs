using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveTimed : MonoBehaviour
{
    public void SetActiveTrue(GameObject _GO)
    {
        StartCoroutine(SetActiveTrueTimed(_GO));
    }

    public void SetActiveFalse(GameObject _GO)
    {
        StartCoroutine(SetActiveFalseTimed(_GO));
    }

    private IEnumerator SetActiveTrueTimed(GameObject _GO)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        _GO.SetActive(true);
    }

   private IEnumerator SetActiveFalseTimed(GameObject _GO)
    {
        yield return new WaitForSeconds((float)AudioGridTick.AudioGridInstance.GetDelayTime());
        _GO.SetActive(false);
    }
}
