using System;
using UnityEngine;

public class SocketController : MonoBehaviour
{
    public static SocketController SocketControllerInstance;
    
    public delegate void StoredSocketSnapAction();
    public event StoredSocketSnapAction StoredSocketSnapEvent;
    public delegate void StoredSocketReleaseAction();
    public event StoredSocketReleaseAction StoredSocketReleaseEvent;

    

    void Start()
    {
        SocketControllerInstance = this;
    }

    public void DoSocketSnap()
    {

        Debug.Log("hello snap 1");
        
        if (StoredSocketSnapEvent != null)
        {
            Debug.Log("hello snap 2");
            StoredSocketSnapEvent();
        }
    }

    void InvokedSnap()
    {

    }
    
    public void DoSocketRelease()
    {
        Debug.Log("hello release 1");
        if (StoredSocketReleaseEvent != null)
        { 
            Debug.Log("hello release 2");
             StoredSocketReleaseEvent();
        }
           
    }


}
