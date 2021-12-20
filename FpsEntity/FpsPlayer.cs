using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CMF;
using Animancer;

public partial class FpsPlayer : FpsCharacter
{
	[SerializeField]
	private ProgressionWeaponConfig progressionWeaponConfig;

	[SerializeField]
	private Transform weaponViewParent;
    
	[SerializeField]
	private ArmBoneToWeaponBone arm;
	private CMF.AdvancedWalkerController playerController;
    private float moveSpeed = 5.5f;
    private ActionCooldown painShockCooldown = new ActionCooldown(){ interval = 0.1f};
	
    public GameObject viewCamera;
    public GameObject fpCameraContainer;
    public GameObject tpCameraContainer;
    public CMF.CameraController cameraController;
	
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
    
    private PlayerSettingDto localPlayerSettingDto;
    private FpsWeaponPlayerInputHandler weaponInputHandler;
    
    // This is the one attached to the fpsCamera , sync the position of LookAt to this so as to sync the character rotation.
    [SerializeField] Transform localPlayerLookAt;
        
	protected override void Start()
	{
		base.Start();
        painShockCooldown.interval = 0.06f;

	    if(isLocalPlayer)
	    {
	    	progressionWeaponConfig.InitializeWeaponList();
            
	    	LocalPlayerContext.Instance.onSwitchWeaponSlotEvent.AddListener(LocalSwitchWeapon);
	    	LocalPlayerContext.Instance.InitalizeFieldsOnFirstSpawn(this);
	    	fpsWeaponView = GetComponentInChildren<FpsWeaponView>();
	    	playerController = GetComponent<CMF.AdvancedWalkerController>();
            cameraController = GetComponentInChildren<CMF.CameraController>();
            
            // For reset the layer for non-local player so that we can see other players.
            //   as our camera has culling mask for LOCAL_PLAYER_MODEL
            // The ragdoll object is LocalPlayerModel , so player won't raycast on themselves in FpsView
            fpsModel.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_LOCAL_PLAYER_MODEL);
            // The children are LocalPlayerHitBox , so that bot can raycast on them
            Utils.ReplaceLayerRecursively(fpsModel.gameObject ,Constants.LAYER_HITBOX, Constants.LAYER_LOCAL_PLAYER_HITBOX);
            
            localPlayerSettingDto = LocalPlayerContext.Instance.playerSettingDto;
            weaponInputHandler = new FpsWeaponPlayerInputHandler(this);
            
            LoadLocalPlayerSettings();
            
            CmdSetupPlayer(LocalPlayerContext.Instance.playerSettingDto.playerName);
            
            CmdGetWeapon("csgo_awp" , 0);
            CmdGetWeapon("csgo_ak47" , 1);
            CmdGetWeapon("csgo_sawoff" , 2);
	    }
	    else
	    {
            
	    }
        
        if(isServer)
        {
            team = TeamEnum.TeamA;
            ServerContext.Instance.playerList.Add(this);
            // *ToFix* Teleport only , since 'Respawn' will cause null on client side 
            PlayerManager.Instance.TeleportToSpawnPoint(this);
        }
        

        Utils.ChangeTagRecursively(modelObject , Constants.TAG_PLAYER , true);
	}
    
    [Command]
    public void CmdSetupPlayer(string playerName)
    {
        this.characterName = playerName;
    }
    
    public void LoadLocalPlayerSettings()
    {
        cameraController.cameraSpeed = localPlayerSettingDto.mouseSpeed;
        
        AudioManager.Instance.localPlayerAudioSource.transform.SetParent(cameraController.transform);
        AudioManager.Instance.localPlayerAudioSource.transform.localPosition = Vector3.zero;
    }
    
	protected override void Update()
    {
	    base.Update();
        if(isLocalPlayer)
        {
            lookAtTransform.position = localPlayerLookAt.position;
            if(painShockCooldown.IsOnCooldown())
                painShockCooldown.ReduceCooldown(Time.deltaTime);
            else
            {
                if(playerController.movementSpeed != moveSpeed)
                    playerController.movementSpeed = moveSpeed;               
            }
            LerpHandView();
        }
    }
        
	private void LerpHandView()
	{
		if(playerController == null || playerController.GetMovementVelocity() == Vector3.zero)
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
	public void OnHitInClient(GameObject hitByObj)
	{
		if(!isClient)	return;
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
            painShockCooldown.StartCooldown();
            playerController.movementSpeed = 0f;
            LocalPlayerContext.Instance.OnHealthUpdate(health , maxHealth);
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
        if(hitInfoDto.hitEntityInfoDtoList != null && hitInfoDto.hitEntityInfoDtoList.Count > 0)
        {
            UiHitMarker.Instance.ShowHitMarker();
        }
        CmdHandleHitInfo(hitInfoDto);
    }
    
    // Tells the server side that I hit something on these locations , process damage for them and spawn fx
    [Command]
    public void CmdHandleHitInfo(HitInfoDto hitInfoDto)
    {
        foreach(HitEntityInfoDto hitInfo in hitInfoDto.hitEntityInfoDtoList)
        {
            FpsEntity hitEntity = hitInfo.networkIdentity.gameObject.GetComponent<FpsEntity>();
            if(hitEntity)
            {
                hitEntity.TakeDamage(hitInfo.damageInfo);
            }
        }

        CoreGameManager.Instance.RpcSpawnFxHitInfo(hitInfoDto);
        // Audio / Muzzle effects
        RpcFireWeapon();
        /*
        foreach(HitWallInfoDto hitWallInfo in hitInfoDto.hitWallInfoDtoList)
        {
            Debug.Log(hitWallInfo.hitPoint);
            Debug.Log(hitWallInfo.hitPointNormal);
        }
        */
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
    }
    
    [TargetRpc]
    public void TargetDoTeleport(NetworkConnection nc, Vector3 position)
    {
        transform.position = position;
    }
    
    protected override void SetRagdollState(bool isRagdollState)
    {
        base.SetRagdollState(isRagdollState);
                
        // Also disable the Mover's collider so it won't fly to sky
        GetComponent<CapsuleCollider>().enabled = isRagdollState ? false : true;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        
        // Local player only settings  e.g MainCamera , LocalPlayerModel layer for cullingMask
        if(isLocalPlayer)
        {
            if(isRagdollState)
            {
                // Change layer to show the body from camera
                fpsModel.bodyRenderer.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_CHARACTER_MODEL);
                // Then we attach view camera to tpView 
                viewCamera.transform.SetParent(tpCameraContainer.transform);
                viewCamera.transform.localEulerAngles = Vector3.zero;
                viewCamera.transform.localPosition = Vector3.zero;
            }
            else
            {
                // Change layer to hide the body from camera
                fpsModel.bodyRenderer.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_LOCAL_PLAYER_MODEL);
                viewCamera.transform.SetParent(fpCameraContainer.transform);
                viewCamera.transform.localEulerAngles = Vector3.zero;
                viewCamera.transform.localPosition = Vector3.zero;
                // Reset the lerp
                isMoveLerpDone = true;
            }
        }
    }
    
	#endregion
	
	void OnDestroy()
	{
        base.OnDestroy();
		if(isServer)
			ServerContext.Instance.playerList.Remove(this);
	}
        
    public override Vector3 GetMovementVelocity()
    {
        if(isLocalPlayer)
        {
            Vector3 controllerVelocity = playerController.GetMovementVelocity();
            CmdSetVelocity(controllerVelocity);
            currentVelocity = controllerVelocity;
        }
        return currentVelocity;
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
        
        CmdSwitchWeapon(slot);
    }
}
