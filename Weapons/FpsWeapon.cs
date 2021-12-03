using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit.Physic;
using MoreMountains.Feedbacks;
using UnityEngine.Events;
using Animancer;


public class FpsWeapon : MonoBehaviour
{
    [SerializeField] public GameObject weaponViewPrefab;
    
    private ActionCooldown cooldownUntilIdle = new ActionCooldown();
    private float secondaryActionInterval = 0.2f;
    private ActionCooldown secondaryActionCooldown = new ActionCooldown();
    
	private WeaponState weaponState = WeaponState.Idle;
    private WeaponSecondaryState weaponSecondaryState = WeaponSecondaryState.None;
    
    public WeaponReloadType reloadType = WeaponReloadType.Clip;
    public WeaponCategory weaponCategory = WeaponCategory.Rifle;
	
	[SerializeField]
	private RaycastSetting raycastSetting;
	public bool isMelee = false;
	
	private MMFeedbacks muzzleFeedbacks;
	private KeyPressState primaryActionState = KeyPressState.Released;
    private KeyPressState secondaryActionState = KeyPressState.Released;
	
    public bool isFirstPersonView = true;
    
    [HideInInspector] public FpsWeaponWorldModel weaponWorldModel;
	
    public string weaponName;
    private int currentClip;
    
	[HideInInspector] public int damage = 20;
	private int clipSize = 30;
    private float reloadTime = 3f;
    private float reloadTime_PalletStart = 0.2f;
    private float reloadTime_PalletInsert = 0.2f;
    private float reloadTime_PalletEnd = 0.2f;
    private float drawTime = 2f;
    
    [HideInInspector] public float shootInterval = 0.1f;
    
    
    [HideInInspector] public int palletPerShot = 1;
    [HideInInspector] public float spread = 0.1f;
    
    [HideInInspector] public FpsCharacter owner;
    
	void Awake()
	{
        FetchDataFromDb();
		muzzleFeedbacks = GetComponentInChildren<MMFeedbacks>();
        weaponWorldModel = GetComponent<FpsWeaponWorldModel>();
        currentClip = clipSize;
        secondaryActionCooldown.interval = secondaryActionInterval;
	}
    
    private void FetchDataFromDb()
    {
        E_weapon_info dbWeaponInfo = E_weapon_info.GetEntity(weaponName);
        
        damage = dbWeaponInfo.f_base_damage;
        clipSize = dbWeaponInfo.f_clip_size;
        shootInterval = dbWeaponInfo.f_shoot_interval;
        reloadType = dbWeaponInfo.f_reload_type;
        weaponCategory = dbWeaponInfo.f_category;
        reloadTime = dbWeaponInfo.f_reload_time;
        reloadTime_PalletStart = dbWeaponInfo.f_reload_time_pallet_start;
        reloadTime_PalletInsert = dbWeaponInfo.f_reload_time_pallet_insert;
        reloadTime_PalletEnd = dbWeaponInfo.f_reload_time_pallet_end;
        drawTime = dbWeaponInfo.f_draw_time;
        palletPerShot = dbWeaponInfo.f_pallet_per_shot;
        spread = dbWeaponInfo.f_spread;
    }
    
	void Start()
	{

	}

	void Update()
	{
        if(owner.IsDead())  return;
        // cooldown with secondary action
        HandleCooldownInterrupt();
        HandleCooldown();
        if(owner is FpsPlayer && owner.isLocalPlayer)
        {
    		if(weaponState == WeaponState.Idle && primaryActionState != KeyPressState.Released)
    		{
    			DoWeaponPrimaryAction();
    		}
            if(weaponState == WeaponState.Idle && secondaryActionState != KeyPressState.Released)
            {
                DoWeaponSecondaryAction();
            }
        }
	}
    
    private void HandleCooldownInterrupt()
    {
        if(weaponState == WeaponState.Idle || !cooldownUntilIdle.IsOnCooldown()) return;
        
        // During pallet insert , if player tries to fire the weapon, transit to Reload-End immediately
        if(weaponState == WeaponState.Reloading_PalletInsert && primaryActionState != KeyPressState.Released)
        {
            cooldownUntilIdle.StartCooldown(reloadTime_PalletEnd);
            EmitWeaponViewEvent(WeaponEvent.Reload_PalletEnd);
            weaponState = WeaponState.Reloading_PalletEnd;
            return;
        }
    }
    
