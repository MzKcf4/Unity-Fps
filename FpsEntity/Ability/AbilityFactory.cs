using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AbilityFactory
{
    public static List<AbilityEnum> AbilitiesForPlayer = new List<AbilityEnum>() {
        AbilityEnum.HawkEye
    };

    public static Ability GetAbility(AbilityEnum abilityEnum, FpsCharacter owner)
    {
        switch (abilityEnum) 
        {
            case AbilityEnum.HawkEye:
                return new AbilityHawkEye(owner);
            default:
                return null;
        }
    }
}

