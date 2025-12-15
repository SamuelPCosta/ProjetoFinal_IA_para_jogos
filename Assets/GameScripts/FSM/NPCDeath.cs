using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NPCDeath : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCPatrol patrol;
    float timer;

    public NPCDeath(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;
    }

    public void Enter() {
        Debug.Log("Morto");
    }

    void IState.Update()
    {
        
    }

    public void Exit() { }

}
