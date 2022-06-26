using UnityEngine;

// When an enemy is found , transit to Engage state.
public class BotEngageStateProcessor : AbstractBotStateProcessor
{
    private readonly ActionCooldown weaponShotCooldown = new ActionCooldown();
    private readonly ActionCooldown reactionTime = new ActionCooldown();

    public BotEngageStateProcessor(MzFpsBotBrain fpsBot, FpsHumanoidCharacter character, BotFsmDto botFsmDto) : base(fpsBot,character, botFsmDto)
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

        weaponShotCooldown.interval = fpsCharacter.GetActiveWeapon().shootInterval;
        weaponShotCooldown.StartCooldown();
    }

    public override void ProcessState()
    {
        fpsCharacter.AimAtPosition(botFsmDto.shootTargetModel.transform.position + new Vector3(0, 1f, 0));

        // Still in reaction time
        if (!reactionTime.CanExecuteAfterDeltaTime())
        {
            fpsBot.StopMoving();
            return;
        } 

        // Can shoot now
        if (!weaponShotCooldown.CanExecuteAfterDeltaTime(true)) return;

        FpsModel shootTarget = botFsmDto.shootTargetModel;

        // Check if Target is valid / already dead
        if (!shootTarget || shootTarget.fpsCharacter.IsDead())
        {
            ResetShootTarget();
            ExitToState(BotStateEnum.Wandering);
            return;
        }

        /*
        // Set points for bot to strafe
        if (CanStrafe() && fpsBot.IsReachedDesination())
        {
            fpsBot.SetDestination(PickRandomStrafePoint());
        }
        */

        // Otherwise , find a visible body part to shoot
        Transform shootAtHitBox = fpsBot.GetVisibleHitBoxFromAimTarget(shootTarget.gameObject);
        
        if (shootAtHitBox != null)
        {
            // Got visible hitbox to shoot
            fpsCharacter.AimAtPosition(shootAtHitBox.position);
            fpsCharacter.ShootAtTarget();
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

    private Vector3 PickRandomStrafePoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * 2f;
        randomPoint.y = fpsBot.transform.position.y;

        return randomPoint + fpsBot.transform.position;
    }

    private bool CanStrafe()
    {
        
        return fpsCharacter.GetActiveWeapon() != null && fpsCharacter.GetActiveWeapon().weaponCategory != WeaponCategory.Sniper;
    }
}