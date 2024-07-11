using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;


public enum ButtonAxis
{
    XAxis,
    YAxis,
    ZAxis
};

public class HandButton : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    public UnityEvent OnPress = null;

    public ButtonAxis ButtonPushDirection = new ButtonAxis();

    private float yMin = 0.0f;
    private float yMax = 0.0f;
    private bool previousPress = false;

    private float previousHandHeight = 0.0f;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable hoverInteractor = null;

    protected override void Awake()
    {
        base.Awake();
        hoverEntered.AddListener(StartPress);
        hoverExited.AddListener(EndPress);
    }

    private void OnDestroy()    
    {
        hoverEntered.RemoveListener(StartPress);
        hoverExited.RemoveListener(EndPress);
    }

    private void StartPress(HoverEnterEventArgs args)
    {
        OnPress.Invoke();
    
        hoverInteractor = args.interactableObject;
        previousHandHeight = GetLocalYPosition(hoverInteractor.transform.position);
    }

    private void EndPress(HoverExitEventArgs args)
    {
        hoverInteractor = null;
        previousHandHeight = 0.0f;

        previousPress = false;
        SetYPosition(yMax);
    }

    private void Start()
    {

        SetMinMax();
    }

    private void SetMinMax()
    {
        Collider collider = GetComponent<Collider>();

        if(ButtonPushDirection == ButtonAxis.YAxis)
        {
        yMin = transform.localPosition.y - (collider.bounds.size.y * 0.5f);
        yMax = transform.localPosition.y;
        }

        if (ButtonPushDirection == ButtonAxis.XAxis)
        {
            yMin = transform.localPosition.x ;
            yMax = transform.localPosition.x + (collider.bounds.size.x * 0.5f);
        }

        if (ButtonPushDirection == ButtonAxis.ZAxis)
        {
            yMin = transform.localPosition.z - (collider.bounds.size.z * 0.5f);
            yMax = transform.localPosition.z;
        }

    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if(hoverInteractor != null)
        {
            float newHandHeight = GetLocalYPosition(hoverInteractor.transform.position);
            float handDifference = previousHandHeight - newHandHeight;
            previousHandHeight = newHandHeight;
            float newPosition = 0;

            if (ButtonPushDirection == ButtonAxis.YAxis)
                newPosition = transform.localPosition.y - handDifference;


            if (ButtonPushDirection == ButtonAxis.XAxis)
                newPosition = transform.localPosition.x + handDifference;


            if (ButtonPushDirection == ButtonAxis.ZAxis)
                newPosition = transform.localPosition.z - handDifference;

            SetYPosition(newPosition);

            CheckPress();
        }
    }

    private float GetLocalYPosition(Vector3 position)
    {
        Vector3 localPosition = transform.root.InverseTransformPoint(position);

        if (ButtonPushDirection == ButtonAxis.YAxis)
            return localPosition.y;

        if (ButtonPushDirection == ButtonAxis.XAxis)
            return localPosition.x;

        if (ButtonPushDirection == ButtonAxis.ZAxis)
            return localPosition.z;

        return localPosition.y;
    }

    private void SetYPosition(float position)
    {
        Vector3 newPosition = transform.localPosition;

        if (ButtonPushDirection == ButtonAxis.YAxis)
            newPosition.y = Mathf.Clamp(position, yMin, yMax);

        if (ButtonPushDirection == ButtonAxis.XAxis)
            newPosition.x = Mathf.Clamp(position, yMax, yMin);

        if (ButtonPushDirection == ButtonAxis.ZAxis)
            newPosition.z = Mathf.Clamp(position, yMin, yMax);

        transform.localPosition = newPosition;
    }

    private void CheckPress()
    {
        bool inPosition = InPosition();

        if (inPosition && inPosition != previousPress)
            OnPress.Invoke();

        previousPress = inPosition;
    }

    private bool InPosition()
    {
        float inRange = 0;
        
        if (ButtonPushDirection == ButtonAxis.YAxis)
        {
            inRange = Mathf.Clamp(transform.localPosition.y, yMin, yMin + 0.01f);
            return transform.localPosition.y == inRange;
        }
            
        if (ButtonPushDirection == ButtonAxis.XAxis)
        {
            inRange = Mathf.Clamp(transform.localPosition.x, yMin, yMin - 0.01f);
            return transform.localPosition.x == inRange;
        }
            
        if (ButtonPushDirection == ButtonAxis.ZAxis)
        {
            inRange = Mathf.Clamp(transform.localPosition.z, yMin, yMin + 0.01f);
            return transform.localPosition.z == inRange;
        }
           

        return transform.localPosition.y == inRange;
    }

    public void ButtonPressed()
    {
        print("BUTTON PRESSED");
    }
}
