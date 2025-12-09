using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCTracker tracker;

    NavMeshAgent agent;
    List<Transform> waypoints;
    int index;

    public NPCPatrol(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;

        agent = controller.agent;
        waypoints = controller.waypoints;
    }

    public void Enter() { }

    void IState.Update()
    {
        Patrol();

        if (controller.getTarget() != null)
        {
            machine.changeState(tracker);
        }
    }

    public void Exit() { }

    void Patrol()
    {
        if (waypoints.Count == 0) return;

        float d = Vector3.Distance(waypoints[index].position, controller.transform.position);
        if (d <= 2) index = (index + 1) % waypoints.Count;

        agent.SetDestination(waypoints[index].position);
    }

    public void SetTracker(NPCTracker t)
    {
        tracker = t;
    }

    public void SetDependencies(NPCTracker npcTracker) {
        this.tracker = npcTracker;
    }
}
