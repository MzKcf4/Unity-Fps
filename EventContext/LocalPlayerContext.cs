using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Cinemachine;
using UnityEngine.Audio;
using System;

public class LocalPlayerContext : MonoBehaviour
{
	public static LocalPlayerContext Instance;
    // [SerializeField] InputActionAsset playerInputActionAsset;
    private PlayerInputActions playerInputActions;
    private Dictionary<string, string> dictAdditionalInfo = new Dictionary<string, string>();
    
	// ----- Input events : events fired when corresponding input key pressed -----
	public InputWeaponPrimaryActionEvent weaponPrimaryActionInputEvent = new InputWeaponPrimaryActionEvent();
    public UnityEvent<KeyPressState> weaponSecondaryActionInputEvent = new UnityEvent<KeyPressState>();
    
    public InputActionContextEvent movementInputEvent = new InputActionContextEvent();
    public UnityEvent<InputAction.CallbackContext> mouseLookInputEvent = new UnityEvent<InputAction.CallbackContext>();
	public UnityEvent weaponReloadInputEvent = new UnityEvent();
	public UnityEvent<int> onSwitchWeaponSlotEvent = new UnityEvent<int>();
    public UnityEvent previousWeaponInputEvent = new UnityEvent();
	// ---------------
	public UnityEvent onWeaponShootEvent = new UnityEvent();
	public UnityEvent<int,int> onHealthUpdateEvent = new UnityEvent<int,int>();
	public UnityEvent<InputAction.CallbackContext> jumpEvent = new UnityEvent<InputAction.CallbackContext>();
    public UnityEvent onOptionMenuToggleEvent = new UnityEvent();
    public UnityEvent onTempDeathmatchWeaponMenuToggleEvent = new UnityEvent();
    // ----------------------------
    private HashSet<InputAction> actionsToDisableInMenu = new HashSet<InputAction>();
    
	[HideInInspector] public FpsPlayer player;
	private CinemachineImpulseSource cameraShake;
	private CinemachineVirtualCamera virtualCamera;
    [HideInInspector] public float weaponRecoilImpulse = 0f;
    
    public PlayerSettingDto playerSettingDto;
    public AudioMixer audioMixerMaster;
    public AudioMixerGroup audioMixerGroup;
    
	void Awake()
	{
		Instance = this;
        playerSettingDto = new PlayerSettingDto();
        playerInputActions = new PlayerInputActions();
        SetupInputActionEvents();
        DontDestroyOnLoad(gameObject);
	}
    
    void Start()
    {
        LoadPlayerSettings();
    }

