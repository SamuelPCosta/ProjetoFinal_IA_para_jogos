using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGroupController : MonoBehaviour
{
    public GameObject routes;
    public GameObject routeCover;

    private List<GameObject> npcs;

    // Start is called before the first frame update
    void Start()
    {
        npcs = new List<GameObject>();

        int i = 0;
        foreach (Transform npc in transform){
            npcs.Add(npc.gameObject);
            NPCController npcController = npc.GetComponent<NPCController>();

            if (i < routes.transform.childCount){
                var route = routes.transform.GetChild(i).GetComponentsInChildren<Transform>();
                var list = new List<Transform>();
                for (int w = 1; w < route.Length; w++) //waypoint - index 1 skipa o pai da rota
                    list.Add(route[w]);
                npcController.waypoints = list;
            }

            npcController.setNPCIndex(i);
            i++;

            var newRoute = routeCover.transform.GetComponentsInChildren<Transform>();
            var newlist = new List<Transform>();
            for (int w = 1; w < newRoute.Length; w++) //waypoint - index 1 skipa o pai da rota
                newlist.Add(newRoute[w]);
            npcController.coverWaypoints = newlist;
        }
    }

    // Update is called once per frame
    const int numberOfCover = 2; 
    void Update(){
        if(getNumberOfNPCs() == numberOfCover)
        {
            foreach (Transform npc in transform)
                npc.GetComponent<NPCController>().setCover(true);
        }else
            foreach (Transform npc in transform)
                npc.GetComponent<NPCController>().setCover(false);
    }

    public void groupCheckNoise(Vector3 collisionPoint)
    {
        print("Checando ruido");
        foreach (var npc in npcs)
            if(npc != null)
                npc.GetComponent<NPCController>().checkNoise(collisionPoint);
    }

    public int getNumberOfNPCs()
    {
        return transform.childCount;
    }
}