    private void HandleCooldown()
    {
        secondaryActionCooldown.ReduceCooldown();
        
        if(weaponState == WeaponState.Idle) return;
        
        if(cooldownUntilIdle.IsOnCooldown())
        {
            cooldownUntilIdle.ReduceCooldown(Time.deltaTime);
        }
        else
        {
            // When action is done, update weapon info according to action.
            if(weaponState == WeaponState.Reloading)
            {
                currentClip = clipSize;
                UpdateAmmoDisplay();
                weaponState = WeaponState.Idle;
                return;
            } 
            else if (weaponState == WeaponState.Reloading_PalletStart)
            {
                // next action is pallet insert
                cooldownUntilIdle.StartCooldown(reloadTime_PalletInsert);
                EmitWeaponViewEvent(WeaponEvent.Reload_PalletInsertStart);
                weaponState = WeaponState.Reloading_PalletInsert;
                return;
            }
            else if (weaponState == WeaponState.Reloading_PalletInsert)
            {
                currentClip++;
                UpdateAmmoDisplay();
                if(currentClip != clipSize)
                {
                    cooldownUntilIdle.StartCooldown(reloadTime_PalletInsert);
                    EmitWeaponViewEvent(WeaponEvent.Reload_PalletInsertStart);
                    return;
                }
                else
                {
                    cooldownUntilIdle.StartCooldown(reloadTime_PalletEnd);
                    EmitWeaponViewEvent(WeaponEvent.Reload_PalletEnd);
                    weaponState = WeaponState.Reloading_PalletEnd;
                    return;
                }

            }
            // Otherwise should always reset to Idle
            weaponState = WeaponState.Idle;
        }
    }
    
    public void OnWeaponPrimaryAction(KeyPressState keyPressState)
    {
        primaryActionState = keyPressState;
    }
    
    public void OnWeaponSecondaryAction(KeyPressState keyPressState)
    {
        secondaryActionState = keyPressState;
    }
    
    public void DoWeaponSecondaryAction()
    {
        if(weaponCategory == WeaponCategory.Sniper)
        {
            if(!secondaryActionCooldown.IsOnCooldown() && secondaryActionState == KeyPressState.Holding)
            {
                secondaryActionCooldown.StartCooldown();
                
                if(weaponSecondaryState == WeaponSecondaryState.None)
                {
                    weaponSecondaryState = WeaponSecondaryState.Scoped;
                    EmitWeaponViewEvent(WeaponEvent.Scope);
                }
                else if(weaponSecondaryState == WeaponSecondaryState.Scoped)
                {
                    weaponSecondaryState = WeaponSecondaryState.None;
                    EmitWeaponViewEvent(WeaponEvent.UnScope);
                }
            }
        }
    }
    
    private void ResetWeaponSecondaryState()
    {
        if(weaponSecondaryState == WeaponSecondaryState.Scoped)
        {
            weaponSecondaryState = WeaponSecondaryState.None;
            EmitWeaponViewEvent(WeaponEvent.UnScope);
        }
    }
	
	public void DoWeaponReload()
	{
		if(weaponState != WeaponState.Idle || currentClip == clipSize)
			return;
        
        ResetWeaponSecondaryState();
        if(reloadType == WeaponReloadType.Clip)
        {
            EmitWeaponViewEvent(WeaponEvent.Reload);
            weaponState = WeaponState.Reloading;
            cooldownUntilIdle.StartCooldown(reloadTime);
        }
        else if (reloadType == WeaponReloadType.Pallet)
        {
            EmitWeaponViewEvent(WeaponEvent.Reload_PalletStart);
            weaponState = WeaponState.Reloading_PalletStart;
            cooldownUntilIdle.StartCooldown(reloadTime_PalletStart);
        }
	}
	
