﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using CMF;
using Animancer;

public class FpsPlayer : FpsCharacter
{
	[SerializeField]
	private ProgressionWeaponConfig progressionWeaponConfig;
	
	public FpsWeapon[] weaponSlots = new FpsWeapon[Constants.WEAPON_SLOT_MAX];
	public FpsWeapon activeWeapon;
    [SerializeField] private FpsWeaponView fpsWeaponView;
    
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
    
	protected override void Start()
	{
		base.Start();
        painShockCooldown.interval = 0.06f;
		if(isServer)
		{
			ServerContext.Instance.playerList.Add(this);
		}
    	
	    if(isLocalPlayer)
	    {
	    	progressionWeaponConfig.InitializeWeaponList();
	    	PlayerContext.Instance.onSwitchWeaponSlotEvent.AddListener(SwitchWeapon);
	    	PlayerContext.Instance.InitalizeFieldsOnFirstSpawn(this);
	    	fpsWeaponView = GetComponentInChildren<FpsWeaponView>();
	    	playerController = GetComponent<CMF.AdvancedWalkerController>();
            
            // For reset the layer for non-local player so that we can see other players.
            //   as our camera has culling mask for LOCAL_PLAYER_MODEL
            // The ragdoll object is LocalPlayerModel , so player won't raycast on themselves in FpsView
            fpsModel.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_LOCAL_PLAYER_MODEL);
            // The children are LocalPlayerHitBox , so that bot can raycast on them
            Utils.ReplaceLayerRecursively(fpsModel.gameObject ,Constants.LAYER_HITBOX, Constants.LAYER_LOCAL_PLAYER_HITBOX);
	    }
	    else
	    {
            
	    }
        
        fpsModel.SetLookAtTransform(lookAtTransform);
        Utils.ChangeTagRecursively(modelObject , Constants.TAG_PLAYER , true);
        weaponRootTransform = GetComponentInChildren<CharacterWeaponRoot>().transform;
        GetWeapon(WeaponAssetManager.Instance.ak47WeaponPrefab , 0);
        GetWeapon(WeaponAssetManager.Instance.sawoffWeaponPrefab , 1);
	}
    
	public override void OnStartServer()
	{
		base.OnStartServer();
		PlayerManager.Instance.TeleportToSpawnPoint(this);
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
	

	[ClientRpc]
	public void RpcGetWeapon(string weaponName , int slot)
	{
		if(!isLocalPlayer)	return;
		
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
        }
    }
	
	protected void SwitchWeapon(int slot)
	{        
		if(weaponSlots[slot] == null)
			return;
		
		if(activeWeapon != null)
		{
			activeWeapon.hitTargetEvent.RemoveListener(OnWeaponHitTarget);
			activeWeapon.gameObject.SetActive(false);
		}
		
		activeWeapon = weaponSlots[slot];
		activeWeapon.gameObject.SetActive(true);
		activeWeapon.hitTargetEvent.AddListener(OnWeaponHitTarget);
        fpsWeaponView.SwitchWeapon(slot);
		activeWeapon.DoWeaponDraw();
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
    
    [Server]
    public override void TakeDamage(DamageInfo damageInfo)
    {
        base.TakeDamage(damageInfo);
        
    }
	
	[ClientRpc]
	protected override void RpcTakeDamage(DamageInfo damageInfo)
	{
		base.RpcTakeDamage(damageInfo);
        if(isLocalPlayer)
        {
            PlayerContext.Instance.OnHealthUpdate(health , maxHealth);
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
    
    [ClientRpc]
    protected override void RpcKilled(DamageInfo damageInfo)
    {
        base.RpcKilled(damageInfo);
    }
    
    [Server]
    protected override void Killed(DamageInfo damageInfo)
    {
        base.Killed(damageInfo);
        PlayerManager.Instance.QueueRespawn(this);
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
		if(isServer)
		{
			ServerContext.Instance.playerList.Remove(this);
		}
        ServerContext.Instance.characterList.Remove(this);
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
        
        // return playerController.GetMovementVelocity();
    }
}
