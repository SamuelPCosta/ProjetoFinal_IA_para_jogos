using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGroupController : MonoBehaviour
{
    public GameObject routes;
    public GameObject routeCover;

    private List<GameObject> npcs;

    private NPCController doorNPC1 = null;
    private NPCController doorNPC2 = null;

    private Vector3 doorPosition2 = Vector3.zero;
    private DoorController door = null;
    private bool canOpen = true;

    // Start is called before the first frame update
    void Start()
    {
        setRoutes();
        StartCoroutine(InitChildrenStates());
    }

    public void setRoutes()
    {
        npcs = new List<GameObject>();

        int i = 0;
        foreach (Transform npc in transform)
        {
            npcs.Add(npc.gameObject);
            NPCController npcController = npc.GetComponent<NPCController>();

            if (i < routes.transform.childCount)
            {
                var route = routes.transform.GetChild(i).GetComponentsInChildren<Transform>();
                var list = new List<Transform>();
                for (int w = 1; w < route.Length; w++) //waypoint - index 1 skipa o pai da rota
                    list.Add(route[w]);
                npcController.waypoints = list;
            }
            i++;

            var newRoute = routeCover.transform.GetComponentsInChildren<Transform>();
            var newlist = new List<Transform>();
            for (int w = 1; w < newRoute.Length; w++) //waypoint - index 1 skipa o pai da rota
                newlist.Add(newRoute[w]);
            npcController.coverWaypoints = newlist;
        }
    }

    IEnumerator InitChildrenStates()
    {
        yield return null;
        foreach (Transform npc in transform)
        {
            NPCController controller = npc.GetComponent<NPCController>();
            controller.InitStates();
        }
    }

    // Update is called once per frame
    const int numberOfCover = 2; 
    void Update(){
        setIndexDynamically();
        if (getNumberOfNPCs() <= numberOfCover){
            foreach (Transform npc in transform)
                npc.GetComponent<NPCController>().setCover(true);
        }else
            foreach (Transform npc in transform)
                npc.GetComponent<NPCController>().setCover(false);


        if(doorNPC1 != null && doorNPC2 != null){
            checkDoor();
        }
    }

    private void setIndexDynamically(){
        int i = 0;
        foreach (Transform npc in transform){
            NPCController npcController = npc.GetComponent<NPCController>();
            npcController.setNPCIndex(i);
            i++;
        }
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

    public void setDoorNPC1(NPCController npc) => doorNPC1 = npc;
    public void setDoorNPC2(NPCController npc) => doorNPC2 = npc;

    public NPCController getDoorNPC1() => doorNPC1;
    public NPCController getDoorNPC2() => doorNPC2;

    public void setDoorPosition(Vector3 target) => doorPosition2 = target;
    public Vector3 getDoorPosition2() => doorPosition2;
    public void setDoor(DoorController door) => this.door = door;
    public DoorController getDoor() => door;

    private void checkDoor(){
        if(getDoorNPC1().getTarget() != null || getDoorNPC2().getTarget() != null)
        {
            setDoorNPC1(null);
            setDoorNPC2(null);
            return;
        }
        if(door != null){
            if ((!doorNPC1.agent.pathPending && doorNPC1.agent.remainingDistance <= doorNPC1.agent.stoppingDistance) &&
                !(!doorNPC2.agent.pathPending && doorNPC2.agent.remainingDistance <= doorNPC2.agent.stoppingDistance)){
                // So 1 chegou ao destino
                getDoorNPC1().setTriggerAnim("Idle");
            }
            else
            if ((!doorNPC1.agent.pathPending && doorNPC1.agent.remainingDistance <= doorNPC1.agent.stoppingDistance) &&
                (!doorNPC2.agent.pathPending && doorNPC2.agent.remainingDistance <= doorNPC2.agent.stoppingDistance) &&
                doorNPC1.getTarget() == null && doorNPC2.getTarget() == null)
            {
                // Chegaram ao destino
                getDoorNPC1().setTriggerAnim("Idle");
                getDoorNPC2().setTriggerAnim("Idle");
                if (canOpen) { 
                    Invoke(nameof(kickingDoor), .5f);
                    Invoke(nameof(UnlockDoorDelayed), 1f);
                    Invoke(nameof(ResetStateAfterDoor), 1.8f);
                    canOpen = false;
                }
            }
        }
    }

    private void kickingDoor(){
        Quaternion rot = Quaternion.LookRotation(-door.transform.right);

        StartCoroutine(SmoothRotate(doorNPC1.transform, rot));
        StartCoroutine(SmoothRotate(doorNPC2.transform, rot));

        getDoorNPC1().setAnimNow("Kicking");
        getDoorNPC2().setAnimNow("Kicking");
    }

    IEnumerator SmoothRotate(Transform t, Quaternion target){
        Quaternion start = t.rotation;
        float time = 0f;
        while (time < 0.5f){
            time += Time.deltaTime;
            t.rotation = Quaternion.Slerp(start, target, time / 0.5f);
            yield return null;
        }
        t.rotation = target;
    }

    private void UnlockDoorDelayed(){
        if (door != null){
            getDoor().UnlockDoor();
        }
    }

    private void ResetStateAfterDoor(){
        setDoorNPC1(null);
        setDoorNPC2(null);
        getDoorNPC1().setTriggerAnim("Idle");
        getDoorNPC2().setTriggerAnim("Idle");
        canOpen = true;
    }
}