	public void DoWeaponDraw()
	{
        ResetWeaponSecondaryState();
        EmitWeaponViewEvent(WeaponEvent.Draw);
        UpdateAmmoDisplay();
        weaponState = WeaponState.Drawing;
        cooldownUntilIdle.StartCooldown(drawTime);
	}
    
	public void DoWeaponPrimaryAction()
	{
		if(!CanFire())	return;

		if(!isMelee)
		{
            EmitWeaponViewEvent(WeaponEvent.Shoot);
            ResetWeaponSecondaryState();
            currentClip--;
            weaponState = WeaponState.Shooting;
            cooldownUntilIdle.StartCooldown(shootInterval);
            
			ApplyRecoil();
            UpdateAmmoDisplay();
		}
	}
    
    public void FireWeapon(Vector3 dest)
    {
        weaponWorldModel.ShootProjectile(dest);
    }
            
    private void UpdateAmmoDisplay()
    {
        if(owner is FpsPlayer && owner.isLocalPlayer)
            FpsUiManager.Instance.OnWeaponAmmoUpdate(currentClip);
    }
	
	protected void ApplyRecoil()
	{
		Crosshair.Instance.DoLerp();
		PlayerContext.Instance.ShakeCamera();
	}
	
	private bool CanFire()
	{
		return weaponState == WeaponState.Idle && currentClip > 0;
	}
	
    private void EmitWeaponViewEvent(WeaponEvent evt)
    {
        if(owner != null && owner is FpsPlayer && owner.isLocalPlayer)
        {
            PlayerWeaponViewContext.Instance.EmitWeaponEvent(evt);
        }
    }
    
	void OnDestroy()
	{
        if(owner != null && owner is FpsPlayer && owner.isLocalPlayer)
        {
            PlayerContext.Instance.weaponPrimaryActionInputEvent.RemoveListener(OnWeaponPrimaryAction);
            PlayerContext.Instance.weaponSecondaryActionInputEvent.RemoveListener(OnWeaponSecondaryAction);
            PlayerContext.Instance.weaponReloadInputEvent.RemoveListener(DoWeaponReload);
        }
	}
	
	void OnEnable()
	{
        if(owner != null && owner is FpsPlayer && owner.isLocalPlayer)
        {
            PlayerContext.Instance.weaponPrimaryActionInputEvent.AddListener(OnWeaponPrimaryAction);
            PlayerContext.Instance.weaponSecondaryActionInputEvent.AddListener(OnWeaponSecondaryAction);
            PlayerContext.Instance.weaponReloadInputEvent.AddListener(DoWeaponReload);
        }
	}
	
	void OnDisable()
	{
        if(owner != null && owner is FpsPlayer && owner.isLocalPlayer)
        {
            PlayerContext.Instance.weaponPrimaryActionInputEvent.RemoveListener(OnWeaponPrimaryAction);
            PlayerContext.Instance.weaponSecondaryActionInputEvent.RemoveListener(OnWeaponSecondaryAction);
            PlayerContext.Instance.weaponReloadInputEvent.RemoveListener(DoWeaponReload);
        }
	}
    
    
    // ----------------- Archived melee raycast ---------------------- //
    /*
    private void CheckWeaponRaycastHit()
    {
        if(isMelee)
        {
            // ToDo: Single hit count on single enemy
            raycastHelper.m_LocalPosition = raycastSetting.localPosition;
            raycastHelper.m_HalfExtends = raycastSetting.halfExtends;
            raycastHelper.CheckPhysic();
            IEnumerable<Collider> colliders = raycastHelper.GetOverlapColliders();
            foreach(Collider c in colliders)
            {
                if(c.CompareTag(Constants.TAG_ENEMY))
                {
                    FpsEnemy fpsEnemy = c.GetComponent<FpsEnemy>();
                    DamageInfo dmgInfo = new DamageInfo(){
                        damage = 10,
                        bodyPart = BodyPart.Chest,
                        hitPoint = c.transform.position
                    };
                    
                    hitTargetEvent.Invoke(fpsEnemy.gameObject, dmgInfo);
                }
            }
        }
    }
    */
}
