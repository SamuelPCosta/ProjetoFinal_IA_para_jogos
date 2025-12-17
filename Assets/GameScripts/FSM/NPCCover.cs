using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCCover : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCPatrol patrol;
    NPCChase chase;
    NPCChaseNoise chaseNoise;
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
    //const int firstNPC_cover = 0;
    const int secondNPC_cover = 1;

    public NPCCover(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;

        waypoints = controller.coverWaypoints;

        agent = controller.agent;
    }

    public void Enter() {
        agent = controller.agent;

        if (agent == null)
        {
            Debug.LogError("Agent is null in NPCCover!");
            agent = controller.GetComponent<NavMeshAgent>();
        }

        wait = false;
        timer = 0f;
        index = 0;

        if (!agent.isActiveAndEnabled)
        {
            agent.enabled = true;
        }
        
        waypoints = controller.coverWaypoints;
        controller.setTriggerAnim("Walking");

        if (controller.getNPCIndex() == secondNPC_cover)
            controller.setTriggerAnim("WalkingBackwards");
        else
            controller.setTriggerAnim("Walking");
        ComputeCenter();
    }

    void IState.Update()
    {
        if (!controller.isAlive())
            machine.changeState(death);
        //if (groupController.getDoorNPC1() == controller) machine.changeState(unlockDoor); //NPC1
        //if (groupController.getDoorNPC1() != null && groupController.getDoorNPC2() == null)
        //{ //NPC1 TA PRECISANDO DE NPC2
        //    if (groupController.getDoorNPC1() != controller)
        //    {
        //        groupController.setDoorNPC2(controller); //SE VOLUNTARIA A AJUDAR
        //        machine.changeState(unlockDoor);
        //    }
        //}

        if (controller.getTarget() != null)
        {
            machine.changeState(chase);
        }
        if (controller.getNoise() != Vector3.zero)
        {
            machine.changeState(chaseNoise);
        }
        if (!controller.getCover())
            machine.changeState(patrol);
        Patrol();
    }

    void Patrol()
    {
        if (wait)
        {
            timer += Time.deltaTime;
            controller.setTriggerAnim("Idle");

            Vector3 lookDir = center - controller.transform.position;
            lookDir.y = 0;

            if (controller.getNPCIndex() == secondNPC_cover)
                lookDir = -lookDir;

            if (lookDir != Vector3.zero)
                controller.transform.rotation = Quaternion.RotateTowards(
                    controller.transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 180f
                );

            controller.setCenterpoint(center);

            if (timer >= controller.TimePatrol)
            {
                wait = false;
                timer = 0f;
                agent.SetDestination(waypoints[index].position);
            }
            return;
        }

        if (waypoints.Count == 0) return;

        Vector3 targetPos = waypoints[index].position;
        bool isSecond = controller.getNPCIndex() == secondNPC_cover;

        if (isSecond)
        {
            GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
            GameObject targetNPC = null;
            float shortest = Mathf.Infinity;

            foreach (GameObject npc in npcs)
            {
                if (npc == controller.gameObject) continue;
                NPCController c = npc.GetComponent<NPCController>();
                if (!c || !c.isAlive()) continue;

                float dist = Vector3.Distance(controller.transform.position, npc.transform.position);
                if (dist < shortest)
                {
                    shortest = dist;
                    targetNPC = npc;
                }
            }

            if (targetNPC != null)
            {
                targetPos = targetNPC.transform.position;
                agent.updateRotation = false;
                agent.SetDestination(targetPos);

                Vector3 dir = controller.transform.position - targetPos;
                dir.y = 0;
                if (dir != Vector3.zero)
                    controller.transform.rotation = Quaternion.LookRotation(dir);

                if (agent.velocity.sqrMagnitude < 0.01f)
                    controller.setTriggerAnim("Idle");
                else
                    controller.setTriggerAnim("WalkingBackwards");

                if (Vector3.Distance(controller.transform.position, targetPos) <= agent.stoppingDistance)
                    agent.ResetPath();

                return;
            }
        }

        agent.updateRotation = true;
        agent.SetDestination(targetPos);

        if (agent.velocity.sqrMagnitude < 0.01f)
            controller.setTriggerAnim("Idle");
        else
            controller.setTriggerAnim("Walking");

        if (Vector3.Distance(controller.transform.position, targetPos) <= minDistanceToPoint)
        {
            index = (index + 1) % waypoints.Count;
            wait = true;
        }
    }

    public void Exit() {
        controller.setCenterpoint(Vector3.zero);
        agent.updateRotation = true;
        agent.ResetPath();
        agent.isStopped = false;
        agent.velocity = Vector3.zero;
        controller.transform.rotation = agent.transform.rotation;
    }

    void ComputeCenter()
    {
        if (waypoints.Count == 0) return;
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < waypoints.Count; i++) sum += waypoints[i].position;
        center = sum / waypoints.Count;
    }

    public void SetDependencies(NPCPatrol patrol, NPCChase chase, NPCUnlockDoor unlockDoor, NPCChaseNoise chaseNoise, NPCDeath death, NPCGroupController groupController)
    {
        this.patrol = patrol;
        this.chase = chase;
        this.unlockDoor = unlockDoor;
        this.groupController = groupController;
        this.chaseNoise = chaseNoise;
        this.death = death;
    }
}
