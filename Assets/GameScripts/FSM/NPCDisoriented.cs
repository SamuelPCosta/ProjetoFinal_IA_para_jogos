using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NPCDisoriented : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCPatrol patrol;
    NPCDeath death;
    float timer;

    public NPCDisoriented(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;
    }

    public void Enter() {
        controller.agent.ResetPath();
        controller.setDisoriented(true);
        timer = controller.TimeDesoriented;
        controller.setTriggerAnim("Dizzy");
    }

    void IState.Update()
    {
        if (!controller.isAlive()){
            machine.changeState(death);
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f) machine.changeState(patrol);
    }

    public void Exit() {
        controller.setDisoriented(false);
    }

    public void SetDependencies(NPCPatrol patrol, NPCDeath death)
    {
        this.patrol = patrol;
        this.death = death;
    }
}
