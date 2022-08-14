using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Physic;
using UnityEngine;
public class AbilityRadiation : Ability
{
    public static readonly string ID = "radiation";
    public AbilityRadiation(FpsCharacter owner) : base(owner)
    {
        duration = 1;
        cooldown = 1;
    }

    protected override void ApplyEffects() 
    {
        DoRadiationDamage();
    }

    private void DoRadiationDamage() 
    {
        DoRadiusDamage(owner.transform.position, owner, 3, 6f, TeamEnum.Blue);
    }

    public override string GetID()
    {
        return ID;
    }
}

