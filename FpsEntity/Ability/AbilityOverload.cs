using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AbilityOverload : Ability
{
    public static readonly string ID = "overload";

    private int minDmg = 300;
    private float range = 5f;

    public AbilityOverload(FpsCharacter owner) : base(owner)
    {
        hasCooldown = false;
    }

    public override void OnOwnerPostDealDamage(DamageInfo damageInfo)
    {
        if (!damageInfo.isFromWeapon || damageInfo.victim == null)
            return;

        int overflowDmg = CalculateOverflowDamage(damageInfo);
        if (overflowDmg < minDmg) return;
        
        int halfDmg = overflowDmg / 2;
        EffectManager.Instance.RpcPlayEffect(Constants.EFFECT_NAME_FIRE_LARGE_EXPLOSION, damageInfo.hitPoint, 3f);
        DoRadiusDamage(damageInfo.hitPoint, owner, halfDmg, range, TeamEnum.Monster);
    }

    private int CalculateOverflowDamage(DamageInfo damageInfo) 
    {
        return damageInfo.damage - damageInfo.victim.health;
    }

    public override string GetID()
    {
        return ID;
    }
}
