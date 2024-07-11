using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



    public class ChessController : MonoBehaviour
    {
        // Start is called before the first frame update

        public Transform ClickSoundEmitter;
            public UnityEvent OnPieceCollide;

 

       public void PickUp()
    {
        ClickSoundEmitter.position = transform.position;
    }
    private void OnCollisionEnter(Collision collision)
    {
    if(collision.gameObject.tag == "ChessBoard" && OnPieceCollide!= null) OnPieceCollide.Invoke();  
    }

    void Start()
        {

        }



    }


    
