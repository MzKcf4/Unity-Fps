﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CMF;

public class InputSystemCharacterInput : CharacterInput
{
	private Vector3 rawInputMovement = Vector3.zero;
	public bool CanMove = true;
	
	public void OnMovement(InputAction.CallbackContext value)
	{
		Vector2 input = value.ReadValue<Vector2>();
		rawInputMovement = new Vector3(input.x, 0 , input.y);
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
		return false;
	}
}
