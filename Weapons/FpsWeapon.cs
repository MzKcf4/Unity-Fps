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
    
    [HideInInspector] public float rangeModifier = 1f;
    [HideInInspector] public float shootInterval = 0.1f;
    [HideInInspector] public int palletPerShot = 1;
    [HideInInspector] public float spreadInMove = 0f;
    
    private float spreadMin = 0f;
    private float spreadMax = 0.1f;
    public float currentSpread = 0.1f;
    private float spreadPerShot = 0.01f;
    private ActionCooldown cooldownUntilSpreadReduction = new ActionCooldown(){interval = 0.3f};
    private ActionCooldown spreadReductionCooldown = new ActionCooldown(){interval = 0.1f};
    private float spreadReductionInterval = 0.1f;
    private float spreadReductionPerTick = 0.03f;
    
    public int dmKillScore = 5;
    
    [HideInInspector] public FpsCharacter owner;
    
    public FpsWeapon(){}
    
    public FpsWeapon(string weaponName)
    {
        this.weaponName = weaponName;
        FetchDataFromDb();
        currentClip = clipSize;
        secondaryActionCooldown.interval = secondaryActionInterval;
        this.weaponResouces = WeaponAssetManager.Instance.GetWeaponResouce(weaponName);
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
        
        spreadMin = dbWeaponInfo.f_spread_min;
        spreadMax = dbWeaponInfo.f_spread_max;
        spreadPerShot = dbWeaponInfo.f_spread_per_shot;
        
        rangeModifier = dbWeaponInfo.f_range_modifier;
        spreadInMove = dbWeaponInfo.f_spread_move;
        
        dmKillScore = dbWeaponInfo.f_dm_kill_score;
    }
    
    public void Reset()
    {
        currentClip = clipSize;
        currentSpread = spreadMin;
        primaryActionState = KeyPressState.Released;
        secondaryActionState = KeyPressState.Released;
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
    }
    
    public void ManualUpdate()
    {
        if(owner.IsDead())  return;
        // cooldown with secondary action
        HandleCooldownInterrupt();
        HandleWeaponStateCooldown();
        HandleSpreadCooldown();
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
    
    private void HandleSpreadCooldown()
    {
        if(cooldownUntilSpreadReduction.CanExecuteAfterDeltaTime() &&  
            currentSpread > 0f && spreadReductionCooldown.CanExecuteAfterDeltaTime(true))
        {
            currentSpread -= spreadReductionPerTick;
            currentSpread = currentSpread < spreadMin ? spreadMin : currentSpread;
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
        EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);
        weaponState = WeaponState.Drawing;
        cooldownUntilIdle.StartCooldown(drawTime);
    }
    
    public void DoWeaponPrimaryAction()
    {
        if(!CanFire())  return;
        DoWeaponFire();
    }
    
    public void DoWeaponFire()
    {
        EmitWeaponViewEvent(WeaponEvent.Shoot);
        ResetWeaponSecondaryState();
        currentClip--;
        currentSpread += spreadPerShot;
        currentSpread = currentSpread > spreadMax ? spreadMax : currentSpread;
        cooldownUntilSpreadReduction.StartCooldown();
        weaponState = WeaponState.Shooting;
        EmitWeaponViewEvent(WeaponEvent.AmmoUpdate);
        cooldownUntilIdle.StartCooldown(shootInterval);
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
                
    public bool CanFire()
    {
        return weaponState == WeaponState.Idle && currentClip > 0;
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
}
