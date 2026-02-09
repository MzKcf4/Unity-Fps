using Org.BouncyCastle.Utilities;
using Pathfinding;
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
            // Chance to camp
            float campChance = 0.3f;
            var weapon = fpsCharacter.GetActiveWeapon();
            if (weapon != null)
            {
                if (weapon.weaponCategory == WeaponCategory.Sniper) campChance = 0.7f;
                else if (weapon.weaponCategory == WeaponCategory.Smg) campChance = 0.1f;
            }

            if (CampingManager.Instance != null && CampingManager.Instance.campingSpots.Count > 0 && Utils.WithinChance(campChance))
            {
                ExitToState(BotStateEnum.Camping);
                return;
            }

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
        TeamEnum myTeam = fpsCharacter.team;
        TeamEnum enemyTeam = myTeam == TeamEnum.Blue ? TeamEnum.Red : TeamEnum.Blue;

        List<Transform> mySpawns = WaypointManager.Instance.GetSpawnPoints(myTeam);
        List<Transform> enemySpawns = WaypointManager.Instance.GetSpawnPoints(enemyTeam);

        // Fallback for modes without clear teams
        if (mySpawns.Count == 0 || enemySpawns.Count == 0)
        {
            mySpawns = WaypointManager.Instance.mapGoalList;
            enemySpawns = WaypointManager.Instance.mapGoalList;
        }

        bool isAtEnemyBase = false;
        foreach (Transform t in enemySpawns)
        {
            if (Vector3.Distance(fpsBotBrain.transform.position, t.position) < 15.0f)
            {
                isAtEnemyBase = true;
                break;
            }
        }

        // If at enemy base, return home. Otherwise, attack enemy base.
        List<Transform> targetList = isAtEnemyBase ? mySpawns : enemySpawns;

        Transform newDest = Utils.GetRandomElement(targetList);
        float randomOffset = UnityEngine.Random.Range(0.4f, 1f);

        // Find a position between current position and newDest
        Vector3 offsetPosition = Vector3.Lerp(fpsBotBrain.transform.position, newDest.position, randomOffset);
        var node = AstarPath.active.GetNearest(offsetPosition, NearestNodeConstraint.Walkable).node;
        var walkablePos = (Vector3)node.position;

        fpsBotBrain.SetDestination(walkablePos);
        botFsmDto.targetWaypoint = walkablePos;
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

