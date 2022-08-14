using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AbilityFactory
{
    public static List<AbilityEnum> AbilitiesForPlayer = new List<AbilityEnum>() {
        AbilityEnum.HawkEye,
        AbilityEnum.Demolition,
        AbilityEnum.Overload,
        AbilityEnum.Safeguard
    };

    public static Ability GetAbility(AbilityEnum abilityEnum, FpsCharacter owner)
    {
        switch (abilityEnum) 
        {
            case AbilityEnum.HawkEye:
                return new AbilityHawkEye(owner);
            case AbilityEnum.Demolition:
                return new AbilityDemolition(owner);
            case AbilityEnum.Overload:
                return new AbilityOverload(owner);
            case AbilityEnum.Safeguard:
                return new AbilitySafeguard(owner);
            default:
                return null;
        }
    }
}

