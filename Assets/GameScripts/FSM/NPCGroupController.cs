using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGroupController : MonoBehaviour
{
    private List<GameObject> npcs;

    // Start is called before the first frame update
    void Start()
    {
        npcs = new List<GameObject>();
        foreach (Transform t in transform) npcs.Add(t.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void groupCheckNoise(Vector3 collisionPoint)
    {
        print("Checando ruido");
        foreach (var npc in npcs)
            npc.GetComponent<NPCController>().checkNoise(collisionPoint);
    }
}
