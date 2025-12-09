using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NPCCotroller : MonoBehaviour
{
    [SerializeField] private float range = 6f;
    [SerializeField] private float angle = 60f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private int segments = 6;
    [SerializeField] private float angularSpeed = 900f;
    [SerializeField] private float hearingRange = 3f;
    [SerializeField] private LayerMask obstacleMask;
    public StarterAssets.StarterAssetsInputs PlayerInputs;

    private Mesh viewMesh;
    private Vector3 lastTargetPosition;

    private NavMeshAgent agent;

    void Start()
    {
        // inicializa o agente
        agent = GetComponent<NavMeshAgent>();
        //agent.angularSpeed = angularSpeed;
        //agent.acceleration = 999f;
        agent.updateRotation = true;

        if (agent == null)
        {
            Debug.LogError("Componente NavMeshAgent ausente no NPC!");
        }
    }

    private void HandleRotation()
    {
        if (lastTargetPosition != Vector3.zero)
        {
            Vector3 direction = (lastTargetPosition - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    lookRotation,
                    Time.deltaTime * angularSpeed
                );
            }
        }
    }

    void Update()
    {
        DrawFieldOfViewBorders(false, Color.red);

        if (DetectPlayerByVision() || DetectPlayerBySound())
        {
            Debug.Log("JOGADOR DETECTADO! Alerta!");
        }
        HandleRotation();
    }

    void OnDrawGizmos()
    {
        DrawFieldOfViewFilled(Color.red);
        DrawFieldOfViewBorders(true, Color.red);
        DrawHearingZone(Color.blue);
    }

    private bool DetectPlayerByVision()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Collider[] hits = Physics.OverlapSphere(origin, range);

        foreach (Collider collider in hits)
        {
            if (collider.CompareTag("Player"))
            {
                Vector3 targetPos = collider.bounds.center;
                Vector3 dir = (targetPos - origin).normalized;
                float dist = Vector3.Distance(origin, targetPos);

                if (Vector3.Dot(transform.forward, dir) >= Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad))
                {
                    if (!Physics.Raycast(origin, dir, dist, obstacleMask))
                    {
                        Debug.DrawLine(origin, targetPos, Color.blue);
                        if (agent != null)
                            agent.SetDestination(targetPos);
                        return true;
                    }
                }
            }
        }

        if (agent != null && agent.enabled && agent.hasPath)
        {
            // 
            //agent.isStopped = true; 
        }

        return false;
    }

    private bool DetectPlayerBySound()
    {
        Vector3 centerPosition = transform.position;
        Collider[] detectedColliders = Physics.OverlapSphere(centerPosition, hearingRange);

        foreach (Collider detectedCollider in detectedColliders){
            if (detectedCollider.CompareTag("Player")){
                StarterAssets.StarterAssetsInputs playerInputs = detectedCollider.GetComponent<StarterAssets.StarterAssetsInputs>();
                if (playerInputs != null && 
                   !playerInputs.crouch && playerInputs.move != Vector2.zero) {
                    Vector3 targetPos = detectedCollider.bounds.center;
                    if (agent != null)
                        agent.SetDestination(targetPos);
                    return true;
                }
            }
        }

        return false;
    }

    #region draw
    private void DrawFieldOfViewFilled(Color color)
    {
        if (viewMesh == null)
        {
            viewMesh = new Mesh();
        }

        CreateViewMesh(viewMesh);

        Gizmos.color = color;
        Gizmos.DrawMesh(viewMesh, transform.position + Vector3.up * eyeHeight, transform.rotation);
    }

    private void DrawFieldOfViewBorders(bool isGizmos, Color color)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 forwardBase = transform.forward * range;
        Vector3 lastPoint = origin + Quaternion.Euler(0, -angle * 0.5f, 0) * forwardBase;

        if (isGizmos)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(origin, lastPoint);
        }
        else
        {
            Debug.DrawLine(origin, lastPoint, color);
        }

        for (int i = 1; i <= segments; i++)
        {
            float segmentAngle = -angle * 0.5f + (angle / segments) * i;
            Vector3 currentPoint = origin + Quaternion.Euler(0, segmentAngle, 0) * forwardBase;

            if (isGizmos)
            {
                Gizmos.DrawLine(lastPoint, currentPoint);
            }
            else
            {
                Debug.DrawLine(lastPoint, currentPoint, color);
            }

            lastPoint = currentPoint;

            if (i == segments)
            {
                if (isGizmos)
                {
                    Gizmos.DrawLine(origin, currentPoint);
                }
                else
                {
                    Debug.DrawLine(origin, currentPoint, color);
                }
            }
        }
    }

    private void DrawHearingZone(Color color)
    {
        Gizmos.color = color;
        int seg = 32;
        Vector3 c = transform.position;
        Vector3 last = c + new Vector3(hearingRange, 0, 0);
        for (int i = 1; i <= seg; i++)
        {
            float a = i * Mathf.PI * 2f / seg;
            Vector3 cur = c + new Vector3(Mathf.Cos(a) * hearingRange, 0, Mathf.Sin(a) * hearingRange);
            Gizmos.DrawLine(last, cur);
            last = cur;
        }
    }

    private void CreateViewMesh(Mesh mesh)
    {
        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        Vector3 forward = Vector3.forward * range;

        for (int i = 0; i <= segments; i++)
        {
            float segmentAngle = -angle * 0.5f + (angle / segments) * i;
            Quaternion rotation = Quaternion.Euler(0, segmentAngle, 0);

            vertices[i + 1] = rotation * forward;
        }

        int vertexIndex = 1;
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = vertexIndex;
            triangles[i * 3 + 2] = vertexIndex + 1;

            vertexIndex++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    #endregion
}