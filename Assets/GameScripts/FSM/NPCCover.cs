using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCCover : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCPatrol patrol;
    NPCTracker tracker;

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
        ComputeCenter();
    }

    void IState.Update()
    {
        if (controller.getTarget() != null)
        {
            machine.changeState(tracker);
        }
        if (controller.getNoise() != Vector3.zero)
        {
            machine.changeState(tracker);
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

            Vector3 lookDir = center - controller.transform.position;
            lookDir.y = 0;

            if (controller.getNPCIndex() == secondNPC_cover)
                lookDir = -lookDir;

            controller.transform.rotation =
                Quaternion.RotateTowards(
                    controller.transform.rotation,
                    Quaternion.LookRotation(lookDir),
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

        Vector3 targetPos = waypoints[index].position;

        if (controller.getNPCIndex() == secondNPC_cover)
        {
            GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
            GameObject targetNPC = null;
            float shortestDistance = Mathf.Infinity;

            foreach (GameObject npc in npcs)
            {
                if (npc == controller.gameObject) continue;

                float distance = Vector3.Distance(controller.transform.position, npc.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    targetNPC = npc;
                }
            }

            if (targetNPC != null)
            {
                targetPos = targetNPC.transform.position;

                //agent.updateRotation = false;
                agent.SetDestination(targetPos);

                Vector3 dir = controller.transform.position - targetPos;
                dir.y = 0;
                if (dir != Vector3.zero)
                    controller.transform.rotation = Quaternion.LookRotation(dir);

                return;
            }
        }

        float d = Vector3.Distance(waypoints[index].position, controller.transform.position);
        if (d <= minDistanceToPoint)
        {
            index = (index + 1) % waypoints.Count;
            wait = true;
            return;
        }

        agent.updateRotation = controller.getNPCIndex() != secondNPC_cover;
        agent.SetDestination(targetPos);

        if (controller.getNPCIndex() == secondNPC_cover)
        {
            Vector3 dir = controller.transform.position - targetPos;
            dir.y = 0;
            if (dir != Vector3.zero)
                controller.transform.rotation = Quaternion.LookRotation(dir);
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

    public void SetDependencies(NPCPatrol npcPatrol, NPCTracker npcTracker)
    {
        this.patrol = npcPatrol;
        this.tracker = npcTracker;
    }
}
