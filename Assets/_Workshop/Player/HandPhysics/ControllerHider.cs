using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PhysicsPoser))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor))]
public class ControllerHider : MonoBehaviour
{
    [SerializeField] private GameObject controllerObject = null;

    private PhysicsPoser physicsPoser = null;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor interactor = null;

    private void Awake()
    {
        physicsPoser = GetComponent<PhysicsPoser>();
        interactor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
    }

    private void OnEnable()
    {
        interactor.selectEntered.AddListener(Hide);
        interactor.selectExited.AddListener(Show);
    }

    private void OnDisable()
    {
        interactor.selectEntered.RemoveListener(Hide);
        interactor.selectExited.RemoveListener(Show);
    }

    private void Hide(SelectEnterEventArgs args)
    {
        controllerObject.SetActive(false);
    }

    private void Show(SelectExitEventArgs args)
    {
        StartCoroutine(WaitForRange());
    }

    private IEnumerator WaitForRange()
    {
        yield return new WaitWhile(physicsPoser.WithinPhysicsRange);
        controllerObject.SetActive(true);
    }
}
