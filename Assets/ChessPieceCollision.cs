using ChessEngine.Game;
using UnityEngine;

public class ChessPieceCollision : MonoBehaviour
{
    public Rigidbody RB;
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
    }
    private void OnCollisionEnter(Collision collision)
    {

        Debug.Log(collision.transform.tag);
        if (collision.transform.tag != "ChessTile") return;
        VisualChessTableTile VCT = collision.gameObject.GetComponent<VisualChessTableTile>();
        if (VCT != null)
        {
            Debug.Log("FoundTile");
            RB.isKinematic = true;
            //VCT.Select();
        }
        {

        }
    }
}
