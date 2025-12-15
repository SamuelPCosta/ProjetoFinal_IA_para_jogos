using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NPCChase : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCPatrol patrol;
    NPCDisoriented disoriented;

    private bool checkingNoise = false; 

    public NPCChase(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;
    }

    public void Enter() {
        controller.PlayAudio();
        checkingNoise = false;
        if (controller.agent != null)
        {
            controller.agent.isStopped = false;
            controller.agent.updateRotation = true;

            controller.transform.rotation = controller.agent.transform.rotation;
        }
        controller.setTriggerAnim("Walking");
    }

    void IState.Update(){
        if (!controller.getStatus())
            return;

        if (controller.agent.remainingDistance <= controller.agent.stoppingDistance)
            controller.setTriggerAnim("Idle");
        else
            controller.setTriggerAnim("Walking");

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
        controller.agent.remainingDistance <= controller.agent.stoppingDistance)
        {
            machine.changeState(patrol);
            return;
        }

        if (controller.getSeeingSmoke()) { 
            machine.changeState(disoriented);
            controller.resetSeeingSmoke();
        }

        if (controller.getTarget() != null) { 
            controller.agent.SetDestination(controller.getTarget().Value);
            if (controller.getDistance() <= controller.getMinDistanceToAttack())
            {
                //TODO
                Debug.Log("Atacando");
                controller.setTriggerAnim("Attacking");
                controller.attack();
            }
            return;
        }

        Vector3 noise = controller.getNoise();
        if (noise != Vector3.zero){
            checkingNoise = true;
            controller.agent.SetDestination(noise);
            controller.setTriggerAnim("Walking");
        }
        else
            checkingNoise = false;
    }

    public void Exit() {
        checkingNoise = false;
        //controller.resetNoise();
    }

    public void SetDependencies(NPCPatrol patrol, NPCDisoriented disoriented){
        this.patrol = patrol;
        this.disoriented = disoriented;
    }
}
