using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NPCTracker : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCPatrol patrol;

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
    }

    public void Exit() { }

    public void SetDependencies(NPCPatrol patrol)
    {
        this.patrol = patrol;
    }
}