    private void SetupInputActionEvents()
    {
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.MouseLook);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.Movement);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.WeaponPrimaryAction);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.Slot1);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.Slot2);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.Slot3);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.Reload);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.Jump);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.WeaponSecondaryAction);
        actionsToDisableInMenu.Add(playerInputActions.PlayerControls.PreviousWeapon);

        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.MouseLook, OnMouseLookInput);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.Movement, OnMovementInput);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.WeaponPrimaryAction, OnWeaponPrimaryActionInput);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.MouseLock, OnMouseLockInput);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.Slot1, OnSwitchWeaponSlot1);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.Slot2, OnSwitchWeaponSlot2);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.Slot3, OnSwitchWeaponSlot3);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.Reload, OnWeaponReloadInput);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.Jump, OnJump);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.WeaponSecondaryAction, OnWeaponSecondaryActionInputEvent);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.PreviousWeapon, OnPreviousWeaponInput);
        
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.ToggleOptionMenu, OnOptionMenuToggleInput);
        MapInputActionToHandlerMethod(playerInputActions.PlayerControls.TempDeathmatchWeaponMenu, OnTempDeathmatchWeaponMenuToggle);
    }

    private void MapInputActionToHandlerMethod(InputAction action , Action<InputAction.CallbackContext> inputHandlerMethod)
    {
        action.started += inputHandlerMethod;
        action.performed += inputHandlerMethod;
        action.canceled += inputHandlerMethod;
        action.Enable();
    }
	
	public void InitalizeFieldsOnFirstSpawn(FpsPlayer fpsPlayer)
	{
		this.player = fpsPlayer;
		cameraShake = fpsPlayer.GetComponentInChildren<CinemachineImpulseSource>();
        virtualCamera = fpsPlayer.GetComponentInChildren<CinemachineVirtualCamera>();
        
        dictAdditionalInfo.Clear();
	}
	
	public void ShakeCamera()
	{
        weaponRecoilImpulse = player.GetActiveWeapon().recoil;
        
		if(cameraShake == null)	return;
        cameraShake.GenerateImpulse(Camera.main.transform.forward * player.GetActiveWeapon().cameraShake);
        
	}
    
    public float TakeWeaponRecoilImpulse()
    {
        if(weaponRecoilImpulse > 0f)
        {
            float toReturn = weaponRecoilImpulse;
            weaponRecoilImpulse = 0f;
            return toReturn;
        }
        return 0f;
    }
        
    public void ToggleScope(bool scoped)
    {
        virtualCamera.m_Lens.FieldOfView = scoped ? 15f : 60f;
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
    
    public void OnWeaponSecondaryActionInputEvent(InputAction.CallbackContext value)
    {
        if(value.started){
            weaponSecondaryActionInputEvent.Invoke(KeyPressState.Pressed);
        } else if (value.performed){
            weaponSecondaryActionInputEvent.Invoke(KeyPressState.Holding);
        } else if (value.canceled){
            weaponSecondaryActionInputEvent.Invoke(KeyPressState.Released);
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
    
    public void OnPreviousWeaponInput(InputAction.CallbackContext value)
    {
        if(value.started)
            previousWeaponInputEvent.Invoke();
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
        
    // ============================================================================================
    
    public void LoadPlayerSettings()
    {
        float masterVolume = ES3.Load<float>(Constants.SETTING_KEY_AUDIO_MASTER_VOLUME, -40f);
        float mouseSpeed = ES3.Load<float>(Constants.SETTING_KEY_MOUSE_SPEED, 10);
        float mouseSpeedZoomed = ES3.Load<float>(Constants.SETTING_KEY_MOUSE_SPEED_ZOOMED, 3);
        string playerName = ES3.Load<string>(Constants.SETTING_KEY_PLAYER_NAME);
        
        playerSettingDto.audioMasterVolume = masterVolume;
        playerSettingDto.mouseSpeed = mouseSpeed;       // CameraInput's default mouse sensitivity 0.001
        playerSettingDto.mouseSpeedZoomed = mouseSpeedZoomed;
        playerSettingDto.playerName = playerName;
        
        audioMixerMaster.SetFloat("volume" , masterVolume);
    }
    
    public void SavePlayerSettings()
    {
        ES3.Save<float>(Constants.SETTING_KEY_AUDIO_MASTER_VOLUME, playerSettingDto.audioMasterVolume);
        ES3.Save<float>(Constants.SETTING_KEY_MOUSE_SPEED, playerSettingDto.mouseSpeed);
        ES3.Save<float>(Constants.SETTING_KEY_MOUSE_SPEED_ZOOMED, playerSettingDto.mouseSpeedZoomed);
        ES3.Save<string>(Constants.SETTING_KEY_PLAYER_NAME, playerSettingDto.playerName);
        
        // A fast way to apply settings , when already in game
        if(player != null)
            player.LoadLocalPlayerSettings();
    }
    
    public void UpdatePlayerName(string newName)
    {
        playerSettingDto.playerName = newName;
        ES3.Save<string>(Constants.SETTING_KEY_PLAYER_NAME, playerSettingDto.playerName);
    }
	
    public void OnOptionMenuToggleInput(InputAction.CallbackContext value)
    {
        if(value.started)
            onOptionMenuToggleEvent.Invoke();
    }
    
    public void OnAudioVolumeChanged(float newVolumn)
    {
        audioMixerMaster.SetFloat("volume" , newVolumn);
        playerSettingDto.audioMasterVolume = newVolumn;
    }

    public void OnMouseSpeedChanged(float newSpeed)
    {
        playerSettingDto.mouseSpeed = newSpeed;
    }
    
    public void OnMouseSpeedZoomedChanged(float newSpeed)
    {
        playerSettingDto.mouseSpeedZoomed = newSpeed;
    }
    
    public void OnTempDeathmatchWeaponMenuToggle(InputAction.CallbackContext value)
    {
        if(value.started)
            onTempDeathmatchWeaponMenuToggleEvent.Invoke();
    }

    public void OnMouseLockInput(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ?
                                         CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void TogglePlayerControl(bool isEnable)
    {
        if (isEnable)
        {
            foreach (InputAction inputAction in actionsToDisableInMenu)
                inputAction.Enable();

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            foreach (InputAction inputAction in actionsToDisableInMenu)
                inputAction.Disable();

            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void StoreAdditionalValue(string key , string value)
    {
        if(dictAdditionalInfo.ContainsKey(key))
            dictAdditionalInfo[key] = value;    
        else
            dictAdditionalInfo.Add(key, value);
    }
    
    public string GetAdditionalValue(string key, string defaultValue)
    {
        if(!dictAdditionalInfo.ContainsKey(key))
            return defaultValue;
            
        return dictAdditionalInfo[key];
    }
}
