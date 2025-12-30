using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class CharacterEffect
{
    protected FpsCharacter owner;
    protected CooldownSystem cooldownSystem;
    protected int duration = 5;
    protected int cooldown = 5;
    protected bool isActive = false;

    public CharacterEffect(FpsCharacter owner)
    {
        this.owner = owner;
        this.cooldownSystem = owner.GetComponent<CooldownSystem>();
        if (cooldownSystem == null)
        {
            Debug.LogError("No cooldown system found.");
        }
    }

    public virtual void Update(float deltaTime)
    {
        
    }
}

