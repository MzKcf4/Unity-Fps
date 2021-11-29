using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Cinemachine;

public class PlayerContext : MonoBehaviour
{
	public static PlayerContext Instance;
	// ----- Input events : events fired when corresponding input key pressed -----
	public InputWeaponPrimaryActionEvent weaponPrimaryActionInputEvent = new InputWeaponPrimaryActionEvent();
	public InputActionContextEvent movementInputEvent = new InputActionContextEvent();
	public InputActionContextEvent mouseLookInputEvent = new InputActionContextEvent();
	public UnityEvent weaponReloadInputEvent = new UnityEvent();
	public UnityEvent<int> onSwitchWeaponSlotEvent = new UnityEvent<int>();	
	// ---------------
	public UnityEvent onWeaponShootEvent = new UnityEvent();
	public UnityEvent<int,int> onHealthUpdateEvent = new UnityEvent<int,int>();
	public UnityEvent<InputAction.CallbackContext> jumpEvent = new UnityEvent<InputAction.CallbackContext>();
    
    
	public FpsPlayer player;
	private CinemachineImpulseSource cameraShake;
	
	void Awake()
	{
		Instance = this;
	}
	
	public void InitalizeFieldsOnFirstSpawn(FpsPlayer fpsPlayer)
	{
		this.player = fpsPlayer;
		cameraShake = fpsPlayer.GetComponentInChildren<CinemachineImpulseSource>();
	}
	
	public void ShakeCamera()
	{
		if(cameraShake == null)	return;
		cameraShake.GenerateImpulse(Camera.main.transform.forward);
	}
	
	public void OnWeaponPrimaryActionInput(InputAction.CallbackContext value)
	{
		if(value.started){
			weaponPrimaryActionInputEvent.Invoke(KeyPressState.Pressed);
		} else if (value.performed){
			weaponPrimaryActionInputEvent.Invoke(KeyPressState.Holding);
		} else if (value.canceled){
			weaponPrimaryActionInputEvent.Invoke(KeyPressState.Released);
		}
	}
	
	public void OnSwitchWeaponSlot1(InputAction.CallbackContext value)
	{
		if(value.started)
			onSwitchWeaponSlotEvent.Invoke(Constants.WEAPON_SLOT_1);
	}
	
	public void OnSwitchWeaponSlot2(InputAction.CallbackContext value)
	{
		if(value.started)
			onSwitchWeaponSlotEvent.Invoke(Constants.WEAPON_SLOT_2);
	}
	
	public void OnSwitchWeaponSlot3(InputAction.CallbackContext value)
	{
		if(value.started)
			onSwitchWeaponSlotEvent.Invoke(Constants.WEAPON_SLOT_3);
	}
	
	public void OnWeaponReloadInput(InputAction.CallbackContext value)
	{
		if(value.started)
			weaponReloadInputEvent.Invoke();
	}
	
	public void OnWeaponShoot()
	{
		onWeaponShootEvent.Invoke();
	}
	
	public void OnMovementInput(InputAction.CallbackContext value)
	{
		movementInputEvent.Invoke(value);
	}
	
	public void OnMouseLookInput(InputAction.CallbackContext value)
	{
		mouseLookInputEvent.Invoke(value);
	}	
    
    public void OnJump(InputAction.CallbackContext value)
    {
        jumpEvent.Invoke(value);
    }
	
	public void OnHealthUpdate(int newHealth , int maxHealth)
	{
		onHealthUpdateEvent.Invoke(newHealth, maxHealth);
	}
	
	
}
