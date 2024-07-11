using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ElevatorFloorIndicator : MonoBehaviour
{
    public Texture[] TextureArray;
    public float Speed;

    public Renderer R;
    public int ElevatorState = 0;

    public int FloorNumber, Destination;
    public int CurrentFloor;

    public UnityEvent ReachCurrentFloor;

    

    void Start()
    {
        
    }

    public void SetFloor(int _Floor)
    {
        Destination = _Floor;
    }

    void ChangeFloor()
    {

        if (!TextureArray[FloorNumber]) return;
        R.material.SetTexture("_EmissiveColorMap", TextureArray[FloorNumber]);
        if (FloorNumber == CurrentFloor && ReachCurrentFloor != null) ReachCurrentFloor.Invoke();
    }

    float Timer = 0;
    void Update()
    {

        if(ElevatorState != 0)
        {
            Timer += Time.deltaTime;
        }

        if(Timer > Speed)
        {
            if (FloorNumber < Destination) FloorNumber++;
            if (FloorNumber > Destination) FloorNumber--;
            ChangeFloor();
            Timer = 0;
        }

        if (FloorNumber == Destination) ElevatorState = 0;
        if (FloorNumber != Destination) ElevatorState = 1;
    }
}
