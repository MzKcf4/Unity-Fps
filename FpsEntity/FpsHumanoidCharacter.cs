using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using Animancer;
using NUnit.Framework.Interfaces;

// A Fps_Humanoid_Character should 
// -  have a Humanoid model , with weaponRoot component defined in hand for weapon world model
// -  be able to hold FpsWeapon
// -  perform weapon action with FpsWeapon

public partial class FpsHumanoidCharacter : FpsCharacter
{
    protected CooldownSystem cooldownSystem;
    protected static readonly string FOOTSTEP_COOLDOWN = "footstep-cooldown";
    protected static readonly float FOOTSTEP_UPDATE_INTERVAL = 0.35f;

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
        cooldownSystem = GetComponentInChildren<CooldownSystem>();
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

        PlayFootstep();
        Update_Weapon();
    }

    protected void PlayFootstep() 
    {
        if (cooldownSystem.IsOnCooldown(FOOTSTEP_COOLDOWN)) return;
        if (!characterMovement.isGrounded || isWalking) return;
        
        var velocity = GetMovementVelocity();
        if (velocity.magnitude > 0 || currentVelocity.magnitude > 0) 
        {
            AudioClip hurtSoundClip = Utils.GetRandomElement<AudioClip>(characterCommonResources.footstepList);
            audioSourceCharacter.PlayOneShot(hurtSoundClip);
            cooldownSystem.PutOnCooldown(FOOTSTEP_COOLDOWN, FOOTSTEP_UPDATE_INTERVAL);
        } 
        
    }
}

