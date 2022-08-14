using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AbilityDemolition : Ability
{
    public static readonly string ID = "demolition";

    private int shotsRequired = 3;
    private int currentShots = 0;
    private float range = 3.5f;

    public AbilityDemolition(FpsCharacter owner) : base(owner)
    {
        hasCooldown = false;
    }

    public override void OnOwnerPreDealDamage(DamageInfo damageInfo)
    {
        if (!damageInfo.isFromWeapon)
            return;

        if (++currentShots >= shotsRequired)
        {
            currentShots = 0;
            DoDemolition(damageInfo.hitPoint, damageInfo.damage);
        }
    }

    private void DoDemolition(Vector3 position, int damage) 
    {
        EffectManager.Instance.RpcPlayEffect(Constants.EFFECT_NAME_FIRE_SMALL_EXPLOSION, position, 3f);
        DoRadiusDamage(position, owner, damage, range, TeamEnum.Monster);
    }

    public override string GetID()
    {
        return ID;
    }
}

