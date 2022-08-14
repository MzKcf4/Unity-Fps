using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AbilitySafeguard : Ability
{
    public static readonly string ID = "safeguard";

    public AbilitySafeguard(FpsCharacter owner) : base(owner)
    {
        duration = 5;
        cooldown = 90;
    }

    public override void OnOwnerPostTakeDamage(DamageInfo damageInfo)
    {
        if (isAbilityOnCooldown()) return;
        if (owner.health > damageInfo.damage) return;

        owner.health = owner.maxHealth / 2;
        isActive = true;
        StartCooldown();
        owner.SetGodmode(true);
    }

    protected override void RemoveEffects()
    {
        owner.SetGodmode(false);
    }

    public override string GetID()
    {
        return ID;
    }
}
