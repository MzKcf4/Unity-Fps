using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AbilityMetatronicSkill : Ability
{
    public static readonly string ID = "metatronicSkill";
    public static readonly string VFX_MARKER_NAME = "metatronicSkill";

    public AbilityMetatronicSkill(FpsCharacter owner) : base(owner)
    {
        duration = 10;
        cooldown = 10;
    }

    protected override void ApplyEffects()
    {
        owner.ServerPlayAnimationByKey(Constants.ACTION_KEY_SKILL, true, true, 1.05f);
        owner.PlayVfxInMarker(VFX_MARKER_NAME, Constants.EFFECT_NAME_LIGHTNING_MESH_GLOW, duration);
    }

    protected override void RemoveEffects()
    {
        
    }

    public override void OnOwnerPreTakeDamage(DamageInfo damageInfo)
    {
        if(isActive)
            damageInfo.damage = 0;
    }

    public override string GetID()
    {
        return ID;
    }
}

