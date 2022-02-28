using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The logical layer (brain) of a weapon , updated by owner in Update()
// It shouldn't care about the view/world model

// Weapons should be able to perform following actions : 
//      Draw , Shoot , Reload  ,  Scope (if possible)

// The actions are either triggered by local player input , or direct call by bot

// *** Note that weapons variables are managed by players locally ! ***
public class FpsWeapon
{
    public string weaponName;
    
    private WeaponResources weaponResouces;
    
    private ActionCooldown cooldownUntilIdle = new ActionCooldown();
    private float secondaryActionInterval = 0.2f;
    private ActionCooldown secondaryActionCooldown = new ActionCooldown();
    
    private WeaponState weaponState = WeaponState.Idle;
    private WeaponSecondaryState weaponSecondaryState = WeaponSecondaryState.None;
    
    public WeaponReloadType reloadType = WeaponReloadType.Clip;
    public WeaponCategory weaponCategory = WeaponCategory.Rifle;
    
    public KeyPressState primaryActionState = KeyPressState.Released;
    public KeyPressState secondaryActionState = KeyPressState.Released;

    public int currentClip;
    
    [HideInInspector] public int damage = 20;
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
    [HideInInspector] public int palletPerShot = 1;
    [HideInInspector] public float spreadInMove = 0f;
    
    public float spread = 0f;
    public float recoil = 0f;
    public float currentRecoil = 0f;
    public float cameraShake = 0f;
    public int dmKillScore = 5;
    
    [HideInInspector] public FpsHumanoidCharacter owner;
    public string displayName = "";
    
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
        recoil = dbWeaponInfo.f_recoil;
        cameraShake = dbWeaponInfo.f_camera_shake;

        rangeModifier = dbWeaponInfo.f_range_modifier;
        spreadInMove = dbWeaponInfo.f_spread_move;
        
        dmKillScore = dbWeaponInfo.f_dm_kill_score;
        isSemiAuto = dbWeaponInfo.f_is_semi_auto;

        displayName = dbWeaponInfo.f_display_name;
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
                currentClip = clipSize;
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
                currentClip++;
                EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);
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

    private void CheckAmmoAndAutoReload()
    {
        if (!isOutOfAmmo() || primaryActionState != KeyPressState.Released || weaponState != WeaponState.Idle)
            return;

        DoWeaponReload();
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
        if (weaponCategory == WeaponCategory.Melee) return;

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
        EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);
        weaponState = WeaponState.Drawing;
        cooldownUntilIdle.StartCooldown(drawTime);
    }
    
    public void DoWeaponPrimaryAction()
    {
        if(!CanTriggerFire())  
            return;

        if (isOutOfAmmo())
            DoWeaponFireOutOfAmmo();
        else
            DoWeaponFire();
        
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
    
    // Temp way for bot to use weapon's cooldown
    public void DoCooldownFromShoot()
    {
        weaponState = WeaponState.Shooting;
        cooldownUntilIdle.StartCooldown(shootInterval);
    }
    
    public void FireWeapon(Vector3 dest)
    {
        // weaponWorldModel.ShootProjectile(dest);
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

    private bool isOutOfAmmo()
    {
        return currentClip <= 0 && weaponCategory != WeaponCategory.Melee;
    }
}
