using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using CMF;

public class PlayerContextCameraInput : CameraInput
{
	//Mouse input axes;
	private Vector2 axisInput = Vector2.zero;

	//Invert input options;
	public bool invertHorizontalInput = false;
	public bool invertVerticalInput = false;

	//Use this value to fine-tune mouse movement;
	//All mouse input will be multiplied by this value;
	public float mouseInputMultiplier = 0.01f;
	
	public void Start()
	{
		PlayerContext.Instance.mouseLookInputEvent.AddListener(OnView);
	}
	
	public void OnView(InputAction.CallbackContext value)
	{
		axisInput = value.ReadValue<Vector2>();
	}

	public override float GetHorizontalCameraInput()
	{
		//Get raw mouse input;
		float _input = axisInput.x;
            
		//Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
		if(Time.timeScale > 0f)
		{
			_input /= Time.deltaTime;
			_input *= Time.timeScale;
		}
		else
			_input = 0f;

		//Apply mouse sensitivity;
		_input *= mouseInputMultiplier;

		//Invert input;
		if(invertHorizontalInput)
			_input *= -1f;

		return _input;
	}

	public override float GetVerticalCameraInput()
	{
		//Get raw mouse input;
		float _input = -axisInput.y;
            
		//Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
		if(Time.timeScale > 0f)
		{
			_input /= Time.deltaTime;
			_input *= Time.timeScale;
		}
		else
			_input = 0f;

		//Apply mouse sensitivity;
		_input *= mouseInputMultiplier;

		//Invert input;
		if(invertVerticalInput)
			_input *= -1f;

		return _input;
	}
}
