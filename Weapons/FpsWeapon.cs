using UnityEngine;

// The logical layer (brain) of a weapon , updated by owner in Update()
// It shouldn't care about the view/world model

// Weapons should be able to perform following actions : 
//      Draw , Shoot , Reload  ,  Scope (if possible)

// The actions are either triggered by local player input , or direct call by bot

// *** Note that weapons variables are managed by players locally ! ***
public class FpsWeapon
{
    private static int SPREAD_LEVELS = 6;
    private static float SPREAD_COOLDOWN = 0.7f;
    public string weaponName;
    public bool UseBackAmmo { get { return useBackAmmo; } }
    
    private WeaponResources weaponResouces;
    
    private ActionCooldown cooldownUntilIdle = new ActionCooldown();
    private float secondaryActionInterval = 0.2f;
    private ActionCooldown secondaryActionCooldown = new ActionCooldown();
    private ActionCooldown spreadCooldown = new ActionCooldown();

    
    private WeaponState weaponState = WeaponState.Idle;
    private WeaponSecondaryState weaponSecondaryState = WeaponSecondaryState.None;
    
    public WeaponReloadType reloadType = WeaponReloadType.Clip;
    public WeaponCategory weaponCategory = WeaponCategory.Rifle;
    
    public KeyPressState primaryActionState = KeyPressState.Released;
    public KeyPressState secondaryActionState = KeyPressState.Released;

    public int currentClip;
    
    public int damage = 20;
    public int damageSecondary = 60;
    // power = 1 means can penetrate 1 wall
    public int penetrationPower = 1;

    private int clipSize = 30;
    private float reloadTime = 3f;
    private float reloadTime_PalletStart = 0.2f;
    private float reloadTime_PalletInsert = 0.2f;
    private float reloadTime_PalletEnd = 0.2f;
    private float drawTime = 2f;

    // Shoot mode
    public bool isSemiAuto = false;
    private bool isPrimayActionWatingRelease = false;
    
    [HideInInspector] public float rangeModifier = 1f;
    [HideInInspector] public float shootInterval = 0.1f;
    [HideInInspector] public float shootIntervalSecondary = 0.1f;
    [HideInInspector] public int palletPerShot = 1;
    [HideInInspector] public float spreadInMove = 0f;
    
    public float spread = 0f;
    private float[] spreadPerLevel = new float[SPREAD_LEVELS];
    private int spreadLevel;

    public float recoil = 0f;
    public float currentRecoil = 0f;
    public float cameraShake = 0f;
    public int dmKillScore = 5;
    public float moveSpeed = 6f;
    
    [HideInInspector] public FpsHumanoidCharacter owner;
    public string displayName = "";
    private bool useBackAmmo = false;

    public bool isMelee;

    public FpsWeapon(){}
    
    public FpsWeapon(string weaponName)
    {
        this.weaponName = weaponName;
        FetchDataFromDb();
        currentClip = clipSize;
        secondaryActionCooldown.interval = secondaryActionInterval;
        this.weaponResouces = StreamingAssetManager.Instance.GetWeaponResouce(weaponName);
    }
        
    private void FetchDataFromDb()
    {
        E_weapon_info dbWeaponInfo = E_weapon_info.GetEntity(weaponName);
        
        damage = dbWeaponInfo.f_base_damage;
        damageSecondary = dbWeaponInfo.f_secondary_damage;
        penetrationPower = dbWeaponInfo.f_penetration;
        clipSize = dbWeaponInfo.f_clip_size;
        shootInterval = dbWeaponInfo.f_shoot_interval;
        shootIntervalSecondary = dbWeaponInfo.f_shoot_interval_secondary;
        reloadType = dbWeaponInfo.f_reload_type;
        weaponCategory = dbWeaponInfo.f_category;
        reloadTime = dbWeaponInfo.f_reload_time;
        reloadTime_PalletStart = dbWeaponInfo.f_reload_time_pallet_start;
        reloadTime_PalletInsert = dbWeaponInfo.f_reload_time_pallet_insert;
        reloadTime_PalletEnd = dbWeaponInfo.f_reload_time_pallet_end;
        drawTime = dbWeaponInfo.f_draw_time;
        palletPerShot = dbWeaponInfo.f_pallet_per_shot;
        
        spread = dbWeaponInfo.f_spread;
        float spreadPerLv = spread / (SPREAD_LEVELS-1);
        for (int i = 0; i < SPREAD_LEVELS; i++)
        {
            spreadPerLevel[i] = i * spreadPerLv;
        }

        recoil = dbWeaponInfo.f_recoil;
        cameraShake = dbWeaponInfo.f_camera_shake;

        rangeModifier = dbWeaponInfo.f_range_modifier;
        spreadInMove = dbWeaponInfo.f_spread_move;
        moveSpeed = dbWeaponInfo.f_speed;

        dmKillScore = dbWeaponInfo.f_dm_kill_score;
        isSemiAuto = dbWeaponInfo.f_is_semi_auto;

        displayName = dbWeaponInfo.f_display_name;

        if (CoreGameManager.Instance.GameMode == GameModeEnum.Monster)
        {
            if(dbWeaponInfo.f_monster_damage > 0)
                damage = dbWeaponInfo.f_monster_damage;
        }

        useBackAmmo = weaponCategory != WeaponCategory.Melee && CoreGameManager.Instance.GameMode == GameModeEnum.Monster;
        isMelee = dbWeaponInfo.f_category == WeaponCategory.Melee;
    }
    
