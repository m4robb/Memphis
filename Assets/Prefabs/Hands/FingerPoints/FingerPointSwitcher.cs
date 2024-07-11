using System.Collections;
using UnityEngine;


public class FingerPointSwitcher : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject FingerPointPrefab;

    IEnumerator DoSwitch(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor _Interactor, float _Delay)
    {

        HandManager HM = _Interactor.GetComponentInChildren<HandManager>();

        yield return new WaitForSeconds(_Delay);

        if (HM != null)
        {
            HM.BuildHand(FingerPointPrefab);
        }
    }

    public void SwitchFingerPoint(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor _Interactor)
    {
        StartCoroutine(DoSwitch(_Interactor,(float)AudioGridTick.AudioGridInstance.GetDelayTime()));

    }
}
