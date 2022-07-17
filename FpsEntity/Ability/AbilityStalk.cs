using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AbilityStalk : Ability
{
    public static readonly string ID = "stalk";
    private static readonly Color TRANSPARENT = new Color(1f, 1f, 1f, 0.2f);

    public AbilityStalk(FpsCharacter owner) : base(owner)
    {

    }

    protected override void ApplyEffects()
    {
        owner.SetMaxSpeed(owner.MaxSpeed * 1.5f);
        EffectManager.Instance.RpcSetColor(owner.gameObject, TRANSPARENT);
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

