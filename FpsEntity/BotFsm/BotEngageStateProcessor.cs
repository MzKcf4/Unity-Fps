using System.Runtime.CompilerServices;
using UnityEngine;

// When an enemy is found , transit to Engage state.
public class BotEngageStateProcessor : AbstractBotStateProcessor
{
    private readonly ActionCooldown weaponShotCooldown = new ActionCooldown();
    private readonly ActionCooldown reactionTime = new ActionCooldown();

    public BotEngageStateProcessor(MzFpsBotBrain fpsBotBrain, FpsHumanoidCharacter character, BotFsmDto botFsmDto) : base(fpsBotBrain,character, botFsmDto)
    {
        isReactToUnknownDamage = false;
        isReactToTeammateKilled = false;
    }

    public override void EnterState()
    {
        if (botFsmDto.shootTargetModel == null || fpsCharacter.GetActiveWeapon() == null)
        {
            ExitToState(BotStateEnum.Wandering);
            return;
        }

        reactionTime.interval = fpsBotBrain.reactionTime;
        reactionTime.StartCooldown();

        weaponShotCooldown.interval = fpsCharacter.GetActiveWeapon().shootInterval;
        weaponShotCooldown.StartCooldown();
    }

    public override void ProcessState()
    {
        fpsCharacter.AimAtPosition(botFsmDto.shootTargetModel.transform.position + new Vector3(0, 1f, 0));

        if (fpsCharacter.IsWeaponReloading() || fpsCharacter.GetActiveWeapon() == null)
        {
            return;
        }

        if (fpsCharacter.IsWeaponOutOfAmmo())
        {
            fpsCharacter.DoWeaponReload();
            return;
        }

        // Still in reaction time
        if (!reactionTime.CanExecuteAfterDeltaTime())
        {
            fpsBotBrain.StopMoving();
            return;
        } 

        // Can shoot now
        // if (!weaponShotCooldown.CanExecuteAfterDeltaTime(true)) return;

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
        Transform shootAtHitBox = fpsBotBrain.GetVisibleHitBoxFromAimTarget(shootTarget.gameObject);
        
        if (shootAtHitBox != null)
        {
            // Got visible hitbox to shoot
            fpsCharacter.AimAtPosition(shootAtHitBox.position);
            fpsCharacter.DoWeaponPrimaryAction();
            // Manually "release" it for semi-auto weapons to fire
            fpsCharacter.GetActiveWeapon().UpdateWeaponPrimaryActionState(KeyPressState.Released);
        }
        else
        {
            // Lost the target when shooting , chase the target
            ResetShootTarget();
            botFsmDto.shootTargetLastSeenPosition = shootTarget.transform.position;
            ExitToState(BotStateEnum.Chasing);
        }
    }

    private void ResetShootTarget()
    {
        botFsmDto.shootTargetModel = null;
    }

    private Vector3 PickRandomStrafePoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * 2f;
        randomPoint.y = fpsBotBrain.transform.position.y;

        return randomPoint + fpsBotBrain.transform.position;
    }

    private bool CanStrafe()
    {
        
        return fpsCharacter.GetActiveWeapon() != null && fpsCharacter.GetActiveWeapon().weaponCategory != WeaponCategory.Sniper;
    }
}