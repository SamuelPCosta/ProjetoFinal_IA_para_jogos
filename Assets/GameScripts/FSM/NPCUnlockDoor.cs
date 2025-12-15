using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCUnlockDoor : IState
{
    NPCController controller;
    NPCStateMachine machine;
    NPCTracker tracker;
    NPCCover cover;
    NPCPatrol patrol;
    NPCGroupController groupController;
    NavMeshAgent agent;
    int index;

    Vector3 center = Vector3.zero;

    public NPCUnlockDoor(NPCController controller, NPCStateMachine machine)
    {
        this.controller = controller;
        this.machine = machine;

        agent = controller.agent;
    }

    public void Enter() { }

    void IState.Update(){
        if (controller.getTarget() != null)
        {
            groupController.setDoorNPC1(null);
            groupController.setDoorNPC2(null);
            machine.changeState(tracker);
        }
        if (controller.getNoise() != Vector3.zero)
        {
            groupController.setDoorNPC1(null);
            groupController.setDoorNPC2(null);
            machine.changeState(tracker);
        }

        //Checar porta e parceiro
        if(groupController.getDoorNPC1() == controller){
            //Ir para ponto 1
            if(controller.getTargetDoor() != null)
                controller.agent.SetDestination(controller.getTargetDoor().Value);

        }else if(groupController.getDoorNPC2() == controller){
            //Ir para ponto 2
            controller.agent.SetDestination(groupController.getDoorPosition2());
            controller.setTriggerAnim("Walking");
        }
        if(groupController.getDoorNPC1() == null && groupController.getDoorNPC2() == null)
            machine.changeState(patrol);

        if (controller.getCover()) machine.changeState(cover);
    }

    public void Exit() {
        groupController.setDoor(null);
    }

    public void SetDependencies(NPCPatrol npcPatrol, NPCTracker npcTracker, NPCGroupController npcGroupController) {
        this.patrol = npcPatrol;
        this.tracker = npcTracker;
        this.groupController = npcGroupController;
        //this.cover = npcCover;
    }
}
