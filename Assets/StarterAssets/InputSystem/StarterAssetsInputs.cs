using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool dash;
		public bool crouch;
		public bool smoke;
		public bool action;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnDash(InputValue value)
		{
			DashInput(value.isPressed);
		}

		public void OnCrouch(InputValue value)
		{
			CrouchInput(value.isPressed);
		}

		public void OnSmoke(InputValue value)
		{
			SmokeInput(value.isPressed);
		}

		public void OnAction(InputValue value)
		{
			ActionInput(value.isPressed);
		}

#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void DashInput(bool newDashState)
		{
			dash = newDashState;
		}

		public void CrouchInput(bool newCrouchState)
		{
			crouch = newCrouchState;
		}

		public void SmokeInput(bool newSmokeState)
		{
			smoke = newSmokeState;
		}

		public void ActionInput(bool newActionState)
		{
			action = newActionState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}