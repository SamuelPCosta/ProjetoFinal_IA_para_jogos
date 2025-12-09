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
    public GameObject smokeGranade;
    public Transform dropObjectPoint;

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
        if (_input.interact && !_input.jump)
        {
            print("smoke");
            _input.interact = false;

            Instantiate(smokeGranade, dropObjectPoint.position, dropObjectPoint.rotation);
        }
    }
}
