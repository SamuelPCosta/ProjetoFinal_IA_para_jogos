using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCTracker tracker;

    bool wait;
    float timer;
    float minDistanceToPoint = 2f;

    NavMeshAgent agent;
    List<Transform> waypoints;
    int index;

    Vector3 center = Vector3.zero;

    public NPCPatrol(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;

        agent = controller.agent;
        waypoints = controller.waypoints;

        ComputeCenter();
    }

    public void Enter() { }

    void IState.Update(){
        Patrol();

        if (controller.getTarget() != null){
            machine.changeState(tracker);
        }
    }

    public void Exit() {
        controller.setCenterpoint(Vector3.zero);
    }

    void Patrol()
    {
        if (wait){
            timer += Time.deltaTime;
            controller.transform.rotation =
                Quaternion.RotateTowards(
                    controller.transform.rotation,
                    Quaternion.LookRotation(center - controller.transform.position),
                    Time.deltaTime * 180f
                );

            controller.setCenterpoint(center);

            if (timer >= controller.TimePatrol)
            {
                wait = false;
                timer = 0f;
            }
            return;
        }

        if (waypoints.Count == 0) return;

        float d = Vector3.Distance(waypoints[index].position, controller.transform.position);
        if (d <= minDistanceToPoint){
            index = (index + 1) % waypoints.Count;
            wait = true;
            return;
        }

        agent.SetDestination(waypoints[index].position);
    }

    void ComputeCenter()
    {
        if (waypoints.Count == 0) return;
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < waypoints.Count; i++) sum += waypoints[i].position;
        center = sum / waypoints.Count;
    }

    public void SetDependencies(NPCTracker npcTracker) {
        this.tracker = npcTracker;
    }
}
