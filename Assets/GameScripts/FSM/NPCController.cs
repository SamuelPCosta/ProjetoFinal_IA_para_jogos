using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("Cone of vision")]
    [SerializeField] private float range = 6f;
    [SerializeField] private float angle = 80f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private int segments = 6;

    [Space(10)]
    [Header("Downward cone of vision")]
    [SerializeField] private float secondaryRange = 10f;
    [SerializeField] private float secondaryAngle = 40f;
    [SerializeField] private float secondaryPitch = -45f;

    [Space(10)]
    [Header("Hearing")]
    [SerializeField] private float hearingRange = 2f;

    [Space(10)]
    [SerializeField] private float hearingRangeProjectile = 4f;

    [Space(10)]
    [SerializeField] private float angularSpeed = 300f;

    [Header("LayerMasks")]
    [Space(10)]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask smokeMask;
    public StarterAssets.StarterAssetsInputs PlayerInputs;

    [Space(10)]
    public NavMeshAgent agent;

    [Space(10)]
    [Header("Patrol")]
    public float TimePatrol = 4f;
    public List<Transform> waypoints;
    public List<Transform> coverWaypoints;

    [Space(10)]
    public float TimeDesoriented = 2.5f;

    private Collider target = null;
    private bool seeingSmoke = false;
    private bool isDisoriented = false;

    private Mesh viewMesh;
    private Mesh secondaryMesh;
    private Vector3 lastTargetPosition;

    private Vector3 centerpoint = Vector3.zero;

    private Vector3 noiseSource = Vector3.zero;

    private bool cover = false;

    private int index = -1;

    //##############################
    NPCStateMachine npcStateMachine;
    NPCPatrol npcPatrol;
    NPCTracker npcTracker;
    NPCDisoriented npcDisoriented;
    NPCCover npcCover;

    // Start is called before the first frame update
    void Start()
    {
        // DEBUG E ATRIBUICOES
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;

        if (agent == null)
        {
            Debug.LogError("Componente NavMeshAgent ausente no NPC!");
        }
    }

    public void InitStates()
    {
        npcStateMachine = new NPCStateMachine();
        npcPatrol = new NPCPatrol(this, npcStateMachine);
        npcTracker = new NPCTracker(this, npcStateMachine);
        npcDisoriented = new NPCDisoriented(this, npcStateMachine);
        npcCover = new NPCCover(this, npcStateMachine);

        npcPatrol.SetDependencies(npcTracker, npcCover);
        npcTracker.SetDependencies(npcPatrol, npcDisoriented);
        npcDisoriented.SetDependencies(npcPatrol);
        npcCover.SetDependencies(npcPatrol, npcTracker);

        npcStateMachine.changeState(npcPatrol);
    }

    // Update is called once per frame
    void Update()
    {
        if (npcStateMachine != null)
            npcStateMachine.Update();

        if (!isDisoriented) {
            if (DetectPlayerByVision() || DetectPlayerByVisionAbove() || DetectPlayerBySound())
                Debug.Log("JOGADOR DETECTADO!");
            else
                target = null;
        }
        else
            target = null;

        HandleRotation();
    }

    public Collider getTarget() => target;

    public bool getSeeingSmoke() => seeingSmoke;
    public bool setSeeingSmoke() => seeingSmoke = false;

    public void setCenterpoint(Vector3 point) => centerpoint = point;

    public void setDisoriented(bool state) => isDisoriented = state;


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

    #region detectPlayer
    private bool DetectPlayerByVision()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Collider[] hits = Physics.OverlapSphere(origin, range);

        RaycastHit hit;

        foreach (Collider collider in hits){
            if (collider.CompareTag("Player")){
                Vector3 targetPos = collider.bounds.center;
                Vector3 dir = (targetPos - origin).normalized;
                float dist = Vector3.Distance(origin, targetPos);

                if (Vector3.Dot(transform.forward, dir) >= Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad)){
                    if (Physics.Raycast(origin, dir, out hit, dist, obstacleMask, QueryTriggerInteraction.Collide)){
                        if (((1 << hit.collider.gameObject.layer) & smokeMask) != 0)
                            seeingSmoke = true; // Visao bloqueada pela FUMACA
                        else
                            setSeeingSmoke();
                    }
                    else{
                        Debug.DrawLine(origin, targetPos, Color.blue);
                        target = collider;
                        setSeeingSmoke();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool DetectPlayerByVisionAbove()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Collider[] hits = Physics.OverlapSphere(origin, secondaryRange);

        Vector3 secondaryForward = (transform.rotation * Quaternion.Euler(-secondaryPitch, 0f, 0f)) * Vector3.forward;
        RaycastHit hit;

        foreach (Collider collider in hits)
        {
            if (!collider.CompareTag("Player")) continue;

            Vector3 targetPos = collider.bounds.center;
            Vector3 dir = (targetPos - origin).normalized;
            float dist = Vector3.Distance(origin, targetPos);

            float angleToPlayer = Vector3.Angle(secondaryForward, dir);
            if (angleToPlayer <= secondaryAngle * 0.5f)
            {
                if (Physics.Raycast(origin, dir, out hit, dist, obstacleMask, QueryTriggerInteraction.Collide))
                {
                    if (((1 << hit.collider.gameObject.layer) & smokeMask) != 0)
                        seeingSmoke = true; // Visao bloqueada pela FUMACA
                    else
                        setSeeingSmoke();
                }
                else { 
                    Debug.DrawLine(origin, targetPos, Color.blue);
                    target = collider;
                    setSeeingSmoke();
                    return true;
                }
            }
        }
        return false;
    }

    private bool DetectPlayerBySound()
    {
        Vector3 centerPosition = transform.position;
        Collider[] detectedColliders = Physics.OverlapSphere(centerPosition, hearingRange);

        foreach (Collider detectedCollider in detectedColliders)
        {
            if (detectedCollider.CompareTag("Player"))
            {
                StarterAssets.StarterAssetsInputs playerInputs = detectedCollider.GetComponent<StarterAssets.StarterAssetsInputs>();
                if (playerInputs != null &&
                   !playerInputs.crouch && playerInputs.move != Vector2.zero)
                {
                    target = detectedCollider;
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    public void checkNoise(Vector3 collisionPoint)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(collisionPoint, out hit, 2f, agent.areaMask))
            collisionPoint = hit.position;

        Vector3 a = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 b = new Vector3(collisionPoint.x, 0, collisionPoint.z);
        float d = Vector3.Distance(a, b);
        if (d <= hearingRangeProjectile)
            noiseSource = collisionPoint;
    }

    public Vector3 getNoise() => noiseSource;

    public void resetNoise() => noiseSource = Vector3.zero;

    public void setCover(bool state) => cover = state;

    public bool getCover() => cover;

    public void setNPCIndex(int i) => index = i;

    public int getNPCIndex() => index;

    void OnDrawGizmos()
    {
        DrawFieldOfViewFilled(Color.red);
        DrawFieldOfViewBorders(true, Color.red);
        DrawHearingZone(Color.blue);
        DrawHearingZoneProjectile(Color.blue);
        DrawSecondaryCone(Color.red);
        if (centerpoint != Vector3.zero)
            DrawCenterPatrol();
    }

    #region drawDebug
    private void DrawFieldOfViewFilled(Color color)
    {
        if (viewMesh == null)
        {
            viewMesh = new Mesh();
        }

        CreateViewMesh(viewMesh);

        //Gizmos.color = color;
        Gizmos.color = color * 0.5f;
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

    private void DrawSecondaryCone(Color color)
    {
        if (secondaryMesh == null) secondaryMesh = new Mesh();

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Quaternion pitchRotation = transform.rotation * Quaternion.Euler(-secondaryPitch, 0f, 0f);

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float segmentAngle = -secondaryAngle * 0.5f + (secondaryAngle / segments) * i;
            Vector3 point = Quaternion.Euler(0f, segmentAngle, 0f) * Vector3.forward * secondaryRange;
            vertices[i + 1] = pitchRotation * point;
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        secondaryMesh.Clear();
        secondaryMesh.vertices = vertices;
        secondaryMesh.triangles = triangles;
        secondaryMesh.RecalculateNormals();

        Gizmos.color = color * 0.5f;
        Gizmos.DrawMesh(secondaryMesh, origin, Quaternion.identity);

        Gizmos.color = color;
        Vector3 lastPoint = origin + vertices[1];
        for (int i = 1; i <= segments; i++)
        {
            int nextIndex = i + 1 > segments ? 1 : i + 1;
            Vector3 currentPoint = origin + vertices[nextIndex];
            Gizmos.DrawLine(lastPoint, currentPoint);
            Gizmos.DrawLine(origin, origin + vertices[i]);
            lastPoint = currentPoint;
        }
    }

    private void DrawHearingZone(Color color)
    {
        Gizmos.color = color;
        int seg = 32;
        Vector3 c = transform.position;
        Vector3 last = c + new Vector3(hearingRange, 1f, 0);
        for (int i = 1; i <= seg; i++)
        {
            float a = i * Mathf.PI * 2f / seg;
            Vector3 cur = c + new Vector3(Mathf.Cos(a) * hearingRange, 1f, Mathf.Sin(a) * hearingRange);
            Gizmos.DrawLine(last, cur);
            last = cur;
        }
    }

    private void DrawHearingZoneProjectile(Color color)
    {
        Gizmos.color = color * 0.5f;
        int seg = 32;
        Vector3 c = transform.position;
        Vector3 last = c + new Vector3(hearingRangeProjectile, 1f, 0);
        for (int i = 1; i <= seg; i++)
        {
            float a = i * Mathf.PI * 2f / seg;
            Vector3 cur = c + new Vector3(Mathf.Cos(a) * hearingRangeProjectile, 1f, Mathf.Sin(a) * hearingRangeProjectile);
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


    private void DrawCenterPatrol()
    {
        float s = 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(centerpoint + Vector3.right * s, centerpoint - Vector3.right * s);
        Gizmos.DrawLine(centerpoint + Vector3.forward * s, centerpoint - Vector3.forward * s);
    }
    #endregion
}