    public void Reset()
    {
        // Fetch again as there could be live-update
        FetchDataFromDb();
        currentClip = clipSize;
        ResetActionState();
    }
    
    public void ResetActionState()
    {
        primaryActionState = KeyPressState.Released;
        secondaryActionState = KeyPressState.Released;
        
        if(weaponSecondaryState == WeaponSecondaryState.Scoped)
        {
            weaponSecondaryState = WeaponSecondaryState.None;
            EmitWeaponViewEvent(WeaponEvent.UnScope);
        }
        isPrimayActionWatingRelease = false;
        currentRecoil = 0f;
    }
    
    public void ManualUpdate()
    {
        if(owner.IsDead())  return;
        // cooldown with secondary action
        HandleSpreadCooldown();
        HandleCooldownInterrupt();
        HandleWeaponStateCooldown();
        if(owner is FpsPlayer && owner.isLocalPlayer)
        {
            CheckAmmoAndAutoReload();
            if (weaponState == WeaponState.Idle && primaryActionState != KeyPressState.Released)
            {
                DoWeaponPrimaryAction();
            }
            if(weaponState == WeaponState.Idle && secondaryActionState != KeyPressState.Released)
            {
                DoWeaponSecondaryAction();
            }
        }
    }

    private void HandleSpreadCooldown()
    {
        if (spreadCooldown.IsOnCooldown(true) || spreadLevel <= 0) return;

        spreadCooldown.ReduceCooldown();
        spreadLevel--;
        if(spreadLevel > 0)
            spreadCooldown.StartCooldown(SPREAD_COOLDOWN);
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
    
    private void HandleWeaponStateCooldown()
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
                RefillAmmo(false);
                EmitWeaponViewEvent(WeaponEvent.Reload_End);
                EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);
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
                RefillAmmo(true);
                EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);
                if(currentClip != clipSize && OwnerHasBackAmmo())
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

    private void RefillAmmo(bool isPalletReload) 
    {
        if (!useBackAmmo)
            currentClip = isPalletReload ? currentClip + 1 : clipSize;
        else
        { 
            string ammoKey = weaponCategory.ToString();
            int requiredAmmo = isPalletReload ? 1 : clipSize - currentClip;
            int fillableAmmo = Mathf.Min(requiredAmmo, owner.BackAmmoInfo[ammoKey]);
            currentClip += fillableAmmo;
            owner.BackAmmoInfo[ammoKey] -= fillableAmmo;
        }
    }

    private void CheckAmmoAndAutoReload()
    {
        if (!IsOutOfAmmo() || primaryActionState != KeyPressState.Released || weaponState != WeaponState.Idle || !OwnerHasBackAmmo())
            return;

        DoWeaponReload();
    }

    private bool OwnerHasBackAmmo() 
    {
        if (useBackAmmo)
        {
            string ammoKey = weaponCategory.ToString();
            if (!owner.BackAmmoInfo.ContainsKey(ammoKey) || owner.BackAmmoInfo[ammoKey] <= 0)
                return false;
        }
        return true;
    }

    public void UpdateWeaponPrimaryActionState(KeyPressState keyPressState)
    {
        primaryActionState = keyPressState;
        if (primaryActionState == KeyPressState.Released)
        {
            // Reset the flag when received 'released' input
            isPrimayActionWatingRelease = false;
            currentRecoil = 0f;
        }
    }

    public void UpdateWeaponSecondaryActionState(KeyPressState keyPressState)
    {
        secondaryActionState = keyPressState;
    }
    
    public void DoWeaponSecondaryAction()
    {
        if (weaponCategory == WeaponCategory.Sniper)
        {
            if (!secondaryActionCooldown.IsOnCooldown() && secondaryActionState == KeyPressState.Holding)
            {
                secondaryActionCooldown.StartCooldown();

                if (weaponSecondaryState == WeaponSecondaryState.None)
                {
                    weaponSecondaryState = WeaponSecondaryState.Scoped;
                    EmitWeaponViewEvent(WeaponEvent.Scope);
                }
                else if (weaponSecondaryState == WeaponSecondaryState.Scoped)
                {
                    weaponSecondaryState = WeaponSecondaryState.None;
                    EmitWeaponViewEvent(WeaponEvent.UnScope);
                }
            }
        }
        else if (weaponCategory == WeaponCategory.Melee)
        {
            if (secondaryActionCooldown.IsOnCooldown()) return;

            secondaryActionCooldown.StartCooldown();
            DoWeaponSecondaryFire();
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
    
    public bool DoWeaponReload()
    {
        if (weaponCategory == WeaponCategory.Melee) return false;

        if(weaponState != WeaponState.Idle || weaponState == WeaponState.Reloading || currentClip == clipSize || !OwnerHasBackAmmo())
            return false;

        ResetWeaponSecondaryState();
        if(reloadType == WeaponReloadType.Clip)
        {
            EmitWeaponViewEvent(WeaponEvent.Reload);
            weaponState = WeaponState.Reloading;
            cooldownUntilIdle.StartCooldown(reloadTime);
            return true;
        }
        else if (reloadType == WeaponReloadType.Pallet)
        {
            EmitWeaponViewEvent(WeaponEvent.Reload_PalletStart);
            weaponState = WeaponState.Reloading_PalletStart;
            cooldownUntilIdle.StartCooldown(reloadTime_PalletStart);
            return true;
        }
        return false;
    }
    
    public void DoWeaponDraw()
    {
        ResetWeaponSecondaryState();
        EmitWeaponViewEvent(WeaponEvent.Draw);
        EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);
        weaponState = WeaponState.Drawing;
        cooldownUntilIdle.StartCooldown(drawTime);
    }
    
    public bool DoWeaponPrimaryAction()
    {
        if(!CanTriggerFire())  
            return false;

        if (IsOutOfAmmo())
        {
            DoWeaponFireOutOfAmmo();
            return false;
        }
        else
            DoWeaponFire();
        return true;
    }

    public void DoWeaponFireOutOfAmmo()
    {
        ResetWeaponSecondaryState();
        cooldownUntilIdle.StartCooldown(shootInterval);
        weaponState = WeaponState.Shooting;
        // Return here to keep steady sound of empty clip
        EmitWeaponViewEvent(WeaponEvent.OutOfAmmo);
        return;
    }

    public void DoWeaponFire()
    {
        EmitWeaponViewEvent(WeaponEvent.Shoot);

        if(isSemiAuto)
            ResetWeaponSecondaryState();

        cooldownUntilIdle.StartCooldown(shootInterval);

        weaponState = WeaponState.Shooting;
        if (isSemiAuto)
        {
            isPrimayActionWatingRelease = true;
        }

        if (weaponCategory == WeaponCategory.Melee) return;

        currentClip--;
        spreadLevel = spreadLevel >= SPREAD_LEVELS-1 ? spreadLevel : ++spreadLevel;
        spreadCooldown.StartCooldown(SPREAD_COOLDOWN);

        IncreaseRecoil();
        EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);
        
    }

    public void DoWeaponSecondaryFire()
    {
        EmitWeaponViewEvent(WeaponEvent.ShootSecondary);

        if (isSemiAuto)
            ResetWeaponSecondaryState();

        cooldownUntilIdle.StartCooldown(shootIntervalSecondary);

        weaponState = WeaponState.Shooting;
        if (isSemiAuto)
        {
            isPrimayActionWatingRelease = true;
        }

        if (weaponCategory == WeaponCategory.Melee) return;

        currentClip--;
        IncreaseRecoil();
        EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);

    }

    private void IncreaseRecoil()
    {
        if (isSemiAuto) return;
        if (currentRecoil >= recoil)    return;

        currentRecoil += recoil / 4;

        if (currentRecoil > recoil)
            currentRecoil = recoil;
    }
                        
    public bool CanTriggerFire()
    {
        if (weaponCategory == WeaponCategory.Melee)
            return weaponState == WeaponState.Idle;
        else
        {
            if (isSemiAuto && isPrimayActionWatingRelease) 
                return false;
            return weaponState == WeaponState.Idle;
        }
            
    }
    
    private void EmitWeaponViewEvent(WeaponEvent evt)
    {
        if(owner != null)
        {
            owner.ProcessWeaponEventUpdate(evt);
        }
    }
    
    public AudioClip GetShootSound()
    {
        return weaponResouces.dictWeaponSounds[Constants.WEAPON_SOUND_FIRE];
    }
    
    public Vector3 GetMuzzlePosition()
    {
        return Vector3.zero;
        // return weaponWorldModel.muzzleTransform.position;
    }

    public float GetEffectiveSpread()
    {
        if (weaponCategory == WeaponCategory.Shotgun)
            return spread;

        return spreadPerLevel[spreadLevel];
    }

    public bool IsOutOfAmmo()
    {
        return currentClip <= 0 && weaponCategory != WeaponCategory.Melee;
    }

    public bool IsReloading()
    {
        return weaponState == WeaponState.Reloading || weaponState == WeaponState.Reloading_PalletStart || weaponState == WeaponState.Reloading_PalletInsert || weaponState == WeaponState.Reloading_PalletEnd;
    }
}
