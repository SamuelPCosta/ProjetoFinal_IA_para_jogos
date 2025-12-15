using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    private bool alive = true;
    [Header("Cone of vision")]
    [SerializeField] private float range = 6f;
    [SerializeField] private float angle = 80f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private int segments = 6;

    [Space(10)]
    [Header("Hearing")]
    [SerializeField] private float hearingRange = 2f;

    [Space(10)]
    [SerializeField] private float hearingRangeProjectile = 4f;

    [Space(10)]
    [SerializeField] private float angularSpeed = 300f;

    [Space(10)]
    [SerializeField] private float distanceToAttack = 1.1f;
    [SerializeField] private float timeBetweenAttacks = 1.2f;

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

    private Vector3? target = null;
    private Vector3? targetDoor = null;
    private bool seeingSmoke = false;
    private bool isDisoriented = false;

    private Mesh viewMesh;
    private Vector3 lastTargetPosition;

    private Vector3 centerpoint = Vector3.zero;

    private Vector3 noiseSource = Vector3.zero;

    private bool cover = false;

    private int index = -1;

    private float distanceToPlayer = float.MaxValue;

    private StarterAssets.StarterAssetsInputs playerInputs = null;

    private Animator animator = null;

    [SerializeField] float defaultAnimSpeed = 1.4f;

    //##############################
    NPCStateMachine npcStateMachine;
    NPCPatrol npcPatrol;
    NPCChase npcChase;
    NPCChaseNoise npcChaseNoise;
    NPCDisoriented npcDisoriented;
    NPCCover npcCover;
    NPCUnlockDoor npcUnlockDoor;
    NPCDeath npcDeath;

    // Start is called before the first frame update
    void Start()
    {
        // DEBUG E ATRIBUICOES
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        agent.avoidancePriority = Random.Range(20, 50);
        //agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        if (agent == null)
        {
            Debug.LogError("Componente NavMeshAgent ausente no NPC!");
        }

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void InitStates()
    {
        npcStateMachine = new NPCStateMachine();
        npcPatrol = new NPCPatrol(this, npcStateMachine);
        npcChase = new NPCChase(this, npcStateMachine);
        npcChaseNoise = new NPCChaseNoise(this, npcStateMachine);
        npcDisoriented = new NPCDisoriented(this, npcStateMachine);
        npcCover = new NPCCover(this, npcStateMachine);
        npcUnlockDoor = new NPCUnlockDoor(this, npcStateMachine);
        npcDeath = new NPCDeath(this, npcStateMachine);

        npcPatrol.SetDependencies(npcChase, npcCover, npcUnlockDoor, npcChaseNoise, npcDeath,transform.parent.GetComponent<NPCGroupController>());
        npcChase.SetDependencies(npcPatrol, npcDisoriented);
        npcChaseNoise.SetDependencies(npcPatrol, npcDisoriented, npcChase, npcDeath);
        npcDisoriented.SetDependencies(npcPatrol, npcDeath);
        npcCover.SetDependencies(npcPatrol, npcChase, npcUnlockDoor, npcChaseNoise, npcDeath, transform.parent.GetComponent<NPCGroupController>());
        npcUnlockDoor.SetDependencies(npcPatrol, npcChase, npcDeath, transform.parent.GetComponent<NPCGroupController>());

        npcStateMachine.changeState(npcPatrol);
    }

    // Update is called once per frame
    void Update(){
        if (!isAlive())
            return;
        if (playerInputs == null) playerInputs = FindObjectOfType<StarterAssets.StarterAssetsInputs>();
        if (playerInputs != null)
            distanceToPlayer = Vector3.Distance(playerInputs.transform.position, this.transform.position);

        if (npcStateMachine != null)
            npcStateMachine.Update();

        if (!isDisoriented) {
            if (DetectPlayerByVision() || DetectPlayerBySound())
                Debug.Log("JOGADOR DETECTADO!");
            else { 
                //checar portas trancadas
                target = null;
                if (DetectLockedDoor()){
                    NPCGroupController groupController = transform.parent.GetComponent<NPCGroupController>();
                    if (groupController.getDoorNPC1() == null)
                        groupController.setDoorNPC1(this);
                    else
                    if (groupController.getDoorNPC2() == null)
                        groupController.setDoorNPC2(this);
                }
            }
        }
        else
            target = null;

        HandleRotation();
        //UpdateWalkAnimSpeed();
        }

    void UpdateWalkAnimSpeed(){
        Animator anim = GetComponent<Animator>();
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Walking"))
        {
            float factor = agent.velocity.magnitude / agent.speed;
            anim.speed = Mathf.Max(0.1f, factor);
        }
        else
        {
            anim.speed = defaultAnimSpeed;
        }
    }

    public Vector3? getTarget() => target;
    public Vector3? getTargetDoor() => targetDoor;
    public bool getSeeingSmoke() => seeingSmoke;
    public bool resetSeeingSmoke() => seeingSmoke = false;
    public float getDistance() => distanceToPlayer;
    public float getMinDistanceToAttack() => distanceToAttack;

    public void setCenterpoint(Vector3 point) => centerpoint = point;

    public void setDisoriented(bool state) => isDisoriented = state;

    private void HandleRotation()
    {
        Vector3? currentTargetNullable = getTarget();
        if (currentTargetNullable == null) return;
        Vector3 currentTarget = currentTargetNullable.Value;

        Vector3 targetPos = currentTarget;
        Vector3 direction = targetPos - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.001f) return;

        Vector3 newDir = Vector3.RotateTowards(transform.forward, direction, angularSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    #region detectPlayer
    private bool DetectPlayerByVision(){
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Collider[] hits = Physics.OverlapSphere(origin, range);
        RaycastHit hit;
        Vector3 lookDirection = transform.forward;

        float minDetectionDistance = 1f;

        foreach (Collider collider in hits){
            if (collider.CompareTag("Player")){
                Vector3 targetPos = collider.bounds.center;
                Vector3 dir = (targetPos - origin).normalized;
                float dist = Vector3.Distance(origin, targetPos);

                // 1. CHECAGEM DE PROXIMIDADE EXTREMA (Ignora o cone se estiver muito perto)
                if (dist <= minDetectionDistance){
                    target = targetPos;
                    resetSeeingSmoke();
                    return true;
                }

                // 2. CHECAGEM ANGULAR (Cone de Visao 3D)
                if (Vector3.Dot(lookDirection, dir) >= Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad)){
                    if (Physics.Raycast(origin, dir, out hit, dist, obstacleMask, QueryTriggerInteraction.Collide)){
                        if (((1 << hit.collider.gameObject.layer) & smokeMask) != 0)
                            seeingSmoke = true;
                        else
                            resetSeeingSmoke();
                    }
                    else{
                        Debug.DrawLine(origin, targetPos, Color.blue);
                        target = targetPos;
                        resetSeeingSmoke();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private Vector3 lastKnownPlayerDirection;
    private bool DetectPlayerBySound(){
        Collider[] hits = Physics.OverlapSphere(transform.position, hearingRange);
        bool detected = false;

        Vector3 currentPlayerDirection = Vector3.zero;
        Vector3 lastKnownTargetPosition = Vector3.zero;

        foreach (Collider hit in hits){
            if (!hit.CompareTag("Player")) continue;

            Vector3 playerPos = hit.bounds.center;
            NavMeshHit navHit;
            Vector3 dir = Vector3.zero;

            // Atualiza ultimo ponto
            if (NavMesh.SamplePosition(playerPos, out navHit, 2f, agent.areaMask))
                lastKnownTargetPosition = navHit.position;

            Transform playerTransform = hit.transform;
            currentPlayerDirection = playerTransform.forward;
            if (playerInputs != null) {
                bool isLoud = !playerInputs.crouch && playerInputs.move != Vector2.zero;
                if (isLoud) {
                    target = lastKnownTargetPosition;
                    Debug.DrawLine(transform.position, target.Value, Color.blue);
                    if (CanSee(playerPos))
                        detected = true;
                    lastKnownPlayerDirection = currentPlayerDirection;
                }
            }
        }

        if (!detected && lastKnownTargetPosition != Vector3.zero)
        {
            Vector3 dir = (lastKnownTargetPosition - transform.position).normalized;
            target = transform.position + dir * hearingRange; // ponto na borda do range
            bool isLoud = !playerInputs.crouch;
            if (isLoud) {
                if (CanSee(target.Value))
                    detected = true;
            }
        }

        return detected;
    }

    bool CanSee(Vector3 targetPos)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 dir = (targetPos - origin).normalized;
        float dist = Vector3.Distance(origin, targetPos);
        return !Physics.Raycast(origin, dir, dist, obstacleMask, QueryTriggerInteraction.Collide);
    }

    private bool DetectLockedDoor(){
        NPCGroupController groupController = transform.parent.GetComponent<NPCGroupController>();
        if (groupController.getDoorNPC1() != null) //ja tao indo abrir uma porta (UMA POR VEZ)
            return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Collider[] hits = Physics.OverlapSphere(origin, range);
        //RaycastHit hit;
        Vector3 lookDirection = transform.forward;

        foreach (Collider collider in hits){
            if (collider.CompareTag("LokedDoor")){
                DoorController door = collider.transform.parent.GetComponent<DoorController>();
                Vector3 targetPos = door.getFirstPoint();
                Vector3 target2Pos = door.getSecondPoint();

                if (groupController.getNumberOfNPCs() >= 2){

                    //float dist = Vector3.Distance(origin, targetPos);
                    Vector3 dir = (targetPos - origin).normalized;
                    if (Vector3.Dot(lookDirection, dir) >= Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad)){

                        if(groupController.getDoorNPC1() != null && groupController.getDoorNPC1() == this)
                            Debug.DrawLine(origin, targetPos, Color.gray);
                        else
                        if (groupController.getDoorNPC2() != null && groupController.getDoorNPC2() == this)
                            Debug.DrawLine(origin, target2Pos, Color.gray);

                        targetDoor = targetPos;
                        groupController.setDoorPosition(target2Pos);
                        groupController.setDoor(door);
                        return true;
                    }
                }else
                    groupController.setDoorPosition(Vector3.zero);
            }
        }
        targetDoor = null;
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

    public bool isAlive() => alive;

    public void setTriggerAnim(string animation){
        if(animator != null)
            animator.SetTrigger(animation);
    }
    public void setAnimNow(string animation)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animation)) return;
        //animator.Play(animation, 0, 0f);
        animator.CrossFade(animation, 0.15f, 0, 0f);
    }

    public void kill()
    {
        setAnimNow("Death");
        alive = false;
    }

    float lastAttack = -1f;
    public void attack()
    {
        if (Time.time - lastAttack < timeBetweenAttacks) return;
        lastAttack = Time.time;
        FindAnyObjectByType<PlayerController>().receiveDamage();
    }

    public void PlayAudio()
    {
        GetComponent<AudioSource>().Play();
    }

    void OnDrawGizmos()
    {
        if (!isAlive())
            return;
        DrawFieldOfViewFilled(Color.red);
        DrawFieldOfViewBorders(true, Color.red);
        DrawHearingZone(Color.blue);
        DrawHearingZoneProjectile(Color.blue);
        //DrawSecondaryCone(Color.red);
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