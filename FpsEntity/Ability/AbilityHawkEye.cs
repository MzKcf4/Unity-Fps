using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AbilityHawkEye : Ability
{
    public static readonly string ID = "hawkeye";

    public AbilityHawkEye(FpsCharacter owner) : base(owner)
    {
        duration = 0;
        cooldown = 10;
    }

    public override void OnOwnerPreDealDamage(DamageInfo damageInfo)
    {
        if (isAbilityOnCooldown()) return;

        damageInfo.bodyPart = BodyPart.Head;
        StartCooldown();
    }

    public override string GetID()
    {
        return ID;
    }
}

