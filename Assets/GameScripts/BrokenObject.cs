using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenObject : MonoBehaviour
{
    public GameObject entireObject;
    public GameObject brokenObject;

    [Header("Detection Settings")]
    [SerializeField] private LayerMask layerDetection;
    [SerializeField] private float timeToBroke = 4f;

    private bool hasBeenTriggered = false;

    void Start()
    {
        entireObject.SetActive(true);
        brokenObject.SetActive(false);
        hasBeenTriggered = false;
    }

    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & layerDetection) != 0)
        {
            if (!hasBeenTriggered)
            {
                Invoke("BreakObject", timeToBroke);
                hasBeenTriggered = true;
            }
        }
    }

    private void BreakObject()
    {
        entireObject.SetActive(false);
        brokenObject.SetActive(true);
    }
}