using System.Collections;
using UnityEngine;


public class LineOfSight : MonoBehaviour
{
    [SerializeField] private GameObject target;
   public float detection_delay =.5f;

    public bool IsHidden = true;

    public LayerMask IgnoreLayers;

    public float Distance = 20;


    private Collider player_collider;
    private SphereCollider detection_collider;
    private Bounds player_bounds;
    private Coroutine detect_player;

    private void Awake() {
		player_collider = target.GetComponent<Collider>(); ;
		StartCoroutine(DetectPlayer());
    }

    void Hide()
    {
        IsHidden = false;

        Debug.Log("I SEE YOU!!!!");
    }


    bool HideTrigger;
    IEnumerator DetectPlayer()
    {
        while ( true )
        {
            yield return new WaitForSeconds(detection_delay);

            Vector3[] points = GetBoundingPoints( player_collider.bounds );

            int points_hidden = 0;

            foreach ( Vector3 point in points )
            {
                Vector3 target_direction = point - this.transform.position;
                float target_distance = Vector3.Distance( this.transform.position, point );
                float target_angle = Vector3.Angle( target_direction, this.transform.forward );

               

                

                if ( IsPointCovered( target_direction, target_distance ) || target_angle > 60)
                {
                    ++points_hidden;
                }
                else
                {
                     Debug.DrawLine(transform.position, point);
                }
                    

 
            }

            if ( points_hidden >= points.Length)
            {
                IsHidden = true;
                HideTrigger = false;
                CancelInvoke();
            }
            else
            {
                Hide();
           	}
                // player is visible
        }
    }

    private bool IsPointCovered( Vector3 target_direction, float target_distance )
    {
        RaycastHit[] hits = Physics.RaycastAll( this.transform.position, target_direction, 100, ~IgnoreLayers);

        foreach ( RaycastHit hit in hits )
        {
            if ( hit.transform.gameObject.layer != LayerMask.NameToLayer( "Player" ) )
            {
                float cover_distance = Vector3.Distance( this.transform.position, hit.point );

                if ( cover_distance < target_distance )
                    return true;
            }
        }

        if(target_distance > Distance) return true;

        return false;
    }

    private Vector3[] GetBoundingPoints( Bounds bounds )
    {
        Vector3[] bounding_points =
        {
            bounds.min,
            bounds.max,
            new Vector3( bounds.min.x, bounds.min.y, bounds.max.z ),
            new Vector3( bounds.min.x, bounds.max.y, bounds.min.z ),
            new Vector3( bounds.max.x, bounds.min.y, bounds.min.z ),
            new Vector3( bounds.min.x, bounds.max.y, bounds.max.z ),
            new Vector3( bounds.max.x, bounds.min.y, bounds.max.z ),
            new Vector3( bounds.max.x, bounds.max.y, bounds.min.z )
        };

        return bounding_points;
    }
}