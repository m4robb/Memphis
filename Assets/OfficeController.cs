using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OfficeController : MonoBehaviour
{
    // Start is called before the first frame update

    Camera MainCamera;
    MeshFilter RoomMesh;
    Collider RoomCollider;
    bool IsInRoom;

    public AudioSource StairwellMusic;
    public AudioSource Phonograph;
    HingeJoint HJ;
    public DoorController DC;

    public GameObject OfficeContent;
    public GameObject StairwellContent;

    void Start()
    {
        MainCamera = Camera.main;
        RoomMesh = GetComponent<MeshFilter>();
        RoomCollider = GetComponent<Collider>();
    }

    IEnumerator RemoveStairwell()
    {
        yield return new WaitForSeconds(5);
        StairwellContent.SetActive(false);
        if (MainAudioController.MainAudioControllerInstance)
        MainAudioController.MainAudioControllerInstance.InitMelodies = false;
    }
    void Update()
    {


        if(Mathf.Abs(DC.HJ.angle) > 5)
        {
            OfficeContent.SetActive(true);
        }

        if (RoomCollider.bounds.Contains(MainCamera.transform.position) && !IsInRoom)
        {
            StairwellMusic.DOFade(0, 3f).OnComplete(()=> {
                StairwellMusic.Pause();
            });
           
            Phonograph.volume = 0;
            Phonograph.Play();
            Phonograph.DOFade(1, 3f);
            IsInRoom = true;
            return;
        }

        if (RoomCollider.bounds.Contains(MainCamera.transform.position))
        {

            if (Vector3.Distance(MainCamera.transform.position, DC.transform.position) > 2)
            {
                DC.CloseDoor();
                StartCoroutine(RemoveStairwell());
            }
        }


            if (!RoomCollider.bounds.Contains(MainCamera.transform.position) && IsInRoom)
        {

            Phonograph.DOFade(0, 3f).OnComplete(() => {
                Phonograph.Pause();
            });
            StairwellMusic.Play();
            StairwellMusic.DOFade(0, 3f);
            IsInRoom = false;
            Debug.Log("Bounding box contains hidden object!");
            return;
        }
    }
}
