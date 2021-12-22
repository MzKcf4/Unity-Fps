﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class FpsPlayer : FpsCharacter
{

    public override void ProcessWeaponEventUpdate(WeaponEvent evt)
    {
        if(!isLocalPlayer)  return;
        
        // Send the event to weaponView for animations
        PlayerWeaponViewContext.Instance.EmitWeaponEvent(evt);
        
        // Process events for other logic / UI
        if(evt == WeaponEvent.Shoot)
            OnWeaponFireEvent();
        else if (evt == WeaponEvent.Scope)
            OnWeaponScopeEvent();
        else if (evt == WeaponEvent.UnScope)
            OnWeaponUnScopeEvent();
        else if (evt == WeaponEvent.Reload)
            OnWeaponReloadEvent();
        else if (evt == WeaponEvent.AmmoUpdate)
            UpdateAmmoDisplay();
    }
        
    private void OnWeaponReloadEvent()
    {
        RpcReloadWeapon_Animation();
    }
    
    private void OnWeaponScopeEvent()
    {
        FpsUiManager.Instance.ToggleCrosshair(false);
        FpsUiManager.Instance.ToggleScope(true);
        LocalPlayerContext.Instance.ToggleScope(true);
        cameraController.cameraSpeed = (float)localPlayerSettingDto.mouseSpeedZoomed;
    }
    
    private void OnWeaponUnScopeEvent()
    {
        FpsUiManager.Instance.ToggleCrosshair(true);
        FpsUiManager.Instance.ToggleScope(false);
        LocalPlayerContext.Instance.ToggleScope(false);
        cameraController.cameraSpeed = (float)localPlayerSettingDto.mouseSpeed;
    }
    
    // Subscribe to weapon fire event, so when weapon is fired ( in fps view ) , 
    //   notify the server to do corresponding actions
    private void OnWeaponFireEvent()
    {
        Transform fromTransform = Camera.main.transform;
        
        Vector3 fromPos = Camera.main.transform.position;
        Vector3 forwardVec = Camera.main.transform.forward;
        
        ApplyRecoil();
        LocalFireWeapon(fromPos , forwardVec);
        // CmdFireWeapon(fromPos , forwardVec);
    }
    
    private void UpdateAmmoDisplay()
    {
        FpsUiManager.Instance.OnWeaponAmmoUpdate(GetActiveWeapon().currentClip);
    }
    
    protected void ApplyRecoil()
    {
        Crosshair.Instance.DoLerp();
        LocalPlayerContext.Instance.ShakeCamera();
    }
}