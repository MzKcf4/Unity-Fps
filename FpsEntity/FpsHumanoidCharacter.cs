using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using Animancer;

// A Fps_Humanoid_Character should 
// -  have a Humanoid model , with weaponRoot component defined in hand for weapon world model
// -  be able to hold FpsWeapon
// -  perform weapon action with FpsWeapon

public partial class FpsHumanoidCharacter : FpsCharacter
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        characterCommonResources = WeaponAssetManager.Instance.GetCharacterCommonResources();
    }

    protected override void Awake()
    {
        base.Awake();
        
    }

    protected override void Start()
    {
        base.Start();
        weaponRootTransform = GetComponentInChildren<CharacterWeaponRoot>().transform;
        Start_Weapon();
    }

    [Server]
    public void ShootAtTarget()
    {
        if (GetActiveWeapon() == null)
            return;

        GetActiveWeapon().DoWeaponFire();
    }

    [Server]
    public override void Respawn()
    {
        base.Respawn();
        if (activeWeaponSlot >= 0)
            RpcSwitchWeapon(activeWeaponSlot);

        ResetAllWeapons();
    }

    [ClientRpc]
    public override void RpcRespawn()
    {
        base.RpcRespawn();
        ResetAllWeapons();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (IsDead()) return;

        Update_Weapon();
    }

}

