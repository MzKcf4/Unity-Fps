using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// When an enemy is found , transit to Engage state.
public class BotEngageStateProcessor : AbstractBotStateProcessor
{
    private readonly ActionCooldown weaponShotCooldown = new ActionCooldown();
    private readonly ActionCooldown reactionTime = new ActionCooldown();

    public BotEngageStateProcessor(FpsBot fpsBot, BotFsmDto botFsmDto) : base(fpsBot, botFsmDto)
    {
        this.isReactToUnknownDamage = false;
        isReactToTeammateKilled = false;
    }

    public override void EnterState()
    {
        if (botFsmDto.shootTargetModel == null)
        {
            ExitToState(BotStateEnum.Wandering);
            return;
        }

        reactionTime.interval = fpsBot.reactionTime;
        reactionTime.StartCooldown();

        weaponShotCooldown.interval = fpsBot.GetActiveWeapon().shootInterval;
        weaponShotCooldown.StartCooldown();
    }

    public override void ProcessState()
    {
        fpsBot.StopMoving();
        fpsBot.SetLookAtToPosition(botFsmDto.shootTargetModel.transform.position + new Vector3(0, 1f, 0));

        // Still in reaction time
        if (!reactionTime.CanExecuteAfterDeltaTime()) return;

        // Can shoot now
        if (!weaponShotCooldown.CanExecuteAfterDeltaTime(true)) return;

        FpsModel shootTarget = botFsmDto.shootTargetModel;

        // Check if Target is valid / already dead
        if (!shootTarget || shootTarget.controllerEntity.IsDead())
        {
            ResetShootTarget();
            ExitToState(BotStateEnum.Wandering);
            return;
        }

        // Otherwise , find a visible body part to shoot
        Transform shootAtHitBox = fpsBot.GetVisibleHitBoxFromAimTarget(shootTarget.gameObject);
        if (shootAtHitBox != null)
        {
            // Got visible hitbox to shoot
            fpsBot.SetLookAtToPosition(shootAtHitBox.position);
            fpsBot.ShootAtTarget();
        }
        else
        {
            // Lost the target when shooting , chase the target
            ResetShootTarget();
            botFsmDto.shootTargetLastSeenPosition = shootTarget.transform.position;
            ExitToState(BotStateEnum.Chasing);
            return;
        }
    }

    private void ResetShootTarget()
    {
        botFsmDto.shootTargetModel = null;
    }
}

