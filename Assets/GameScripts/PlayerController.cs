using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif

public class PlayerController : MonoBehaviour
{

    //##################### AQUI
    public GameObject smokeGranade;
    public Transform dropObjectPoint;
    [SerializeField] private LayerMask doorLayer;


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
        if (_input.interact){
            GameObject door = CheckRangeDoor();
            if(door != null){
                DoorController doorController = door.GetComponent<DoorController>();
                doorController.LockDoor();
                print("LockDoor");
            }
            else { 
                _input.interact = false;
                Instantiate(smokeGranade, dropObjectPoint.position, dropObjectPoint.rotation);
                print("smoke");
            }
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
}
