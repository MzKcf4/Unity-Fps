using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FpsCharacterWeaponHandler
{
    public FpsCharacter fpsCharacter;
    public Transform weaponRootTransform;
    
    public void ServerGetWeapon(string weaponName , int slot)
    {
        // Server just need to set itself have FpsWeapon , without World/View model.
        FpsWeapon fpsWeapon = new FpsWeapon(weaponName);
        fpsWeapon.owner = fpsCharacter;
        
        fpsCharacter.weaponSlots[slot] = fpsWeapon;
        fpsCharacter.syncWeaponNameInSlots[slot] = fpsWeapon.weaponName;
    }
        
    // Client size gets the weapon , initialize fpsWeapon with models
    public void ClientGetWeapon(string weaponName, int slot)
    {
        FpsWeapon fpsWeapon = new FpsWeapon(weaponName);
        fpsWeapon.owner = fpsCharacter;
        fpsCharacter.weaponSlots[slot] = fpsWeapon;
        
        // ----------------World------------------
        WeaponResources weaponResource = WeaponAssetManager.Instance.GetWeaponResouce(weaponName);
        GameObject weaponWorldObject = GameObject.Instantiate(weaponResource.weaponWorldPrefab, weaponRootTransform);
        weaponWorldObject.transform.localPosition = Vector3.zero;
        weaponWorldObject.gameObject.SetActive(false);
        fpsCharacter.fpsWeaponWorldSlot[slot] = weaponWorldObject.GetComponent<FpsWeaponWorldModel>();
        if(fpsCharacter.isLocalPlayer)
        {
            // Change the local weapon model to camera's culling mask too !
            Utils.ChangeLayerRecursively(weaponWorldObject , Constants.LAYER_LOCAL_PLAYER_MODEL, true); 
        }
        
        // ----------- View ( Local Player only ) -----------------
        if(fpsCharacter.isLocalPlayer)
        {
            fpsCharacter.fpsWeaponView.AddViewWeaponNew(weaponResource.weaponViewPrefab, slot);
        }

    }
    
    public void SwitchWeapon(int slot)
    {
        int currActiveWeaponSlot = fpsCharacter.activeWeaponSlot;
        if(currActiveWeaponSlot != -1)
        {
            fpsCharacter.fpsWeaponWorldSlot[currActiveWeaponSlot].gameObject.SetActive(false);
        }
        
        fpsCharacter.activeWeaponSlot = slot;
        fpsCharacter.fpsWeaponWorldSlot[slot].gameObject.SetActive(true);
        
        if(fpsCharacter.isLocalPlayer)
        {
            fpsCharacter.fpsWeaponView.SwitchWeapon(slot);
            fpsCharacter.weaponSlots[slot].DoWeaponDraw();
        }
    }
    
}
