using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif

public class PlayerController : MonoBehaviour
{

    //##################### AQUI
    [Header("Status")]
    public int energy = 10;
    public int smokeEnergy = 4;
    public int dashEnergy = 6;
    public int projectileEnergy = 1;

    [Space(10)]
    [Header("PlayerConfig")]
    public GameObject smokeGranade;
    public Transform dropObjectPoint;
    [SerializeField] private LayerMask doorLayer;

    public Transform spawnPoint;
    public Transform targetPoint;
    public Cinemachine.CinemachineBrain cam;
    public GameObject projectile;
    public float maxHeight = 2f;
    public float horizontalBoost = 1.8f;
    public float clampThrowAngle = 70f;

    public float angleInfluence = .2f;

    public LineRenderer line;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif

private StarterAssets.StarterAssetsInputs _input;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _input = GetComponent<StarterAssets.StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (_input.action){
            GameObject door = CheckRangeDoor();
            if(door != null){
                DoorController doorController = door.GetComponent<DoorController>();
                doorController.LockDoor();
                print("LockDoor");
                _input.action = false;
            }
        }

        if (_input.smoke){
            _input.action = false;
            Instantiate(smokeGranade, dropObjectPoint.position, dropObjectPoint.rotation);
            print("smoke");
            _input.smoke = false;
        }

        Vector3 euler = spawnPoint.rotation.eulerAngles;
        euler.x = cam.transform.rotation.eulerAngles.x;

        //eixo Y do player vai ser o 0
        float playerY = transform.eulerAngles.y;
        float cameraY = cam.transform.eulerAngles.y;
        cameraY = Mathf.DeltaAngle(playerY, cameraY) + playerY;
        float clampedY = Mathf.Clamp(cameraY, playerY - clampThrowAngle, playerY + clampThrowAngle);
        euler.y = clampedY;

        spawnPoint.rotation = Quaternion.Euler(euler);

        DrawTrajectory();
        if (Input.GetKeyDown(KeyCode.R))
        {
            LaunchProjectile();
        }
    }

    private GameObject CheckRangeDoor()
    {
        Collider playerCol = GetComponent<CharacterController>();
        Collider[] hits = Physics.OverlapBox(playerCol.bounds.center, playerCol.bounds.extents, Quaternion.identity, doorLayer);
        if (hits.Length > 0)
        {
            if (hits[0].transform.parent != null) return hits[0].transform.parent.gameObject;
            return null;
        }
        return null;
    }

    private (float totalTime, Vector3 planarVel, float initialVy, float g) CalculateLaunchParameters(Vector3 start, Vector3 end, float maxHeight, float horizontalBoost){
        float g = Mathf.Abs(Physics.gravity.y);

        Vector3 mid = (start + end) / 2f;

        float camX = cam.transform.rotation.eulerAngles.x;
        if (camX > 180f) camX -= 360f;

        float angleFactor = Mathf.Abs(camX);

        float safeAngleFactor = Mathf.Max(1.0f, angleFactor * angleInfluence);

        float modulatedMaxHeight = maxHeight / safeAngleFactor;

        float minHeightRequired = Mathf.Max(start.y, end.y);
        mid.y = minHeightRequired + Mathf.Max(0.5f, modulatedMaxHeight);

        float timeUp = Mathf.Sqrt(2f * (mid.y - start.y) / g);
        float timeDown = Mathf.Sqrt(2f * (mid.y - end.y) / g);
        float totalTime = timeUp + timeDown;

        float initialVy = g * timeUp;

        Vector3 planarDir = new Vector3(end.x - start.x, 0f, end.z - start.z);
        Vector3 planarVel = planarDir / totalTime;
        planarVel *= horizontalBoost;

        return (totalTime, planarVel, initialVy, g);
    }

    private void LaunchProjectile()
    {
        Vector3 start = spawnPoint.position;
        Vector3 end = targetPoint.position;

        var parameters = CalculateLaunchParameters(start, end, maxHeight, horizontalBoost);

        GameObject proj = Instantiate(projectile, start, Quaternion.identity);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;

        rb.velocity = new Vector3(parameters.planarVel.x, parameters.initialVy, parameters.planarVel.z);

        Vector3 euler = spawnPoint.rotation.eulerAngles;
        euler.x = cam.transform.rotation.eulerAngles.x;
        spawnPoint.rotation = Quaternion.Euler(euler);
    }

    int resolution = 30;
    private void DrawTrajectory()
    {
        Vector3 start = spawnPoint.position;
        Vector3 end = targetPoint.position;

        var parameters = CalculateLaunchParameters(start, end, maxHeight, horizontalBoost);

        line.positionCount = resolution + 1;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (i / (float)resolution) * parameters.totalTime;

            Vector3 pos = start + parameters.planarVel * t;

            pos.y = start.y + parameters.initialVy * t - 0.5f * parameters.g * t * t;

            line.SetPosition(i, pos);
        }
    }
}
