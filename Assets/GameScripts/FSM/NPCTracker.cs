using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NPCTracker : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCPatrol patrol;
    NPCDisoriented disoriented;

    private bool checkingNoise = false; 

    public NPCTracker(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;
    }

    public void Enter() {
        checkingNoise = false;
        if (controller.agent != null)
        {
            controller.agent.isStopped = false;
            controller.agent.updateRotation = true;

            controller.transform.rotation = controller.agent.transform.rotation;
        }
    }

    void IState.Update(){
        if (checkingNoise){
            if (controller.agent.hasPath && controller.agent.remainingDistance <= controller.agent.stoppingDistance){
                checkingNoise = false;
                controller.resetNoise();
            }
        }

        Vector3 dir = controller.agent.destination - controller.transform.position;
        dir.y = 0; // ignora altura
        float angleToTarget = Vector3.Angle(controller.transform.forward, dir);

        if (controller.getTarget() == null &&
            controller.getNoise() == Vector3.zero &&
            !controller.agent.pathPending &&
            controller.agent.remainingDistance <= controller.agent.stoppingDistance &&
            angleToTarget <= 5f) // tolerância de 5 graus
        {
            machine.changeState(patrol);
            return;
        }

        if (controller.agent.remainingDistance <= controller.agent.stoppingDistance &&
            controller.agent.velocity.magnitude < 0.05f)
        {
            machine.changeState(patrol);
            return;
        }

        if (controller.getSeeingSmoke()) { 
            machine.changeState(disoriented);
            controller.setSeeingSmoke();
        }

        if (controller.getTarget() != null) { 
            controller.agent.SetDestination(controller.getTarget().Value);
            return;
        }

        Vector3 noise = controller.getNoise();
        if (noise != Vector3.zero){
            //controller.resetNoise();
            checkingNoise = true;
            controller.agent.SetDestination(noise);
        }
        else
        {
            checkingNoise = false;
        }
    }

    public void Exit() {
        checkingNoise = false;
        //controller.resetNoise();
    }

    public void SetDependencies(NPCPatrol patrol, NPCDisoriented disoriented)
    {
        this.patrol = patrol;
        this.disoriented = disoriented;
    }
}
