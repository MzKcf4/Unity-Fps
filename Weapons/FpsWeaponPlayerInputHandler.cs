using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For Local FpsPlayer only , act as a bridge between player's input and FpsWeapon's available actions
// This sends commands to the weapon regardless of weapon's state, but it's up to weapon to decide if will execute the command
public class FpsWeaponPlayerInputHandler
{
    private FpsPlayer fpsPlayer;
    
    public FpsWeaponPlayerInputHandler(FpsPlayer fpsPlayer)
    {
        this.fpsPlayer = fpsPlayer;
        
        LocalPlayerContext.Instance.weaponPrimaryActionInputEvent.AddListener(OnWeaponPrimaryAction);
        LocalPlayerContext.Instance.weaponSecondaryActionInputEvent.AddListener(OnWeaponSecondaryAction);
        LocalPlayerContext.Instance.weaponReloadInputEvent.AddListener(DoWeaponReload);
    }
    
    public void OnWeaponPrimaryAction(KeyPressState keyPressState)
    {
        fpsPlayer.GetActiveWeapon().primaryActionState = keyPressState;
    }
    
    public void OnWeaponSecondaryAction(KeyPressState keyPressState)
    {
        fpsPlayer.GetActiveWeapon().secondaryActionState = keyPressState;
    }
    
    public void DoWeaponReload()
    {
        fpsPlayer.GetActiveWeapon().DoWeaponReload();
    }
}
