using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit.Physic;
using MoreMountains.Feedbacks;
using UnityEngine.Events;
using Animancer;


public class FpsWeapon : MonoBehaviour
{
	private static LayerMask MASK_HITBOX;
    private static LayerMask MASK_WALL;
    private static LayerMask MASK_HITBOX_AND_WALL;
    [SerializeField] public GameObject weaponViewPrefab;

	private RaycastHelper raycastHelper;
	// ---------- Custom events --------------- //
	// FpsPlayer listens to this event , weapon tells its owner that it hits something
	public DamageInfoEvent hitTargetEvent = new DamageInfoEvent();
	// ---------------------------------------- //    
    private ActionCooldown cooldownUntilIdle = new ActionCooldown();
	private WeaponState weaponState = WeaponState.Idle;
    public WeaponReloadType reloadType = WeaponReloadType.Clip;
    public WeaponCategory weaponCategory = WeaponCategory.Rifle;
	
	[SerializeField]
	private RaycastSetting raycastSetting;
	public bool isMelee = false;
	
	private MMFeedbacks muzzleFeedbacks;
	private KeyPressState primaryActionState = KeyPressState.Released;
	
    public bool isFirstPersonView = true;
    
    public FpsWeaponWorldModel weaponWorldModel;
	
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
    
    public FpsCharacter owner;
    
	void Awake()
	{
        FetchDataFromDb();
		muzzleFeedbacks = GetComponentInChildren<MMFeedbacks>();
        weaponWorldModel = GetComponent<FpsWeaponWorldModel>();
        if(Camera.main)
        {
            raycastHelper = Camera.main.GetComponent<RaycastHelper>();
        }
        currentClip = clipSize;
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
		MASK_HITBOX = LayerMask.GetMask(Constants.LAYER_HITBOX);
        MASK_WALL = LayerMask.GetMask(Constants.LAYER_GROUND);
        MASK_HITBOX_AND_WALL = (1 << MASK_HITBOX.value) | (1 << MASK_WALL.value);
        
        if(raycastHelper != null)
		    raycastHelper.m_LayerMask = MASK_HITBOX;
	}

	void Update()
	{
        if(owner.IsDead())  return;
        HandleCooldownInterrupt();
        HandleCooldown();
        if(owner is FpsPlayer && owner.isLocalPlayer)
        {
    		if(weaponState == WeaponState.Idle && primaryActionState != KeyPressState.Released)
    		{
    			DoWeaponPrimaryAction();
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
                Debug.Log(owner);
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
    
	
	public void DoWeaponReload()
	{
		if(weaponState != WeaponState.Idle || currentClip == clipSize)
			return;
        
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
		FpsUiManager.Instance.OnWeaponAmmoUpdate(currentClip);
        EmitWeaponViewEvent(WeaponEvent.Draw);
        
        weaponState = WeaponState.Drawing;
        cooldownUntilIdle.StartCooldown(drawTime);
	}
    
	public void DoWeaponPrimaryAction()
	{
		if(!CanFire())	return;

		if(!isMelee)
		{
            EmitWeaponViewEvent(WeaponEvent.Shoot);
            PlayerFireWeapon();
            for(int i = 0 ; i < palletPerShot ; i++)
            {
                CheckWeaponRaycastHit();    
            }
			ApplyRecoil();
			FpsUiManager.Instance.OnWeaponAmmoUpdate(currentClip);
		}
	}
    
    public void PlayerFireWeapon()
    {
        if(owner is FpsPlayer)
        {
            Vector3 aim = Utils.GetMouseAim();
            FireWeapon(aim);
        }
    }
    
    public void FireWeapon(Vector3 dest)
    {
        currentClip--;
        weaponState = WeaponState.Shooting;
        cooldownUntilIdle.StartCooldown(shootInterval);
        weaponWorldModel.ShootProjectile(dest);
    }
        
    // Should be used by Local Player only
	private void CheckWeaponRaycastHit()
	{
		if(!isMelee)
		{
            RayHitInfo rayHitInfo = Utils.GetMouseAimHitInfo(LayerMask.GetMask(Constants.LAYER_HITBOX , Constants.LAYER_GROUND), spread);
            if(rayHitInfo == null)
                return;
            
            GameObject objOnHit = rayHitInfo.hitObject;
            // Hits wall
            if( (1 << objOnHit.layer) == MASK_WALL.value)
            {
                LocalSpawnManager.Instance.SpawnBulletDecalFx(rayHitInfo.hitPoint , rayHitInfo.normal);
                return;
            }
            
            // Else should expect hitting hitbox
            FpsHitbox enemyHitBox = objOnHit.GetComponent<FpsHitbox>();
            FpsEntity hitEntity = enemyHitBox.fpsEntity;
                
            if(hitEntity is FpsCharacter)
            {
                TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                if(hitTeam == owner.team)   return;
            }
                
            if(hitEntity != null)
            {
                DamageInfo dmgInfo = DamageInfo.AsDamageInfo(this, enemyHitBox , rayHitInfo.hitPoint);    
                if(owner is FpsPlayer && owner.isLocalPlayer)
                    UiHitMarker.Instance.ShowHitMarker();
                    
                hitTargetEvent.Invoke(hitEntity.gameObject, dmgInfo);
                return;
            }
		}
		else
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
            PlayerContext.Instance.weaponReloadInputEvent.RemoveListener(DoWeaponReload);
        }
	}
	
	void OnEnable()
	{
        if(owner != null && owner is FpsPlayer && owner.isLocalPlayer)
        {
		    PlayerContext.Instance.weaponPrimaryActionInputEvent.AddListener(OnWeaponPrimaryAction);
            PlayerContext.Instance.weaponReloadInputEvent.AddListener(DoWeaponReload);
        }
	}
	
	void OnDisable()
	{
        if(owner != null && owner is FpsPlayer && owner.isLocalPlayer)
        {
		    PlayerContext.Instance.weaponPrimaryActionInputEvent.RemoveListener(OnWeaponPrimaryAction);
            PlayerContext.Instance.weaponReloadInputEvent.RemoveListener(DoWeaponReload);
        }
	}
}
