
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class KeeperWhisker : MonoBehaviour
{
    [SerializeField] private UnityEvent OpenDoorAction;
    [SerializeField] private UnityEvent CloseUmbrellaAction;
    [SerializeField] private UnityEvent DestroyKeeperAction;
    [SerializeField] private GameObject Keeper;
    void Start()
    {
        
    }
    IEnumerator OpenDoor(Collider other)
    {
        yield return new WaitForSeconds(1);
        Debug.Log("Keeper Open Door");
        other.GetComponent<OpenDoor>().AutoOpen();
    }


    public void ResetWhisker() {
        StoredObject = null;
    }

    GameObject StoredObject;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Whisker "  + other.tag);

        if (other.tag == "DestroyKeeper")
        {
            Debug.Log("Keeper Destroyer");
            DestroyKeeperAction.Invoke();

        }

        if (other.tag == "UmbrellaCloser" && other.gameObject != StoredObject)

        {

            StoredObject = other.gameObject;

            Debug.Log("Umbrella Closer!!");


            if (CloseUmbrellaAction != null)
            {
                Debug.Log("Umbrella Do Close!!");
                CloseUmbrellaAction.Invoke();
             
            }
            other.gameObject.SetActive(false);
        }

        if (other.tag == "Door" && other.gameObject != StoredObject)
            
        {

            Debug.Log("DoorOpen");

            StoredObject = other.gameObject;

            if (OpenDoorAction != null)
            {
                OpenDoorAction.Invoke();
            }

            if (other.gameObject.GetComponent<OpenDoor>())
            {
                StartCoroutine(OpenDoor(other));
            }

        }
    }

    void Update()
    {
        
    }
}
