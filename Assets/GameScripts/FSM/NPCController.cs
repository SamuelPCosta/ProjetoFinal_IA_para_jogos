using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{

    NPCStateMachine npcStateMachine;

    // Start is called before the first frame update
    void Start()
    {
        npcStateMachine = new NPCStateMachine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
