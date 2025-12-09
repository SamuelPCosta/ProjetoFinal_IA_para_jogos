using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine
{

    private IState currentState;
    public void changeState(IState newState)
    {
        if (currentState == newState)
            return;

        if (currentState != null)
            currentState.Exit();
        
        currentState = newState;
        currentState.Enter();
        
    }

    public void Update()
    {
        if (currentState != null)
            currentState.Update();
    }
}
