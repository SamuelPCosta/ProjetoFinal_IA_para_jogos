using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public GameObject Door;

    public void LockDoor(){
        if (!Door.activeSelf)
            Door.SetActive(true);
    }

    public void UnlockDoor(){
        if (Door.activeSelf) { 
            Door.SetActive(false);
            //TODO
            Destroy(Door);
        }
    }

}
