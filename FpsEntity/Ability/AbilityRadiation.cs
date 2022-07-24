using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kit.Physic;
using UnityEngine;
public class AbilityRadiation : Ability
{
    private int radiationHitLayer;
    public static readonly string ID = "radiation";
    public AbilityRadiation(FpsCharacter owner) : base(owner)
    {
        radiationHitLayer = LayerMask.GetMask(Constants.LAYER_CHARACTER_RAYCAST_LAYER);
        duration = 1;
        cooldown = 1;
    }

    protected override void ApplyEffects() 
    {
        DoRadiationDamage();
    }

    private void DoRadiationDamage() 
    {
        Collider[] colliders = Physics.OverlapSphere(owner.transform.position, 5f, radiationHitLayer);
        foreach (Collider c in colliders) 
        { 
            FpsCharacter character = c.GetComponent<FpsCharacter>();
            if (character == null || character.team == TeamEnum.Monster)
                continue;

            character.TakeDamage(DamageInfo.AsDamageInfo(3, this.owner));
        }
    }

    public override string GetID()
    {
        return ID;
    }
}

