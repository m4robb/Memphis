using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FrontHallwayController : MonoBehaviour
{
    // Start is called before the first frame update
    Camera MainCamera;
    public Transform FrontDoor;
    public GameObject OutFrontContent;

    Collider RoomCollider;

    bool HasLeftRoom;

    void Start()
    {
        MainCamera = Camera.main;
        RoomCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!RoomCollider.bounds.Contains(MainCamera.transform.position) && !HasLeftRoom)
        {

            HasLeftRoom = true;
            FrontDoor.DOLocalRotate(new Vector3(0, 90, -90), 5).OnComplete(() =>
            {
                OutFrontContent.SetActive(false); 
            });
        }


    }
}
