using UnityEngine;
using GrabSystem;
using ChessEngine.Game;
using ChessInVR.Grabbing;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using RootMotion.FinalIK;

namespace ChessInVR
{
    /// <summary>
    /// A simple component that manages a chess piece entering a visual chess table tile trigger.
    /// </summary>
    /// Author: Mathew Aloisio
    public class ChessTilePieceTrigger : MonoBehaviour
    {
        #region Editor Serialized Setting(s)
        [Header("Settings")]
        [Tooltip("A reference to the VisualChessTableTile this trigger is for.")]
        public VisualChessTableTile visualTile;
        public GameObject SelectIndicator;
        public bool IsValidTarget;
        #endregion

        #region Unity Callback(s)
        void Awake()
        {
            if (visualTile == null)
                Debug.LogWarning("No 'visualTile' specified for ChessTilePieceTrigger!", gameObject);
        }

        void Start() { } // NOTE: This method is only included to force the 'enabled' checkbox to show in the editor.

        void Reset()
        {
            // Look for 'visualTile' reference.
            visualTile = GetComponentInParent<VisualChessTableTile>();
        }




        public void RemoteTrigger()
        {
           
            ExecuteTrigger(visualTile.GetVisualPiece());

        }

        void ExecuteTrigger(VisualChessPiece _VPC)
        {
            if (!_VPC) return;

   
            

            XRGrabInteractable XRBI = _VPC.transform.GetComponent<XRGrabInteractable>();

            if (XRBI.interactorsSelecting.Count == 0) return;

            ChessGrabber chessGrabber = XRBI.interactorsSelecting[0].transform.GetComponent<ChessGrabber>();



            if (chessGrabber != null)
            {
                chessGrabber.TriggerTile(this); // Trigger this tile for the found chess grabber.
               // chessGrabber.RemoteVisualChessPieceReleased();
            }
               

        }

        private void OnCollisionEnter(Collision collision)
        {
             

           // CollisionAction(collision.gameObject);
        }


        private void OnCollisionExit(Collision collision)
        {

            //CollisionExitAction(collision.gameObject);


        }


        void CollisionExitAction(GameObject pOther)
        {


                SelectIndicator.SetActive(false);
                //Debug.Log("Collider  Exited for " + pOther);
                // Look for VisualChessPiece that entered.
                VisualChessPiece visualPiece = pOther.GetComponent<VisualChessPiece>();
                if (visualPiece == null)
                {


                    //if (pOther.attachedRigidbody != null)
                    //    visualPiece = pOther.attachedRigidbody.GetComponent<VisualChessPiece>();
                }

                // Only handle visual pieces entering the trigger.
                if (visualPiece != null)
                {
                    // Ensure the visual piece is grabbable.
                    XRGrabInteractable XRBI = visualPiece.transform.GetComponent<XRGrabInteractable>();

                    if (XRBI.interactorsSelecting.Count == 0) return;

                    ChessGrabber chessGrabber = XRBI.interactorsSelecting[0].transform.GetComponent<ChessGrabber>();

                    if (chessGrabber != null)
                    {
                        if (chessGrabber.TriggeringTile == this)
                            chessGrabber.UntriggerTile();

                    }

                }
 
        }
        void CollisionAction(GameObject pOther)
        {
             
            if (pOther.tag== "ChessPiece" && enabled)
            {
                SelectIndicator.SetActive(true);
                Debug.Log(pOther.gameObject.name);
                // Look for VisualChessPiece that entered.
                VisualChessPiece visualPiece = pOther.GetComponent<VisualChessPiece>();
                if (visualPiece == null)
                {
                    Debug.Log("NoVCP");
                }

                // Only handle visual pieces entering the trigger.
                if (visualPiece != null)
                {
                    // Ensure the visual piece is grabbable.
                    XRGrabInteractable XRBI = visualPiece.transform.GetComponent<XRGrabInteractable>();


                    Debug.Log(XRBI.interactorsSelecting.Count);

                    if (XRBI.interactorsSelecting.Count == 0) return;

                    ChessGrabber chessGrabber = XRBI.interactorsSelecting[0].transform.GetComponent<ChessGrabber>();

                    if(chessGrabber != null)
                    {
                        
                        chessGrabber.TriggerTile(this); // Trigger this tile for the found chess grabber.

                    }

                }
            }
        }
        void OnTriggerEnter(Collider pOther)
        {
     
            CollisionAction(pOther.gameObject);

        }

        void OnTriggerExit(Collider pOther)
        {

            CollisionExitAction(pOther.gameObject);
            //// Look for VisualChessPiece that exited.
            //VisualChessPiece visualPiece = pOther.GetComponent<VisualChessPiece>();
            //if (visualPiece == null)
            //{
            //    if (pOther.attachedRigidbody != null)
            //        visualPiece = pOther.attachedRigidbody.GetComponent<VisualChessPiece>();
            //}

            //// Only handle visual pieces exiting. the trigger.
            //if (visualPiece != null)
            //{
            //    // Ensure the visual piece is grabbable.
            //    GrabbableObject grabbablePiece = visualPiece.GetComponent<GrabbableObject>();
            //    if (grabbablePiece != null)
            //    {
            //        // Ensure the piece is being grabbed.
            //        if (grabbablePiece.HeldByCount > 0)
            //        {
            //            // Check if the first grabber holding this piece (since there should only be 1 anyway) has a ChessGrabber.
            //            ChessGrabber chessGrabber = grabbablePiece.GetHeldBy(0).GetComponent<ChessGrabber>();
            //            if (chessGrabber != null)
            //            {
            //                // Ensure that the chess grabber is currently triggering this tile before un-triggering it.
            //                if (chessGrabber.TriggeringTile == this)
            //                    chessGrabber.UntriggerTile();
            //            }
            //        }
            //    }
            //}
        }
        #endregion
    }
}
