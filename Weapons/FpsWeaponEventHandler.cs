using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Local player only
public class FpsWeaponEventHandler
{
    private FpsPlayer fpsPlayer;
    private PlayerSettingDto localPlayerSettingDto;
    
    public FpsWeaponEventHandler(FpsPlayer fpsPlayer)
    {
        this.fpsPlayer = fpsPlayer;
        PlayerWeaponViewContext.Instance.onWeaponEventUpdate.AddListener(OnWeaponEventUpdate);
        localPlayerSettingDto = PlayerContext.Instance.playerSettingDto;
    }
    
    private void OnWeaponEventUpdate(WeaponEvent evt)
    {
        if(evt == WeaponEvent.Shoot)
            OnWeaponFireEvent();
        else if (evt == WeaponEvent.Scope)
            OnWeaponScopeEvent();
        else if (evt == WeaponEvent.UnScope)
            OnWeaponUnScopeEvent();
        else if (evt == WeaponEvent.AmmoUpdate)
            UpdateAmmoDisplay();
    }
    
    private void OnWeaponScopeEvent()
    {
        FpsUiManager.Instance.ToggleCrosshair(false);
        FpsUiManager.Instance.ToggleScope(true);
        PlayerContext.Instance.ToggleScope(true);
        fpsPlayer.cameraController.cameraSpeed = (float)localPlayerSettingDto.mouseSpeedZoomed;
    }
    
    private void OnWeaponUnScopeEvent()
    {
        FpsUiManager.Instance.ToggleCrosshair(true);
        FpsUiManager.Instance.ToggleScope(false);
        PlayerContext.Instance.ToggleScope(false);
        fpsPlayer.cameraController.cameraSpeed = (float)localPlayerSettingDto.mouseSpeed;
    }
    
    private void OnWeaponAmmoUpdateEvent()
    {
        UpdateAmmoDisplay();
    }
    
    // Subscribe to weapon fire event, so when weapon is fired ( in fps view ) , 
    //   notify the server to do corresponding actions
    private void OnWeaponFireEvent()
    {
        Transform fromTransform = Camera.main.transform;
        
        Vector3 fromPos = Camera.main.transform.position;
        Vector3 forwardVec = Camera.main.transform.forward;
        
        UpdateAmmoDisplay();
        ApplyRecoil();
        
        fpsPlayer.CmdFireWeapon(fromPos , forwardVec);
    }
    
    private void UpdateAmmoDisplay()
    {
        FpsUiManager.Instance.OnWeaponAmmoUpdate(fpsPlayer.GetActiveWeapon().currentClip);
    }
    
    protected void ApplyRecoil()
    {
        Crosshair.Instance.DoLerp();
        PlayerContext.Instance.ShakeCamera();
    }
}
