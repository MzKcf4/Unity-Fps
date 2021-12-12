using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// FpsCharacter.Weapon
public partial class FpsCharacter
{
    public readonly SyncList<string> syncWeaponNameInSlots = new SyncList<string>(){"" , "" , ""};
    public FpsWeapon[] weaponSlots = new FpsWeapon[Constants.WEAPON_SLOT_MAX];
    [HideInInspector] [SyncVar] public int activeWeaponSlot = -1;
    
    protected Transform weaponRootTransform;
    
    // Currently should be available to Local FpsPlayer only !
    public FpsWeaponView fpsWeaponView;
    public FpsWeaponWorldModel[] fpsWeaponWorldSlot = new FpsWeaponWorldModel[Constants.WEAPON_SLOT_MAX];
    
    private void Start_Weapon()
    {
        if(isClient && !isLocalPlayer)
        {
            // Process initial SyncList payload , load the weapon GameObjects for existing players.
            for (int index = 0; index < syncWeaponNameInSlots.Count; index++)
            {
                string weaponName = syncWeaponNameInSlots[index];
                if(string.IsNullOrWhiteSpace(weaponName))
                    continue;
                ClientGetWeapon(syncWeaponNameInSlots[index] , index);
            }
        }
    }
    
    private void Update_Weapon()
    {
        // Only process weapon if it's Server (bot) or LocalPlayer
        if(isLocalPlayer || isServer)
        {
            if(activeWeaponSlot != -1)
            {
                weaponSlots[activeWeaponSlot].ManualUpdate();
            }
        }
    }
    
    public FpsWeapon GetActiveWeapon()
    {
        return weaponSlots[activeWeaponSlot];
    }
    
    public void ReloadActiveWeapon()
    {
        weaponSlots[activeWeaponSlot].DoWeaponReload();
    }
    
    [Command]
    public void CmdReloadActiveWeapon()
    {
        ReloadActiveWeapon();
        RpcReloadActiveWeapon();
    }
    
    [ClientRpc]
    public void RpcReloadActiveWeapon()
    {
        RpcReloadWeapon_Animation();
    }
    
    [Command]
    public void CmdGetWeapon(string weaponName , int slot)
    {
        ServerGetWeapon(weaponName, slot);
        RpcGetWeapon(weaponName, slot);
    }
    
    [ClientRpc]
    public void RpcGetWeapon(string weaponName, int slot)
    {
        ClientGetWeapon(weaponName, slot);
        
        // Force switch weapon if it's main slot
        if(slot == 0)
        {
            if(isLocalPlayer)
                CmdSwitchWeapon(slot);
            else if (isServer)
                RpcSwitchWeapon(slot);
        }
    }
    
    [Command]
    public void CmdSwitchWeapon(int slot)
    {
        SwitchWeapon(slot);
        RpcSwitchWeapon(slot);
    }
    
    [ClientRpc]
    protected void RpcSwitchWeapon(int slot)
    {
        SwitchWeapon(slot);
    }
    
    
    public void FireWeapon()
    {
        
    }
    
    [ClientRpc]
    public void RpcFireWeapon()
    {
        AudioManager.Instance.PlaySoundAtPosition(GetActiveWeapon().GetShootSound() , fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform.position);
        if(!isLocalPlayer)
        {
            fpsWeaponWorldSlot[activeWeaponSlot].ShootProjectile();
            RpcFireWeapon_Animation();
        }
    }
    
    public void ServerGetWeapon(string weaponName , int slot)
    {
        // Server just need to set itself have FpsWeapon , without World/View model.
        FpsWeapon fpsWeapon = new FpsWeapon(weaponName);
        fpsWeapon.owner = this;
        
        weaponSlots[slot] = fpsWeapon;
        syncWeaponNameInSlots[slot] = fpsWeapon.weaponName;
    }
        
    // Client size gets the weapon , initialize fpsWeapon with models
    public void ClientGetWeapon(string weaponName, int slot)
    {
        FpsWeapon fpsWeapon = new FpsWeapon(weaponName);
        fpsWeapon.owner = this;
        weaponSlots[slot] = fpsWeapon;
        
        // ----------------World------------------
        WeaponResources weaponResource = WeaponAssetManager.Instance.GetWeaponResouce(weaponName);
        GameObject weaponWorldObject = GameObject.Instantiate(weaponResource.weaponWorldPrefab, weaponRootTransform);
        weaponWorldObject.transform.localPosition = Vector3.zero;
        weaponWorldObject.gameObject.SetActive(false);
        fpsWeaponWorldSlot[slot] = weaponWorldObject.GetComponent<FpsWeaponWorldModel>();
        if(isLocalPlayer)
        {
            // Change the local weapon model to camera's culling mask too !
            Utils.ChangeLayerRecursively(weaponWorldObject , Constants.LAYER_LOCAL_PLAYER_MODEL, true); 
        }
        
        // ----------- View ( Local Player only ) -----------------
        if(isLocalPlayer)
        {
            fpsWeaponView.AddViewWeaponNew(weaponResource.weaponViewPrefab, slot);
        }

    }
    
    public void SwitchWeapon(int slot)
    {
        int currActiveWeaponSlot = activeWeaponSlot;
        if(currActiveWeaponSlot != -1)
        {
            fpsWeaponWorldSlot[currActiveWeaponSlot].gameObject.SetActive(false);
        }
        
        activeWeaponSlot = slot;
        fpsWeaponWorldSlot[slot].gameObject.SetActive(true);
        
        if(isLocalPlayer)
        {
            fpsWeaponView.SwitchWeapon(slot);
            weaponSlots[slot].DoWeaponDraw();
        }
    }
    
    public virtual void ProcessWeaponEventUpdate(WeaponEvent evt)
    {
        
    }
    
}
