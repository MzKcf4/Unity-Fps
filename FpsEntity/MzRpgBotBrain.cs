using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Pathfinding;
using Kit.Physic;


public class MzRpgBotBrain : MzBotBrainBase
{
    protected static readonly string MELEE_COOLDOWN = "bot-melee-cooldown";
    protected static readonly string MELEE_CHECK = "bot-melee-check";

    public bool canMeleeAttack = true;
    public float meleeCooldown = 5.0f;
    public float meleeCheckInterval = 1.0f;

    public bool canRangeAttack = false;
    private MzRpgCharacter rpgCharacter;

    protected override void Start()
    {
        base.Start();

        rpgCharacter = GetComponent<MzRpgCharacter>();
        if (character.isServer)
        {

        }
        else
        {
            this.enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (rpgCharacter == null || rpgCharacter.IsDead() || !rpgCharacter.isServer)
            return;

        SeekTarget();
        CheckMeleeRange();
    }

    protected virtual void CheckMeleeRange()
    {
        if (!cooldownSystem.IsOnCooldown(MELEE_COOLDOWN) && !cooldownSystem.IsOnCooldown(MELEE_CHECK))
        {
            if (!cooldownSystem.IsOnCooldown(MELEE_CHECK))
            {
                if (DetectedTarget())
                {
                    ExecuteMeleeAttack();
                    cooldownSystem.PutOnCooldown(MELEE_COOLDOWN, meleeCooldown);
                }
                cooldownSystem.PutOnCooldown(MELEE_CHECK, meleeCheckInterval);
            }
        }
    }

    protected virtual void ExecuteMeleeAttack()
    {
        character.ServerPlayAnimationByKey(Constants.ACTION_KEY_MELEE);
    }

    protected override void SeekTarget()
    {
        if (cooldownSystem.IsOnCooldown(TARGET_UPDATE_COOLDOWN)) return;
        if (aiDest.target != null) return;

        aiDest.target = ServerContext.Instance.GetRandomPlayer().transform;
        cooldownSystem.PutOnCooldown(TARGET_UPDATE_COOLDOWN, targetUpdateInterval);
        /*
        if (target == null)
            targetPlayer = ServerContext.Instance.GetRandomPlayer();
        else
        {
            // Chance to switch to cloest player.
            if (Random.RandomRange(0, 100) < 25)
            {
                float shortestDist = float.MaxValue;
                foreach (FpsPlayer player in ServerContext.Instance.playerList)
                {
                    float dist = Vector3.Distance(transform.position, player.transform.position);
                    if (dist < shortestDist)
                    {
                        targetPlayer = player;
                        shortestDist = dist;
                    }
                }
            }
        }
        */
    }

    /*
    [Server]
    protected void ShootTargetInRange()
    {
        if (!isServer || !canRanged || rangedDector == null || rangedCooldown.IsOnCooldown()) return;

        List<FpsPlayer> detectedPlayers = rangedDector.GetDetectedByComponent<FpsPlayer>();
        if (detectedPlayers != null && detectedPlayers.Count > 0)
        {
            DoRangedAttack(detectedPlayers[0].gameObject);
        }
        rangedCooldown.StartCooldown();
    }

    protected virtual void DoRangedAttack(GameObject target)
    {
        if (!isServer) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        GameObject projectileObj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
        NetworkServer.Spawn(projectileObj);
        FpsProjectile projectile = projectileObj.GetComponent<FpsProjectile>();
        projectile.Setup(direction);
        RpcDoRangedAttack(direction);
    }
    */

    private void RecoverSpeed()
    {
        if (ai == null || ai.maxSpeed >= moveSpeed) return;

        if (speedRecoverElapsed < speedRecoverDuration)
        {
            ai.maxSpeed = Mathf.Lerp(0, moveSpeed, speedRecoverElapsed / speedRecoverDuration);
            speedRecoverElapsed += Time.deltaTime;
        }
        else
        {
            ai.maxSpeed = moveSpeed;
        }
    }

    protected bool DetectedTarget()
    {
        return rpgCharacter.GetTargetsInMeleeRange().Count > 0;
    }
}

