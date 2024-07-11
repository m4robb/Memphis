using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MxMGameplay
{
    public class AIDestinationSetter : MonoBehaviour
    {
        public Transform m_destinationTransform = null;
        [SerializeField] private bool m_Randomize;
        [SerializeField] private float m_timeToChangeTarget = 5f;
        [SerializeField] private float m_patrolRadius = 30f;


        private Vector3 m_lastDestination = Vector3.zero;

        private NavMeshAgent m_navAgent;

        private float m_moveTimer = 0f;



        private void Start()
        {
            m_navAgent = GetComponent<NavMeshAgent>();

            if(m_navAgent && m_destinationTransform && !m_Randomize)
            {
                Vector3 destination = m_destinationTransform.localPosition;
                m_navAgent.SetDestination(m_destinationTransform.localPosition);
            }

            if (m_Randomize)
            {
                //destination = RandomNavmeshLocation(m_patrolRadius);
                m_navAgent.SetDestination(RandomNavmeshLocation(m_patrolRadius));
            }

          
        }


        [SerializeField] private Vector3 destination;

        public void Update()
        {
            m_moveTimer += Time.deltaTime;


  

                if(m_Randomize && m_moveTimer > m_timeToChangeTarget)
                {
                    destination = RandomNavmeshLocation(m_patrolRadius);
                    m_navAgent.SetDestination(destination);
                    m_moveTimer = 0;
                }

                if (!m_Randomize)
                {
                    destination = m_destinationTransform.localPosition;

                    if (Vector3.SqrMagnitude(destination - m_lastDestination) > 0.01f)
                    {

                        m_navAgent.SetDestination(destination);
                        m_lastDestination = destination;

                    }
                }
  





               
 

        }

        public Vector3 RandomNavmeshLocation(float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }


            return finalPosition;
        }
    }
}
