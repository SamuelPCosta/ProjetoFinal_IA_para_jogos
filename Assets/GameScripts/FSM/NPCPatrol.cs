using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCChase chase;
    NPCChaseNoise chaseNoise;
    NPCCover cover;
    NPCUnlockDoor unlockDoor;
    NPCGroupController groupController;
    NPCDeath death;

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

        waypoints = controller.waypoints;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.SetDestination(waypoints[index].position);
        }
    }

    void IState.Update(){
        if (!controller.isAlive())
            machine.changeState(death);

        if (controller.getTarget() != null) machine.changeState(chase);
        if (controller.getNoise() != Vector3.zero) machine.changeState(chaseNoise);

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

        if (!agent.hasPath && !wait) {
            agent.SetDestination(waypoints[index].position);
        }

        if (wait){
            timer += Time.deltaTime;

            controller.transform.rotation =
                Quaternion.RotateTowards(
                    controller.transform.rotation,
                    Quaternion.LookRotation(new Vector3(center.x - controller.transform.position.x, 0f, center.z - controller.transform.position.z)),
                    Time.deltaTime * 180f
                );

            controller.setCenterpoint(center);

            if (timer >= controller.TimePatrol){
                wait = false;
                timer = 0f;
                agent.SetDestination(waypoints[index].position);
                controller.setTriggerAnim("Walking");
            } else {
                controller.setTriggerAnim("Idle");
            }
            return;
        }

        float d = Vector3.Distance(waypoints[index].position, controller.transform.position);
        if (d <= minDistanceToPoint){
            index = (index + 1) % waypoints.Count;
            wait = true;
            return;
        }

        if (agent.velocity.magnitude > 0.1f)
            controller.setTriggerAnim("Walking");
        else if (agent.velocity.magnitude <= 0.1f )
            controller.setTriggerAnim("Idle");
    }

    void ComputeCenter()
    {
        if (waypoints.Count == 0) return;
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < waypoints.Count; i++) sum += waypoints[i].position;
        center = sum / waypoints.Count;
    }

    public void SetDependencies(NPCChase chase, NPCCover cover, NPCUnlockDoor unlockDoor, NPCChaseNoise chaseNoise, NPCDeath death, NPCGroupController groupController){
        this.chase = chase;
        this.cover = cover;
        this.unlockDoor = unlockDoor;
        this.groupController = groupController;
        this.chaseNoise = chaseNoise;
        this.death = death;
    }

}
