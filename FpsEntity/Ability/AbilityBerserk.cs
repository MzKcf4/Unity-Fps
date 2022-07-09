using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AbilityBerserk : Ability
{
    public static readonly string ID = "berserk";
    
    public AbilityBerserk(FpsCharacter owner) : base(owner)
    { 

    }

    protected override void ApplyEffects() 
    {
        owner.SetMaxSpeed(owner.MaxSpeed * 1.5f);
        EffectManager.Instance.RpcSetColor(owner.gameObject, Color.red);
    }

    protected override void RemoveEffects() 
    { 
        owner.SetMaxSpeed(owner.MaxSpeed / 1.5f);
        EffectManager.Instance.RpcSetColor(owner.gameObject, Color.white);
    }

    public override string GetID()
    {
        return ID;
    }
}

