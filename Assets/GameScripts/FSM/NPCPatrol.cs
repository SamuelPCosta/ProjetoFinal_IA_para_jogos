using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    public List<Transform> waypoints;
    NavMeshAgent navMeshAgent;
    public int currentWaypointIntex = 0;
        
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (waypoints.Count == 0)
        {
            return;
        }

        float distanceToWayPoint = Vector3.Distance(waypoints[currentWaypointIntex].position, transform.position);

        if (distanceToWayPoint <= 2)
        {
            currentWaypointIntex = (currentWaypointIntex + 1) % waypoints.Count;
        }

        navMeshAgent.SetDestination(waypoints[currentWaypointIntex].position);
    }
}
