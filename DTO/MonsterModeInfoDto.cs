using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MonsterModeInfoDto
{
    public HashSet<AbilityEnum> Abilities { get { return abilities; } }

    private readonly HashSet<AbilityEnum> abilities = new HashSet<AbilityEnum>();
}

