﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class FpsPlayer
{
    
    public override void ProcessWeaponEventUpdate(WeaponEvent evt)
    {
        if(!isLocalPlayer)  return;
        
        // Send the event to weaponView for animations
        LocalPlayerContext.Instance.EmitWeaponEvent(evt);

        // Process events for other logic / UI
        if (evt == WeaponEvent.Shoot)
            OnWeaponFireEvent();
        else if (evt == WeaponEvent.Scope)
            OnWeaponScopeEvent();
        else if (evt == WeaponEvent.UnScope)
            OnWeaponUnScopeEvent();
        else if (evt == WeaponEvent.Reload)
            OnWeaponReloadEvent();
        else if (evt == WeaponEvent.AmmoUpdate)
            UpdateAmmoDisplay();
        else if (evt == WeaponEvent.OutOfAmmo)
            OnTriggerEmptyAmmo();
        else if (evt == WeaponEvent.Draw)
            OnWeaponDeploy();
    }

    private void OnWeaponDeploy()
    {
        FpsWeapon fpsWeapon = GetActiveWeapon();
        LocalPlayerContext.Instance.OnWeaponDeployEvent.Invoke(fpsWeapon);
    }

    private void OnWeaponReloadEvent()
    {
        // RpcReloadWeapon_Animation();
    }
    
    private void OnWeaponScopeEvent()
    {
        FpsUiManager.Instance.ToggleCrosshair(false);
        FpsUiManager.Instance.ToggleScope(true);
        LocalPlayerContext.Instance.ToggleScope(true);
        ecmCameraController.mouseHorizontalSensitivity = LocalPlayerSettingManager.Instance.GetMouseZoomedSpeed();
        ecmCameraController.mouseVerticalSensitivity = LocalPlayerSettingManager.Instance.GetMouseZoomedSpeed();
        weaponViewCamera.enabled = false;
    }
    
    private void OnWeaponUnScopeEvent()
    {
        FpsUiManager.Instance.ToggleCrosshair(true);
        FpsUiManager.Instance.ToggleScope(false);
        LocalPlayerContext.Instance.ToggleScope(false);
        ecmCameraController.mouseHorizontalSensitivity = LocalPlayerSettingManager.Instance.GetMouseSpeed();
        ecmCameraController.mouseVerticalSensitivity = LocalPlayerSettingManager.Instance.GetMouseSpeed();
        weaponViewCamera.enabled = true;
    }
    
    // Subscribe to weapon fire event, so when weapon is fired ( in fps view ) , 
    //   notify the server to do corresponding actions
    private void OnWeaponFireEvent()
    {
        Vector3 fromPos = Camera.main.transform.position;
        Vector3 forwardVec = Camera.main.transform.forward;

        if (GetActiveWeapon().weaponCategory != WeaponCategory.Melee)
            ApplyRecoil();

        LocalFireWeapon(fromPos , forwardVec);
    }
    
    private void UpdateAmmoDisplay()
    {
        FpsWeapon activeWeapon = GetActiveWeapon();
        if (activeWeapon.UseBackAmmo) 
            FpsUiManager.Instance.OnWeaponAmmoUpdate(activeWeapon.currentClip , GetBackAmmo(activeWeapon));
        else
            FpsUiManager.Instance.OnWeaponAmmoUpdate(activeWeapon.currentClip);
    }
    
    protected void ApplyRecoil()
    {
        Crosshair.Instance.DoLerp();
        LocalPlayerContext.Instance.ShakeCamera();

        if (!GetActiveWeapon().isSemiAuto)
        {
            playerController.weaponRecoilInput = GetActiveWeapon().currentRecoil;
        }
    }

    private void OnTriggerEmptyAmmo()
    {
        AudioClip emptyClip = WeaponAssetManager.Instance.weaponCommonResources.emptyAmmoClip;
        LocalPlayerContext.Instance.localPlayerAudioSource.PlayOneShot(emptyClip);
    }
}
