using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCChase chase;
    NPCCover cover;
    NPCUnlockDoor unlockDoor;
    NPCGroupController groupController;

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
        //waypoints = controller.waypoints;
    }

    public void Enter() {
        wait = false;
        timer = 0f;

        waypoints = controller.waypoints;
        ComputeCenter();
        controller.setTriggerAnim("Idle");
    }

    void IState.Update(){
        if (!controller.getStatus())
            return;

        if (controller.getTarget() != null) machine.changeState(chase);
        if (controller.getNoise() != Vector3.zero) machine.changeState(chase);

        if (groupController.getDoorNPC1() == controller) machine.changeState(unlockDoor); //NPC1
        if (groupController.getDoorNPC1() != null && groupController.getDoorNPC2() == null){ //NPC1 TA PRECISANDO DE NPC2
            if (groupController.getDoorNPC1() != controller){
                groupController.setDoorNPC2(controller); //SE VOLUNTARIA A AJUDAR
                machine.changeState(unlockDoor);
            }
        }
        if (controller.getCover()) machine.changeState(cover);

        Patrol();
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
            controller.setTriggerAnim("Idle");
            return;
        }

        agent.updateRotation = true;

        if (waypoints.Count == 0) return;

        float d = Vector3.Distance(waypoints[index].position, controller.transform.position);
        if (d <= minDistanceToPoint){
            index = (index + 1) % waypoints.Count;
            wait = true;
            return;
        }

        agent.SetDestination(waypoints[index].position);
        controller.setTriggerAnim("Walking");
    }

    void ComputeCenter()
    {
        if (waypoints.Count == 0) return;
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < waypoints.Count; i++) sum += waypoints[i].position;
        center = sum / waypoints.Count;
    }

    public void SetDependencies(NPCChase npcChase, NPCCover npcCover, NPCUnlockDoor npcUnlockDoor, NPCGroupController npcGroupController) {
        this.chase = npcChase;
        this.cover = npcCover;
        this.unlockDoor = npcUnlockDoor;
        this.groupController = npcGroupController;
    }
}
