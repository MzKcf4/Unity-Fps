using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using Kit.Physic;

// FpsHumanoidCharacter.Weapon
public abstract partial class FpsHumanoidCharacter
{
    public readonly SyncList<string> syncWeaponNameInSlots = new SyncList<string>() { "", "", "" };
    // ** ----- Just note that fpsWeapon is NOT sync by server ------ **
    public FpsWeapon[] weaponSlots = new FpsWeapon[Constants.WEAPON_SLOT_MAX];
    [HideInInspector] [SyncVar] public int activeWeaponSlot = -1;

    protected Transform weaponRootTransform;

    // Currently should be available to Local FpsPlayer only !
    public FpsWeaponView fpsWeaponView;
    public FpsWeaponWorldModel[] fpsWeaponWorldSlot = new FpsWeaponWorldModel[Constants.WEAPON_SLOT_MAX];
    [SerializeField] public RaycastHelper meleeRaycastHelper;

    private void Start_Weapon()
    {
        if (meleeRaycastHelper == null)
        {
            meleeRaycastHelper = GetComponentInChildren<RaycastHelper>();
        }

        if (isClient && !isLocalPlayer)
        {
            // Process initial SyncList payload , load the weapon GameObjects for existing players.
            for (int index = 0; index < syncWeaponNameInSlots.Count; index++)
            {
                string weaponName = syncWeaponNameInSlots[index];
                if (string.IsNullOrWhiteSpace(weaponName))
                    continue;
                ClientGetWeapon(syncWeaponNameInSlots[index], index);
            }
        }
    }

    private void Update_Weapon()
    {
        // Only process weapon if it's Server (bot) or LocalPlayer
        if (isLocalPlayer || isServer)
        {
            if (activeWeaponSlot != -1)
            {
                weaponSlots[activeWeaponSlot].ManualUpdate();
            }
        }
    }

    public FpsWeapon GetActiveWeapon()
    {
        if (activeWeaponSlot < 0)
            return null;

        return weaponSlots[activeWeaponSlot];
    }

    public void ReloadActiveWeapon()
    {
        weaponSlots[activeWeaponSlot].DoWeaponReload();
    }

    public bool HasWeapon(string weaponName)
    {
        foreach (FpsWeapon fpsWeapon in weaponSlots)
        {
            if (fpsWeapon != null && weaponName.Equals(fpsWeapon.weaponName))
                return true;
        }
        return false;
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

    public void LocalCmdGetWeapon(string weaponName, int slot)
    {
        if (isLocalPlayer)
            CmdGetWeapon(weaponName, slot);
    }

    [Command]
    public void CmdGetWeapon(string weaponName, int slot)
    {
        ServerCmdGetWeapon(weaponName, slot);
    }

    [Server]
    public void ServerCmdGetWeapon(string weaponName, int slot)
    {
        ServerGetWeapon(weaponName, slot);
        RpcGetWeapon(weaponName, slot);
    }

    [ClientRpc]
    public void RpcGetWeapon(string weaponName, int slot)
    {
        ClientGetWeapon(weaponName, slot);

        // Force switch weapon if it's main slot
        if (slot == 0)
        {
            if (isLocalPlayer)
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

    protected virtual void SwitchWeapon(int slot)
    {
        int currActiveWeaponSlot = activeWeaponSlot;

        if (currActiveWeaponSlot != -1)
        {
            if (weaponSlots[currActiveWeaponSlot] != null)
                weaponSlots[currActiveWeaponSlot].ResetActionState();

            if (fpsWeaponWorldSlot[currActiveWeaponSlot] != null)
                fpsWeaponWorldSlot[currActiveWeaponSlot].gameObject.SetActive(false);
        }

        activeWeaponSlot = slot;

        if (fpsWeaponWorldSlot[slot] != null)
            fpsWeaponWorldSlot[slot].gameObject.SetActive(true);

        if (isLocalPlayer)
        {
            fpsWeaponView.SwitchWeapon(slot);
            weaponSlots[slot].DoWeaponDraw();
        }
    }


    public void FireWeapon()
    {

    }

    [ClientRpc]
    public void RpcFireWeapon()
    {
        if (activeWeaponSlot == -1 || fpsWeaponWorldSlot[activeWeaponSlot] == null) return;
        Vector3 shootSoundPos = fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform == null ? fpsWeaponWorldSlot[activeWeaponSlot].transform.position
                                                                                             : fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform.position;

        audioSourceWeapon.Stop();
        audioSourceWeapon.PlayOneShot(GetActiveWeapon().GetShootSound());

        if (!isLocalPlayer)
        {
            fpsWeaponWorldSlot[activeWeaponSlot].ShootProjectile();
            RpcFireWeapon_Animation();
        }
    }

    public void ServerGetWeapon(string weaponName, int slot)
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
        // Check if the slot already has weapon , if yes , destroy the existing weapon
        if (fpsWeaponWorldSlot[slot] != null)
        {
            Destroy(fpsWeaponWorldSlot[slot].gameObject, 0.5f);
        }

        WeaponResources weaponResource = StreamingAssetManager.Instance.GetWeaponResouce(weaponName);
        GameObject weaponWorldObject = GameObject.Instantiate(weaponResource.weaponWorldPrefab, weaponRootTransform);
        weaponWorldObject.transform.localPosition = Vector3.zero;
        weaponWorldObject.gameObject.SetActive(false);
        fpsWeaponWorldSlot[slot] = weaponWorldObject.GetComponent<FpsWeaponWorldModel>();
        if (isLocalPlayer)
        {
            // Change the local weapon model to camera's culling mask too !
            Utils.ChangeLayerRecursively(weaponWorldObject, Constants.LAYER_LOCAL_PLAYER_MODEL, true);
        }

        // ----------- View ( Local Player only ) -----------------
        if (isLocalPlayer)
        {
            fpsWeaponView.AddViewWeaponNew(weaponResource, slot);
        }
    }

    public virtual void ProcessWeaponEventUpdate(WeaponEvent evt)
    {

    }

    protected void ResetAllWeapons()
    {
        foreach (FpsWeapon weapon in weaponSlots)
        {
            if (weapon == null)
                continue;
            weapon.Reset();
        }
    }
}
