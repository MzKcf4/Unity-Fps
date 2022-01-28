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
    [HideInInspector] public InputWeaponPrimaryActionEvent weaponPrimaryActionInputEvent = new InputWeaponPrimaryActionEvent();
    [HideInInspector] public UnityEvent<KeyPressState> weaponSecondaryActionInputEvent = new UnityEvent<KeyPressState>();

    [HideInInspector] public InputActionContextEvent movementInputEvent = new InputActionContextEvent();
    [HideInInspector] public UnityEvent<InputAction.CallbackContext> mouseLookInputEvent = new UnityEvent<InputAction.CallbackContext>();
    [HideInInspector] public UnityEvent weaponReloadInputEvent = new UnityEvent();
    [HideInInspector] public UnityEvent<int> onSwitchWeaponSlotEvent = new UnityEvent<int>();
    [HideInInspector] public UnityEvent previousWeaponInputEvent = new UnityEvent();
    // ---------------
    [HideInInspector] public UnityEvent onWeaponShootEvent = new UnityEvent();
    [HideInInspector] public UnityEvent<int,int> onHealthUpdateEvent = new UnityEvent<int,int>();
    [HideInInspector] public UnityEvent<InputAction.CallbackContext> jumpEvent = new UnityEvent<InputAction.CallbackContext>();
    [HideInInspector] public UnityEvent onOptionMenuToggleEvent = new UnityEvent();
    [HideInInspector] public UnityEvent onTempDeathmatchWeaponMenuToggleEvent = new UnityEvent();
    // ----------------------------
    private HashSet<InputAction> actionsToDisableInMenu = new HashSet<InputAction>();

    [HideInInspector] public UnityEvent<WeaponEvent> onWeaponEventUpdate = new UnityEvent<WeaponEvent>();

    [HideInInspector] public FpsPlayer player;

	private CinemachineImpulseSource cameraShake;
	private CinemachineVirtualCamera virtualCamera;
    // This value will be directly used in PlayerContextCameraInput
    [HideInInspector] public float weaponRecoilImpulse = 0f;
    
    public PlayerSettingDto playerSettingDto;
    public AudioMixer audioMixerMaster;
    public AudioMixerGroup audioMixerGroup;
    [SerializeField] public AudioMixerGroup audioMixerMasterGroup;
    public AudioSource localPlayerAudioSource;

    // --------------
    [SerializeField] public Canvas inGameDynamicCanvas;
    [SerializeField] public Canvas inGameCanvas;

    void Awake()
	{
		Instance = this;
        playerSettingDto = new PlayerSettingDto();
        playerInputActions = new PlayerInputActions();
        SetupInputActionEvents();
	}
    
    void Start()
    {
        LoadPlayerSettings();
        Application.targetFrameRate = 120;
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
        if (!player.GetActiveWeapon().isSemiAuto)
            weaponRecoilImpulse = player.GetActiveWeapon().currentRecoil;
        
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

    public void EmitWeaponEvent(WeaponEvent newEvent)
    {
        onWeaponEventUpdate.Invoke(newEvent);
    }

    public void LoadPlayerSettings()
    {
        float masterVolume = ES3.Load<float>(Constants.SETTING_KEY_AUDIO_MASTER_VOLUME, -40f);
        float mouseSpeed = ES3.Load<float>(Constants.SETTING_KEY_MOUSE_SPEED, 10f);
        float mouseSpeedZoomed = ES3.Load<float>(Constants.SETTING_KEY_MOUSE_SPEED_ZOOMED, 3f);
        string playerName = ES3.Load<string>(Constants.SETTING_KEY_PLAYER_NAME);
        CrosshairSizeEnum crosshairSize = ES3.Load<CrosshairSizeEnum>(Constants.SETTING_KEY_CROSSHAIR_SIZE , CrosshairSizeEnum.Standard);
        bool isLerpCrosshair = ES3.Load<bool>(Constants.SETTING_KEY_CROSSHAIR_LERP , true);

        playerSettingDto.audioMasterVolume = masterVolume;
        playerSettingDto.mouseSpeed = mouseSpeed;       // CameraInput's default mouse sensitivity 0.001
        playerSettingDto.mouseSpeedZoomed = mouseSpeedZoomed;
        playerSettingDto.playerName = playerName;
        playerSettingDto.crosshairSize = crosshairSize;
        playerSettingDto.isLerpCrosshair = isLerpCrosshair;
        
        audioMixerMaster.SetFloat("volume" , masterVolume);
        Crosshair.Instance.SetSize(crosshairSize);
        Crosshair.Instance.SetLerp(isLerpCrosshair);
    }
    
    public void SavePlayerSettings()
    {
        ES3.Save<float>(Constants.SETTING_KEY_AUDIO_MASTER_VOLUME, playerSettingDto.audioMasterVolume);
        ES3.Save<float>(Constants.SETTING_KEY_MOUSE_SPEED, playerSettingDto.mouseSpeed);
        ES3.Save<float>(Constants.SETTING_KEY_MOUSE_SPEED_ZOOMED, playerSettingDto.mouseSpeedZoomed);
        ES3.Save<string>(Constants.SETTING_KEY_PLAYER_NAME, playerSettingDto.playerName);
        ES3.Save<CrosshairSizeEnum>(Constants.SETTING_KEY_CROSSHAIR_SIZE, playerSettingDto.crosshairSize);
        ES3.Save<bool>(Constants.SETTING_KEY_CROSSHAIR_LERP, playerSettingDto.isLerpCrosshair);

        // A fast way to apply settings , when already in game
        if (player != null)
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

    public void OnCrosshairSizeChanged(CrosshairSizeEnum newSize)
    {
        playerSettingDto.crosshairSize = newSize;
        Crosshair.Instance.SetSize(newSize);
    }

    public void OnCrosshairLerpChanged(bool isLerp)
    {
        playerSettingDto.isLerpCrosshair = isLerp;
        Crosshair.Instance.SetLerp(isLerp);
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
