using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class PersistentGrab : MonoBehaviour
{
    XRBaseInteractable XRBI;

    Transform OriginalParent;

    void Start()
    {
        XRBI = GetComponent<XRBaseInteractable>();
        XRBI.selectEntered.AddListener(MakePersistent);
        XRBI.selectExited.AddListener(RemovePersistence);
        Debug.Log("Start Perssistent " + XRBI);
        OriginalParent = transform.parent;

    }

    private void OnEnable()
    {
 
       
    }

    private void OnDisable()
    {
        XRBI.selectEntered.RemoveListener(MakePersistent);
        XRBI.selectExited.RemoveListener(RemovePersistence);
    }

    void MakePersistent(SelectEnterEventArgs args)
    {

        Debug.Log("Add Objects");
        if(FallSceneManager.FallSceneManagerInstance && args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left )
        {
            FallSceneManager.FallSceneManagerInstance.InLeftHand = XRBI;
        }

        if (FallSceneManager.FallSceneManagerInstance && args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right)
        {
            FallSceneManager.FallSceneManagerInstance.InRightHand = XRBI;
        }

        DontDestroyOnLoad(this);
    }
    void RemovePersistence(SelectExitEventArgs args)
    {

        if (FallSceneManager.FallSceneManagerInstance && FallSceneManager.FallSceneManagerInstance.IsTransitioning) return;

        if (FallSceneManager.FallSceneManagerInstance && args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left)
        {
            FallSceneManager.FallSceneManagerInstance.InLeftHand = null;
        }

        if (FallSceneManager.FallSceneManagerInstance && args.interactorObject.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right)
        {
            FallSceneManager.FallSceneManagerInstance.InRightHand = null;
        }
        transform.parent = OriginalParent;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
