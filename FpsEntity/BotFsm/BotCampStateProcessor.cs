using UnityEngine;
using Pathfinding;

public class BotCampStateProcessor : AbstractBotStateProcessor
{
    private readonly ActionCooldown scanCooldown = new ActionCooldown { interval = 0.2f };
    private float campingElapsed = 0f;
    private float campingDuration = 15f;
    private CampingSpot currentSpot;

    public BotCampStateProcessor(MzFpsBotBrain fpsBot, FpsHumanoidCharacter character, BotFsmDto botFsmDto) : base(fpsBot, character, botFsmDto)
    {
        isReactToTeammateKilled = false; // Don't leave camping spot for teammate death
        isReactToUnknownDamage = true;
    }

    private bool hasSightedEnemy = false;

    public override void EnterState()
    {
        hasSightedEnemy = false;
        campingElapsed = 0f;
        campingDuration = Random.Range(10f, 20f);
        
        // Request spot
        if (CampingManager.Instance != null)
        {
            currentSpot = CampingManager.Instance.GetBestCampingSpot(fpsCharacter.transform.position, fpsCharacter.team);
        }

        if (currentSpot != null)
        {
            fpsBotBrain.SetDestination(currentSpot.Position);
            botFsmDto.targetWaypoint = currentSpot.Position;
        }
        else
        {
            // No spot found, go back to wandering
            ExitToState(BotStateEnum.Wandering);
        }
    }

    public override void ProcessState()
    {
        if (currentSpot == null)
        {
            ExitToState(BotStateEnum.Wandering);
            return;
        }

        // 1. Move to spot
        if (!fpsBotBrain.IsReachedDesination())
        {
             fpsCharacter.AimAtMovementDirection();
             return; // Still moving
        }

        // 2. Reached spot, now Camp
        fpsBotBrain.StopMoving();

        // Look at average kill direction
        Vector3 lookDir = currentSpot.AverageLookDir;
        if (lookDir == Vector3.zero) lookDir = fpsCharacter.transform.forward;

        // Flatten lookDir y to prevent extreme up/down if not needed, but average look dir might have Y.
        // Let's add height to origin so we aim at head level relative to origin
        Vector3 lookTarget = fpsCharacter.transform.position + Vector3.up * 1.5f + lookDir * 10f;
        fpsCharacter.AimAtPosition(lookTarget);

        // Manually rotate character to face the direction (Help the IK)
        Vector3 flatDir = lookDir;
        flatDir.y = 0;
        if (flatDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            fpsCharacter.transform.rotation = Quaternion.Slerp(fpsCharacter.transform.rotation, targetRot, Time.deltaTime * 5f);
        }

        // Scan for enemies
        if (!fpsBotBrain.aiIgnoreEnemy && scanCooldown.CanExecuteAfterDeltaTime(true))
        {
            FpsModel detectedEnemy = fpsBotBrain.ScanForShootTarget();
            if (detectedEnemy != null)
            {
                if (!hasSightedEnemy)
                {
                    hasSightedEnemy = true;
                    CampingManager.Instance.RegisterCampingResult(currentSpot.Position, true);
                }

                botFsmDto.shootTargetModel = detectedEnemy;
                ExitToState(BotStateEnum.Engage);
                return;
            }
        }

        // Check time
        campingElapsed += Time.deltaTime;
        if (campingElapsed >= campingDuration)
        {
            // Punishment: Camped for full duration but saw no one
            if (!hasSightedEnemy)
            {
                CampingManager.Instance.RegisterCampingResult(currentSpot.Position, false);
            }
            ExitToState(BotStateEnum.Wandering);
        }
    }

    public override void OnTakeHit(DamageInfo damageInfo)
    {
        base.OnTakeHit(damageInfo);

        if (currentSpot != null && damageInfo.attacker != null && fpsCharacter.health - damageInfo.damage <= 0)
        {
            // Fatal hit - check if we saw it coming
            Transform seenTransform = fpsBotBrain.GetVisibleHitBoxFromAimTarget(damageInfo.attacker.gameObject);
            
            if (seenTransform == null)
            {
                // Blindside! This spot is unsafe from flanking/sniping
                CampingManager.Instance.RegisterCampingResult(currentSpot.Position, false);
            }
        }
    }
}
