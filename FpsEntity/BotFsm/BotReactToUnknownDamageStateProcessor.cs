﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class BotReactToUnknownDamageStateProcessor : AbstractBotStateProcessor
{
    private readonly ActionCooldown aimToDmgSourceTimer = new ActionCooldown { interval = 1.5f };

    public BotReactToUnknownDamageStateProcessor(MzFpsBotBrain fpsBot, FpsHumanoidCharacter character, BotFsmDto botFsmDto) : base(fpsBot,character, botFsmDto)
    {
        this.isReactToUnknownDamage = false;
        isReactToTeammateKilled = false;
    }

    public override void EnterState()
    {
        if (botFsmDto.lastUnexpectedDamage == null)
        {
            ExitToState(BotStateEnum.Wandering);
            return;
        }
        aimToDmgSourceTimer.StartCooldown();
    }

    public override void ProcessState()
    {
        // Stop and aim to damage source for certain time
        fpsBot.StopMoving();
        fpsCharacter.AimAtPosition(botFsmDto.lastUnexpectedDamage.damageSourcePosition);

        if (!aimToDmgSourceTimer.CanExecuteAfterDeltaTime())
            return;

        // After aiming time , decide whether to go to damage source position , or continue to move.
        if (Utils.WithinChance(0.5f))
            botFsmDto.targetWaypoint = botFsmDto.lastUnexpectedDamage.damageSourcePosition;

        // Exit to wander state to keep moving
        botFsmDto.lastUnexpectedDamage = null;
        ExitToState(BotStateEnum.Wandering);
    }
}

