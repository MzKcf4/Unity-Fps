using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BotWanderStateProcessor : AbstractBotStateProcessor
{
    private readonly ActionCooldown scanCooldown = new ActionCooldown { interval = 0.2f };
    private readonly ActionCooldown teammateKilledPreAimTimer = new ActionCooldown { interval = 2f };
    private float teammateKilledReactionDistance = 20f;
    private Vector3 preAimPosition;

    public BotWanderStateProcessor(MzFpsBotBrain fpsBot, FpsHumanoidCharacter character, BotFsmDto botFsmDto) : base(fpsBot,character, botFsmDto)
    {
        isReactToTeammateKilled = true;
        isReactToUnknownDamage = true;
    }

    public override void EnterState()
    {
        scanCooldown.StartCooldown();
        teammateKilledPreAimTimer.InstantCooldown();
        if (botFsmDto.targetWaypoint == Vector3.zero)
        {
            SetNewDestWaypoint();
        }
    }

    public override void ProcessState()
    {
        fpsCharacter.AimAtMovementDirection();

        if (fpsBotBrain.aiEnableWander && fpsBotBrain.IsReachedDesination())
        {
            SetNewDestWaypoint();
        }

        if (!fpsBotBrain.aiIgnoreEnemy && scanCooldown.CanExecuteAfterDeltaTime(true))
        {
            FpsModel detectedEnemy = fpsBotBrain.ScanForShootTarget();
            if (detectedEnemy != null)
            {
                botFsmDto.shootTargetModel = detectedEnemy;
                ExitToState(BotStateEnum.Engage);
                return;
            }
        }

        // Pre Aim ended.
        // if(teammateKilledPreAimTimer.CanExecuteAfterDeltaTime())


    }

    private void SetNewDestWaypoint()
    {
        // Transform newDest = Utils.GetRandomElement<Transform>(WaypointManager.Instance.mapGoalList);
        // List<Transform> waypointsByDistance = new List<Transform>();
        var waypointList = new List<Transform>(WaypointManager.Instance.mapGoalList);
        Transform newDest = waypointList.OrderByDescending(obj => Vector3.Distance(fpsBotBrain.transform.position, obj.position)).First();
        fpsBotBrain.SetDestination(newDest);
        botFsmDto.targetWaypoint = newDest.position;
    }

    public override void OnTeammateKilled(Vector3 deathPos, DamageInfo damageInfo)
    {
        base.OnTeammateKilled(deathPos, damageInfo);
        if (damageInfo.damageSourcePosition == Vector3.zero) return;
        if (Vector3.Distance(fpsBotBrain.transform.position, deathPos) > teammateKilledReactionDistance) return;
        if (!Utils.WithinChance(0.5f)) return;

        // Set preAim + move to where teammate died;
        teammateKilledPreAimTimer.StartCooldown();
        fpsBotBrain.SetDestination(deathPos);
        botFsmDto.targetWaypoint = deathPos;
        preAimPosition = damageInfo.damageSourcePosition;
        fpsCharacter.AimAtPosition(preAimPosition);
    }
}

