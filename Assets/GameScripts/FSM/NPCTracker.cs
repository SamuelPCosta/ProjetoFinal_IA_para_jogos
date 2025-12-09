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

    public NPCTracker(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;
    }

    public void Enter() { }

    void IState.Update()
    {
        if (controller.getTarget() == null)
        {
            machine.changeState(patrol);
        }
        if (controller.getSeeingSmoke())
        {
            machine.changeState(disoriented);
        }
    }

    public void Exit() { }

    public void SetDependencies(NPCPatrol patrol, NPCDisoriented disoriented)
    {
        this.patrol = patrol;
        this.disoriented = disoriented;
    }
}
