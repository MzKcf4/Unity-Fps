﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CMF;
using Animancer;
using EasyCharacterMovement;
using UnityEngine.InputSystem;

public partial class FpsPlayer : FpsHumanoidCharacter
{
	[SerializeField]
	private Transform weaponViewParent;
    
	[SerializeField]
	private ArmBoneToWeaponBone arm;
    private MzEcmFpCharacter playerController;
	
    public GameObject viewCamera;
    // The anti-clipping camera 
    [SerializeField] Camera weaponViewCamera;

    // ToDo: Add upper force to cameraInput
    private CharacterLook ecmCameraController;

    // ----------- View Layer ------------- //
    [SerializeField]
	private AnimancerComponent handViewAnimancer;
	private bool isLeftLerp = false;
	private bool isMoveLerpDone = true;
	[SerializeField]
	private AnimationClip viewRightMoveClip;
	[SerializeField]
	private AnimationClip viewLeftMoveClip;
	// ------------------------------------- //
    
    private FpsWeaponPlayerInputHandler weaponInputHandler;

    private int previousActiveWeaponSlot = -1;
    // This is the one attached to the fpsCamera , sync the position of LookAt to this so as to sync the character rotation.
    [SerializeField] Transform localPlayerLookAt;

    [HideInInspector] public AudioSource audioSourceLocalPlayer;
    [HideInInspector] public AudioSource audioSourceAnnouncement;

    
    // [SerializeField] private InputActionAsset playerInputAction;
    
    protected override void Start()
	{
		base.Start();
        playerController = GetComponent<MzEcmFpCharacter>();
        ecmCameraController = GetComponent<CharacterLook>();

        if (isLocalPlayer)
	    {
            LocalPlayerContext.Instance.onSwitchWeaponSlotEvent.AddListener(LocalSwitchWeapon);
	    	LocalPlayerContext.Instance.InitalizeFieldsOnFirstSpawn(this);
            LocalPlayerSettingManager.Instance.OnPlayerSettingUpdateEvent.AddListener(LoadLocalPlayerSettings);
            LocalPlayerContext.Instance.buyAmmoInputEvent.AddListener(OnBuyAmmoInput);

            fpsWeaponView = GetComponentInChildren<FpsWeaponView>();


            weaponInputHandler = new FpsWeaponPlayerInputHandler(this);
            // For reset the layer for non-local player so that we can see other players.
            //   as our camera has culling mask for LOCAL_PLAYER_MODEL
            // The ragdoll object is LocalPlayerModel , so player won't raycast on themselves in FpsView
            fpsModel.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_LOCAL_PLAYER_MODEL);
            // The children are LocalPlayerHitBox , so that bot can raycast on them
            Utils.ReplaceLayerRecursively(fpsModel.gameObject ,Constants.LAYER_HITBOX, Constants.LAYER_LOCAL_PLAYER_HITBOX);
            
            LoadLocalPlayerSettings();
            
            CmdSetupPlayer(LocalPlayerSettingManager.Instance.GetPlayerName());
            CmdGetWeapon("csgo_knife_butterfly", 2);
        }
	    else
	    {
            playerController.inputActions = null;
        }
        
