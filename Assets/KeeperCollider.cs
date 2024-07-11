//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class KeeperCollider : MonoBehaviour
//{

//    public KeeperBrain KB;
//    public MeshCollider Floor;

//    void OnCollisionEnter(Collision collision)
//    {
//        if (collision.transform.tag == "KeeperHand")
//        {

//            Floor.enabled = false;
//            Invoke("KeeperFollow", .5f);

//        }

//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.transform.tag == "KeeperHand")
//        {

//            //Floor.enabled = false;
//            //Invoke("KeeperFollow", .5f);

//        }
//    }

//    // Update is called once per frame
//    void KeeperFollow()
//    {
//        KB.DisableKeeperPuppet();
//    }
//}
