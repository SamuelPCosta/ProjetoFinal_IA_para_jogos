using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BrokenObject : MonoBehaviour
{
    public GameObject entireObject;
    public GameObject brokenObject;

    [SerializeField] private LayerMask layerPlayer;
    [SerializeField] private LayerMask layerNPC;

    [SerializeField] private bool safeForPlayer;
    [SerializeField] private bool safeForNPC;

    [SerializeField] private float timeToBroke = 4f;

    [SerializeField] private Collider activeCollider;

    private bool hasBeenTriggered = false;
    private NavMeshObstacle obstacle;

    void Start()
    {
        obstacle = activeCollider.GetComponent<NavMeshObstacle>();

        if (obstacle != null)
            obstacle.enabled = false;

        entireObject.SetActive(true);
        brokenObject.SetActive(false);
        hasBeenTriggered = false;
    }


    private void Update()
    {
        if (hasBeenTriggered) return;

        Collider[] hits = Physics.OverlapBox(activeCollider.bounds.center, activeCollider.bounds.extents, activeCollider.transform.rotation);

        bool dangerousObjectDetected = false;

        foreach (var hit in hits)
        {
            int hitLayer = hit.gameObject.layer;

            bool isPlayer = ((1 << hitLayer) & layerPlayer) != 0;
            bool isNPC = ((1 << hitLayer) & layerNPC) != 0;

            if (isPlayer && !safeForPlayer)
            {
                dangerousObjectDetected = true;
                break;
            }
            else if (isNPC && !safeForNPC)
            {
                dangerousObjectDetected = true;
                break;
            }
        }

        if (dangerousObjectDetected)
        {
            Invoke("BreakObject", timeToBroke);
            hasBeenTriggered = true;
        }
    }

    private void BreakObject()
    {
        if (obstacle != null)
            obstacle.enabled = true;

        entireObject.SetActive(false);
        brokenObject.SetActive(true);
    }
}