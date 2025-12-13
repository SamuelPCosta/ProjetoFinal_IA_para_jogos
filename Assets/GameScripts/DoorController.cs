using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public GameObject Door;

    public void LockDoor(){
        if(Door!=null)
            if (!Door.activeSelf)
                Door.SetActive(true);
    }

    public void UnlockDoor(){
        if (Door != null && Door.activeSelf) { 
            Door.SetActive(false);
            //TODO
            Destroy(Door);
        }
    }

    public Vector3 getFirstPoint()
    {
        return Door.transform.GetChild(0).position;
    }

    public Vector3 getSecondPoint()
    {
        return Door.transform.GetChild(1).position;
    }
}
