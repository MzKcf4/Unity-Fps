using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CMF;
using UnityEngine.InputSystem;

public class PlayerContextCharacterInput : CharacterInput
{
	private Vector3 rawInputMovement = Vector3.zero;
	public bool CanMove = true;
    public bool isJumpKeyPressed = false;
	
	void Start()
	{
		LocalPlayerContext.Instance.movementInputEvent.AddListener(OnMovement);
        LocalPlayerContext.Instance.jumpEvent.AddListener(OnJump);
	}
	
	public void OnMovement(InputAction.CallbackContext value)
	{
		Vector2 input = value.ReadValue<Vector2>();
		rawInputMovement = new Vector3(input.x, 0 , input.y);
	}
	
    public void OnJump(InputAction.CallbackContext value)
    {
		Debug.Log("Jump input: " + value.phase);
        isJumpKeyPressed = !value.canceled;
    }
    
	public override float GetHorizontalMovementInput()
	{
		if(CanMove)
			return rawInputMovement.x;
		else
			return 0f;
	}
	public override float GetVerticalMovementInput()
	{
		if(CanMove)
			return rawInputMovement.z;
		else
			return 0f;
	}
	public override bool IsJumpKeyPressed()
	{
		return isJumpKeyPressed;
	}
}