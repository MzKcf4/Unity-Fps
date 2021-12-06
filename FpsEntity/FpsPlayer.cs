using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CMF;
using Animancer;

public class FpsPlayer : FpsCharacter
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
    private FpsWeaponEventHandler weaponEventHandler;
    private FpsWeaponPlayerInputHandler weaponInputHandler;
        
	protected override void Start()
	{
		base.Start();
        painShockCooldown.interval = 0.06f;

	    if(isLocalPlayer)
	    {
	    	progressionWeaponConfig.InitializeWeaponList();
            
	    	PlayerContext.Instance.onSwitchWeaponSlotEvent.AddListener(LocalSwitchWeapon);
	    	PlayerContext.Instance.InitalizeFieldsOnFirstSpawn(this);
            PlayerWeaponViewContext.Instance.onWeaponEventUpdate.AddListener(OnWeaponEventUpdate);
	    	fpsWeaponView = GetComponentInChildren<FpsWeaponView>();
	    	playerController = GetComponent<CMF.AdvancedWalkerController>();
            cameraController = GetComponentInChildren<CMF.CameraController>();
            
            // For reset the layer for non-local player so that we can see other players.
            //   as our camera has culling mask for LOCAL_PLAYER_MODEL
            // The ragdoll object is LocalPlayerModel , so player won't raycast on themselves in FpsView
            fpsModel.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_LOCAL_PLAYER_MODEL);
            // The children are LocalPlayerHitBox , so that bot can raycast on them
            Utils.ReplaceLayerRecursively(fpsModel.gameObject ,Constants.LAYER_HITBOX, Constants.LAYER_LOCAL_PLAYER_HITBOX);
            
            localPlayerSettingDto = PlayerContext.Instance.playerSettingDto;
            weaponEventHandler = new FpsWeaponEventHandler(this);
            weaponInputHandler = new FpsWeaponPlayerInputHandler(this);
            
            LoadLocalPlayerSettings();
            
            CmdGetWeapon("csgo_awp" , 0);
            CmdGetWeapon("csgo_ak47" , 1);
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
        
        fpsModel.SetLookAtTransform(lookAtTransform);
        Utils.ChangeTagRecursively(modelObject , Constants.TAG_PLAYER , true);
	}
    
    [Command]
    public void CmdGetWeapon(string weaponName , int slot)
    {
        weaponHandler.CmdGetWeapon(weaponName, slot);
        RpcGetWeapon(weaponName, slot);
    }
    
    [ClientRpc]
    public void RpcGetWeapon(string weaponName, int slot)
    {
        weaponHandler.GetWeapon(weaponName, slot);
        
        // Force switch weapon if it's main slot
        if((isServer || isLocalPlayer) && slot == 0)
            CmdSwitchWeapon(slot);
    }
    
    [Command]
    public void CmdSwitchWeapon(int slot)
    {
        weaponHandler.SwitchWeapon(slot);
        RpcSwitchWeapon(slot);
    }
    
    [ClientRpc]
    protected void RpcSwitchWeapon(int slot)
    {
        weaponHandler.SwitchWeapon(slot);
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
    
    private void OnWeaponEventUpdate(WeaponEvent evt)
    {
        if(!isLocalPlayer)  return;
        
        if(evt == WeaponEvent.Shoot)
            OnWeaponFireEvent();
        else if (evt == WeaponEvent.Scope)
            OnWeaponScopeEvent();
        else if (evt == WeaponEvent.UnScope)
            OnWeaponUnScopeEvent();
    }
    
    private void OnWeaponScopeEvent()
    {
        FpsUiManager.Instance.ToggleCrosshair(false);
        FpsUiManager.Instance.ToggleScope(true);
        PlayerContext.Instance.ToggleScope(true);
        cameraController.cameraSpeed = (float)localPlayerSettingDto.mouseSpeedZoomed;
    }
    
    private void OnWeaponUnScopeEvent()
    {
        FpsUiManager.Instance.ToggleCrosshair(true);
        FpsUiManager.Instance.ToggleScope(false);
        PlayerContext.Instance.ToggleScope(false);
        cameraController.cameraSpeed = (float)localPlayerSettingDto.mouseSpeed;
    }
    
    // Subscribe to weapon fire event, so when weapon is fired ( in fps view ) , 
    //   notify the server to do corresponding actions
    private void OnWeaponFireEvent()
    {
        Transform fromTransform = Camera.main.transform;
        
        Vector3 fromPos = Camera.main.transform.position;
        Vector3 forwardVec = Camera.main.transform.forward;
        
        CmdFireWeapon(fromPos , forwardVec);
    }
    
    // Server then do Raycast to check if it hits
    //    and tells other clients to create weapon fire effects (e.g  muzzleFlash , audio )
    [Command]
    public void CmdFireWeapon(Vector3 fromPos , Vector3 forwardVec)
    {
        CoreGameManager.Instance.DoWeaponRaycast(this , GetActiveWeapon() , fromPos , forwardVec);
        RpcFireWeapon();
    }
    
    // 
    [ClientRpc]
    public void RpcFireWeapon()
    {
        AudioManager.Instance.PlaySoundAtPosition(GetActiveWeapon().GetShootSound() , GetActiveWeapon().GetMuzzlePosition());
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
            PlayerContext.Instance.OnHealthUpdate(health , maxHealth);
    }
    
    protected void LocalSwitchWeapon(int slot)
    {        
        if(weaponSlots[slot] == null)
            return;
        
        CmdSwitchWeapon(slot);
        /*
        CmdSwitchWeapon(slot);
        SwitchWeapon(slot);
        fpsWeaponView.SwitchWeapon(slot);
        activeWeapon.DoWeaponDraw();
        */
    }
    
    
    /*
    [Command]
    public void CmdGetWeapon()
    {
        
    }

    [ClientRpc]
    public void RpcGetWeapon(string weaponName , int slot)
    {
        if(!isLocalPlayer)  return;
        
        GameObject weaponPrefab = progressionWeaponConfig.dictNameToWeaponPrefab[weaponName];
        GetWeapon(weaponPrefab , slot);
    }
    
    public void GetWeapon(GameObject weaponModelPrefab , int slot)
    {
        GameObject weaponModelObj = Instantiate(weaponModelPrefab , weaponRootTransform);
        weaponModelObj.transform.localPosition = Vector3.zero;
        FpsWeapon fpsWeapon = weaponModelObj.GetComponent<FpsWeapon>();
        fpsWeapon.owner = this;
        fpsWeapon.gameObject.SetActive(false);
        fpsWeaponView.AddViewWeapon(fpsWeapon, slot);
        
        weaponSlots[slot] = fpsWeapon;
        
        if(slot == 0)
            SwitchWeapon(slot);
        
        if(isLocalPlayer)
        {
            // Change the local weapon model to camera's culling mask too !
            Utils.ChangeLayerRecursively(weaponModelObj , Constants.LAYER_LOCAL_PLAYER_MODEL, true); 
            
            fpsWeaponView.SwitchWeapon(slot);
            activeWeapon.DoWeaponDraw();
        }
    }
    

    [Command]
    protected void CmdSwitchWeapon(int slot)
    {
        SwitchWeapon(slot);
        RpcSwitchWeapon(slot);
    }
    
    [ClientRpc]
    protected void RpcSwitchWeapon(int slot)
    {
        SwitchWeapon(slot);
    }
    
    protected void SwitchWeapon(int slot)
    {
        if(activeWeapon != null)
        {
            activeWeapon.gameObject.SetActive(false);
        }
        
        activeWeapon = weaponSlots[slot];
        activeWeapon.gameObject.SetActive(true);
    }
    */
}
