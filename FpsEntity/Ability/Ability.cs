using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Ability
{
    protected FpsCharacter owner;
    private MzAbilitySystem ownerAbilitySystem;
    protected CooldownSystem cooldownSystem;
    protected int duration = 5;
    protected int cooldown = 5;
    protected bool isActive = false;
    protected bool hasCooldown = true;

    private bool readyEventFired = true;
    private static int hitLayer = -1;

    public Ability(FpsCharacter owner)
    {
        this.owner = owner;
        this.ownerAbilitySystem = owner.GetComponent<MzAbilitySystem>();
        
        this.cooldownSystem = owner.GetComponent<CooldownSystem>();
        if (cooldownSystem == null) {
            Debug.LogError("No cooldown system found.");
        }

        hitLayer = hitLayer == -1 ? LayerMask.GetMask(Constants.LAYER_CHARACTER_RAYCAST_LAYER) : hitLayer;
    }

    public virtual void Update(float deltaTime) 
    {
        if (isActive && !cooldownSystem.IsOnCooldown(GetDurationKey())) {
            DeactivateAbility();
        }

        if (!readyEventFired && cooldownSystem.IsOnCooldown(GetCooldownKey())) {
            ownerAbilitySystem.AbilityReady(this);
        }
    }

    public void ActiviateAbility() 
    {
        if (cooldownSystem.IsOnCooldown(GetCooldownKey()))
            return;

        ApplyEffects();
        StartCooldown();
        isActive = true;
    }

    protected void StartCooldown() 
    {
        cooldownSystem.PutOnCooldown(GetCooldownKey(), duration + cooldown);
        cooldownSystem.PutOnCooldown(GetDurationKey(), duration);
        readyEventFired = false;
    }

    protected virtual void ApplyEffects() { }

    public void DeactivateAbility() 
    {
        isActive = false;
        RemoveEffects();
    }

    protected virtual void RemoveEffects() { }

    protected virtual void ApplySpeedModifier() { }

    protected virtual void RemoveSpeedModifier() { }

    public virtual void OnOwnerPreDealDamage(DamageInfo damageInfo) { }

    public virtual void OnOwnerPreTakeDamage(DamageInfo damageInfo) { }

    public virtual void OnOwnerPostDealDamage(DamageInfo damageInfo) { }

    public virtual void OnOwnerPostTakeDamage(DamageInfo damageInfo) { }

    protected bool isAbilityOnCooldown() 
    {
        return cooldownSystem.IsOnCooldown(GetCooldownKey());
    }

    protected void DoRadiusDamage(Vector3 position , FpsCharacter attacker, int dmg, float radius, TeamEnum targetTeam)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius, hitLayer);
        foreach (Collider c in colliders)
        {
            FpsCharacter character = c.GetComponent<FpsCharacter>();
            if (character == null || character.IsDead() || character.team != targetTeam)
                continue;

            character.TakeDamage(DamageInfo.AsDamageInfo(dmg, attacker, character));
        }
    }

    public abstract string GetID();

    public virtual string GetDescription() { return ""; }

    protected string GetDurationKey() 
    {
        return "duration-" + GetID();
    }

    protected string GetCooldownKey() 
    {
        return "cooldown-" + GetID();
    }
}