        Utils.ChangeTagRecursively(modelObject , Constants.TAG_PLAYER , true);
	}

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        team = TeamEnum.Blue;
        ServerContext.Instance.playerList.Add(this);   
    }

    [Command]
    public void CmdTeleportToSpawnPoint()
    {
        PlayerManager.Instance.TeleportToSpawnPoint(this);
    }

    [Command]
    public void CmdSetupPlayer(string playerName)
    {
        characterName = playerName;
        PlayerManager.Instance.TeleportToSpawnPoint(this);
    }
    
    public void LoadLocalPlayerSettings()
    {
        ecmCameraController.mouseHorizontalSensitivity = LocalPlayerSettingManager.Instance.GetLocalPlayerSettings().GetConvertedMouseSpeed();
        ecmCameraController.mouseVerticalSensitivity = LocalPlayerSettingManager.Instance.GetLocalPlayerSettings().GetConvertedMouseSpeed();

        audioSourceLocalPlayer = gameObject.AddComponent<AudioSource>();
        InitializeLocalAudioSource(audioSourceLocalPlayer);
        LocalPlayerContext.Instance.localPlayerAudioSource = audioSourceLocalPlayer;

        audioSourceAnnouncement = gameObject.AddComponent<AudioSource>();
        InitializeLocalAudioSource(audioSourceAnnouncement);
        LocalPlayerContext.Instance.localPlayerAnnoucementAudioSource = audioSourceAnnouncement;
    }

    private void InitializeLocalAudioSource(AudioSource audioSource)
    {
        audioSource.outputAudioMixerGroup = LocalPlayerContext.Instance.audioMixerGroup;
        audioSource.playOnAwake = false;
        audioSource.transform.SetParent(fpsWeaponView.transform);
        audioSource.transform.localPosition = Vector3.zero;
    }


    protected override void Update()
    {
	    base.Update();
        if(isLocalPlayer)
        {
            weaponAimAt.position = localPlayerLookAt.position;
            LerpHandView();
        }
    }
        
	private void LerpHandView()
	{
		if(playerController == null || playerController.GetMovementDirection() == Vector3.zero)
			return;		
		
		if(!isMoveLerpDone)	return;
		
		isMoveLerpDone = false;
		isLeftLerp = !isLeftLerp;
		if(isLeftLerp)
		{
			var state = handViewAnimancer.Play(viewLeftMoveClip);
			state.Events.OnEnd = () => {
				isMoveLerpDone = true;
				state.Events = null;
			};
		}
		else
		{
			var state = handViewAnimancer.Play(viewRightMoveClip);
			state.Events.OnEnd = () => {
				isMoveLerpDone = true;
				state.Events = null;
			};
		}
	}
        
    #region hit_dmg
    
	[Client]
	public void OnHitInLocalClient(GameObject hitByObj)
	{
		if(!isClient || !isLocalPlayer) return;
		// Tells server it is hit.
		CmdOnHit(hitByObj);
	}
	
	[Command]
	private void CmdOnHit(GameObject hitByObj)
	{
		// Server updates the health
		DamageInfo dmgInfo = new DamageInfo(){
			damage = 10,
			hitPoint = transform.position + transform.up
		};
		
		TakeDamage(dmgInfo);
	}
    	
	[ClientRpc]
	protected override void RpcTakeDamage(DamageInfo damageInfo)
	{
		base.RpcTakeDamage(damageInfo);
        if(isLocalPlayer)
        {
            if (UiDamageIndicatorManager.Instance != null && damageInfo.attacker != null)
            {
                UiDamageIndicatorManager.Instance.CreateIndicator(weaponViewCamera.transform, damageInfo.attacker.transform);
            }
        }
	}
    
	[Client]
	private void OnWeaponHitTarget(GameObject enemyObject, DamageInfo dmgInfo)
	{
		// Client tells server that he hits something!
		if(isClient)
			CmdHitTarget(enemyObject, dmgInfo);
	}
    
	[Command]
	private void CmdHitTarget(GameObject hitObject, DamageInfo dmgInfo)
	{
        FpsEntity fpsEntity = hitObject.GetComponent<FpsEntity>();
		if(fpsEntity != null)
			fpsEntity.TakeDamage(dmgInfo);
	}
    
    // ========== Hitscan detection on local side =========== //
    protected void LocalFireWeapon(Vector3 fromPos , Vector3 forwardVec)
    {
        HitInfoDto hitInfoDto = CoreGameManager.Instance.DoLocalWeaponRaycast(this , GetActiveWeapon() , fromPos , forwardVec);
        if (hitInfoDto == null || hitInfoDto.IsHitNothing()) 
        {
            CmdHandleHitInfo(null);
            return;
        }
            
        if( hitInfoDto.hitEntityInfoDtoList != null && hitInfoDto.hitEntityInfoDtoList.Count > 0)
        {
            UiHitMarker.Instance.ShowHitMarker();
        }
        CmdHandleHitInfo(hitInfoDto);
    }
    
    // Tells the server side that I hit something on these locations , process damage for them and spawn fx
    [Command]
    public void CmdHandleHitInfo(HitInfoDto hitInfoDto)
    {
        if (hitInfoDto != null)
        {
            foreach (HitEntityInfoDto hitInfo in hitInfoDto.hitEntityInfoDtoList)
            {
                FpsEntity hitEntity = hitInfo.victimIdentity.gameObject.GetComponent<FpsEntity>();
                if (hitEntity)
                {
                    hitEntity.TakeDamage(hitInfo.damageInfo);
                    // Only send to the damage dealer
                    TargetSpawnDamageText(hitInfo.attackerIdentity.connectionToClient, hitInfo.damageInfo.damage, hitInfo.damageInfo.hitPoint, hitInfo.damageInfo.bodyPart == BodyPart.Head);
                }
            }

            CoreGameManager.Instance.RpcSpawnFxHitInfo(hitInfoDto);
        }
        
        // Audio / Muzzle effects
        RpcFireWeapon();
    }

    [TargetRpc]
    public void TargetSpawnDamageText(NetworkConnection target, int damage, Vector3 position , bool isHeadshot)
    {
        LocalSpawnManager.Instance.SpawnDamageText(damage, position + Vector3.up , isHeadshot);
    }
    // ==================================================== //
    
    // Server then do Raycast to check if it hits
    //    and tells other clients to create weapon fire effects (e.g  muzzleFlash , audio )
    [Command]
    public void CmdFireWeapon(Vector3 fromPos , Vector3 forwardVec)
    {
        CoreGameManager.Instance.DoWeaponRaycast(this , GetActiveWeapon() , fromPos , forwardVec);
        RpcFireWeapon();
    }
        
    [TargetRpc]
    public void TargetOnWeaponHitEnemy(NetworkConnection target)
    {
        if(isLocalPlayer)
            UiHitMarker.Instance.ShowHitMarker();
    }
    
    [ClientRpc]
    protected override void RpcKilled(DamageInfo damageInfo)
    {
        base.RpcKilled(damageInfo);
        if (isLocalPlayer)
        {
            // Unscope the weapon on death
            OnWeaponUnScopeEvent();

            if(GetActiveWeapon() != null)
                GetActiveWeapon().ResetActionState();
        }
    }
    
    [TargetRpc]
    public void TargetDoTeleport(NetworkConnection nc, Vector3 position)
    {
        transform.position = position;
    }
    
    protected override void SetRagdollState(bool isRagdollState)
    {
        base.SetRagdollState(isRagdollState);
                
        // Disable the Mover's collider/rigidbody so it won't fly to sky
        GetComponent<CapsuleCollider>().enabled = isRagdollState ? false : true;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        
        // Local player only settings  e.g MainCamera , LocalPlayerModel layer for cullingMask
        if(isLocalPlayer)
        {
            if(isRagdollState)
            {
                ecmCharacter.SetMovementMode(MovementMode.None);
                // Change layer to show the body from camera
                fpsModel.SetRendererLayer(Constants.LAYER_CHARACTER_MODEL);
                // Then we attach view camera to tpView 
                // viewCamera.transform.SetParent(tpCameraContainer.transform);
                viewCamera.transform.localEulerAngles = Vector3.zero;
                viewCamera.transform.localPosition = Vector3.zero;
            }
            else
            {
                ecmCharacter.SetMovementMode(MovementMode.Walking);
                // Change layer to hide the body from camera
                fpsModel.SetRendererLayer(Constants.LAYER_LOCAL_PLAYER_MODEL);
                // viewCamera.transform.SetParent(fpCameraContainer.transform);
                viewCamera.transform.localEulerAngles = Vector3.zero;
                viewCamera.transform.localPosition = Vector3.zero;
                // Reset the lerp
                isMoveLerpDone = true;
            }
        }
    }

    protected override string GetModelLayerName()
    {
        return Constants.LAYER_LOCAL_PLAYER_MODEL;
    }

    protected override string GetHitboxLayerName()
    {
        return Constants.LAYER_LOCAL_PLAYER_HITBOX;
    }

    #endregion

    protected override void OnDestroy()
	{
        base.OnDestroy();
		if(isServer)
			ServerContext.Instance.playerList.Remove(this);
	}

    public override void RpcHealthUpdate()
    {
        base.RpcHealthUpdate();
        if(isLocalPlayer)
            LocalPlayerContext.Instance.OnHealthUpdate(health , maxHealth);
    }
    
    protected void LocalSwitchWeapon(int slot)
    {        
        if(weaponSlots[slot] == null)
            return;

        previousActiveWeaponSlot = activeWeaponSlot;
        CmdSwitchWeapon(slot);
    }

    public void LocalSwitchPreviousWeapon()
    {
        if (previousActiveWeaponSlot != activeWeaponSlot && previousActiveWeaponSlot != -1
            && weaponSlots[previousActiveWeaponSlot] != null)
        {
            LocalSwitchWeapon(previousActiveWeaponSlot);
        }
    }

    protected override void SwitchWeapon(int slot)
    {
        base.SwitchWeapon(slot);

        if (isLocalPlayer)
        {
            OnWeaponUnScopeEvent();
        }
    }

    protected override void OnHealthSync(int oldHealth, int newHealth)
    {
        base.OnHealthSync(oldHealth, newHealth);

        if (isLocalPlayer)
        {
            LocalPlayerContext.Instance.OnHealthUpdate(newHealth, maxHealth);
        }
    }

    private void OnBuyAmmoInput()
    {
        if (!isLocalPlayer || CoreGameManager.Instance.GameMode != GameModeEnum.Monster)
            return;

        FpsWeapon activeWeapon = GetActiveWeapon();
        if (activeWeapon == null || activeWeapon.weaponCategory == WeaponCategory.Melee)
            return;

        int ammopack = int.Parse(TryGetAdditionalInfoValue(Constants.ADDITIONAL_INFO_AMMOPACK, "0"));
        if (ammopack <= 0)
            return;

        int backAmmo = MonsterModeManager.Instance.GetBackAmmoForWeaponType(activeWeapon.weaponCategory, activeWeapon.isSemiAuto);
        ammopack--;
        dictAdditionalInfo[Constants.ADDITIONAL_INFO_AMMOPACK] = ammopack.ToString();

        dictBackAmmo[activeWeapon.weaponCategory.ToString()] += backAmmo;
        UpdateAmmoDisplay();
        UpdateAmmoPackDisplay();
    }

    [TargetRpc]
    public void TargetUpdateAmmoPack(NetworkConnection conn , int amount) 
    {
        int existing = int.Parse(TryGetAdditionalInfoValue(Constants.ADDITIONAL_INFO_AMMOPACK, "0"));
        dictAdditionalInfo[Constants.ADDITIONAL_INFO_AMMOPACK] = (existing += amount).ToString();
        UpdateAmmoPackDisplay();
    }

    private void UpdateAmmoPackDisplay()
    {
        int existing = int.Parse(TryGetAdditionalInfoValue(Constants.ADDITIONAL_INFO_AMMOPACK, "0"));
        MonsterModeUiManager.Instance.UpdateAmmoPackText(existing);
    }

    private string TryGetAdditionalInfoValue(string key , string defaultValue) 
    {
        if (!dictAdditionalInfo.ContainsKey(key))
        {
            dictAdditionalInfo.Add(key, defaultValue);
        }
        return dictAdditionalInfo[key];
    }
}
