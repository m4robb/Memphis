using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;


public class ShutterTeleport : MonoBehaviour
{
    public Transform PlayerRig;
    public Transform Destination;
    public Renderer Shutter;
    public UnityEvent OnTeleport;


    bool TeleTrigger;

    public void DoTeleport()
    {

        if(TeleTrigger) return;
        Shutter.gameObject.SetActive(true);
        TeleTrigger = true;
        Debug.Log("Teleport");

        Shutter.material.DOFade(1, 1).OnComplete(() =>
        {

            Debug.Log("Teleport FTB");
            PlayerRig.position = Destination.position;
            PlayerRig.rotation = Destination.rotation;  
            Shutter.material.DOFade(0, 1).OnComplete(() =>
            {
                Debug.Log("Teleport FI");
                OnTeleport.Invoke();
                TeleTrigger = false;
                Shutter.gameObject.SetActive(false);
            });
        });
    }
}
