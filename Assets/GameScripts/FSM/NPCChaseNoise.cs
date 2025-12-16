using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NPCChaseNoise : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCPatrol patrol;
    NPCChase chase;
    NPCDisoriented disoriented;
    NPCDeath death;

    float delayTimer = 0f;
    bool waitingNoise = false;

    private bool checkingNoise = false; 

    public NPCChaseNoise(NPCController controller, NPCStateMachine machine)
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
        if (!controller.isAlive())
            machine.changeState(death);

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
            if (!waitingNoise){
                waitingNoise = true;
                delayTimer = 0f;
            }

            delayTimer += Time.deltaTime;
            if (delayTimer >= 1.5f){
                machine.changeState(patrol);
                waitingNoise = false;
            }
            return;
        }

        if (controller.getSeeingSmoke()) {
            controller.resetSeeingSmoke();
            machine.changeState(disoriented);
            return;
        }

        if (controller.getTarget() != null) {
            machine.changeState(chase);
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
    }

    public void SetDependencies(NPCPatrol patrol, NPCDisoriented disoriented, NPCChase chase, NPCDeath death)
    {
        this.patrol = patrol;
        this.disoriented = disoriented;
        this.chase = chase;
        this.death = death;
    }
}
