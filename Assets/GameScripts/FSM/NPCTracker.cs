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

    public void Enter() {}

    void IState.Update()
    {
        if (checkingNoise){
            if (!controller.agent.pathPending && controller.agent.remainingDistance <= controller.agent.stoppingDistance)
            {
                checkingNoise = false;
                controller.noiseChecked();
            }
        }

        if (controller.getTarget() == null && controller.getNoise() == Vector3.zero)
        {
            machine.changeState(patrol);
        }
        if (controller.getSeeingSmoke())
        {
            machine.changeState(disoriented);
        }

        if(controller.getTarget() != null)
            controller.agent.SetDestination(controller.getTarget().bounds.center);
        else if(controller.getNoise() != Vector3.zero) { 
            controller.agent.SetDestination(controller.getNoise());
            checkingNoise = true;
            Debug.Log("conferindo lugar com ruido");
        }
    }

    public void Exit() {
        checkingNoise = false;
        controller.noiseChecked();
    }

    public void SetDependencies(NPCPatrol patrol, NPCDisoriented disoriented)
    {
        this.patrol = patrol;
        this.disoriented = disoriented;
    }
}
