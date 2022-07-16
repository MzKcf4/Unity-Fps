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
    protected CooldownSystem cooldownSystem;
    protected int duration = 5;
    protected int cooldown = 5;
    protected bool isActive = false;

    public Ability(FpsCharacter owner)
    {
        this.owner = owner;
        this.cooldownSystem = owner.GetComponent<CooldownSystem>();
        if (cooldownSystem == null) {
            Debug.LogError("No cooldown system found.");
        }
    }

    public virtual void Update(float deltaTime) 
    {
        if (isActive && !cooldownSystem.IsOnCooldown(GetDurationKey())) {
            DeactivateAbility();
        }
    }

    public void ActiviateAbility() 
    {
        if (cooldownSystem.IsOnCooldown(GetCooldownKey()))
            return;

        ApplyEffects();
        cooldownSystem.PutOnCooldown(GetCooldownKey(), duration + cooldown);
        cooldownSystem.PutOnCooldown(GetDurationKey(), duration);
        isActive = true;
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

    public abstract string GetID();

    protected string GetDurationKey() 
    {
        return "duration-" + GetID();
    }

    protected string GetCooldownKey() 
    {
        return "cooldown-" + GetID();
    }

    private IEnumerator CountdownAbility(int duration)
    { 
        yield return new WaitForSeconds(duration);
        DeactivateAbility();
    }
}

