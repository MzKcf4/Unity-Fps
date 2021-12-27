using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public abstract class AbstractBotStateProcessor
{
    protected readonly FpsBot fpsBot;
    protected readonly BotFsmDto botFsmDto;
    protected bool isReactToUnknownDamage = true;
    protected bool isReactToTeammateKilled = true;

    public AbstractBotStateProcessor(FpsBot fpsBot, BotFsmDto botFsmDto) 
    {
        this.fpsBot = fpsBot;
        this.botFsmDto = botFsmDto;
    }
    public abstract void EnterState();
    public abstract void ProcessState();
    public virtual void ExitToState(BotStateEnum newState)
    {
        fpsBot.TransitToState(newState);
    }

    public virtual void OnTeammateKilled(Vector3 deathPos, DamageInfo damageInfo)
    { 
    
    }

    public virtual void OnTakeHit(DamageInfo damageInfo)
    {
        botFsmDto.lastUnexpectedDamage = damageInfo;
        if (isReactToUnknownDamage)
            ExitToState(BotStateEnum.ReactToUnknownDamage);
    }
}

