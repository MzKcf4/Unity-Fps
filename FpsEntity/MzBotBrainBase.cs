using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Pathfinding;
using Kit.Physic;

// A bot has following properties
// - Make use of and manages pathfinding components
// - Deploys AI to execute situational behaviors
// - Can consider as an AI-character-controller

[RequireComponent(typeof(CooldownSystem))]
public abstract class MzBotBrainBase : MonoBehaviour
{
    protected static readonly string TARGET_UPDATE_COOLDOWN = "bot-target-update-cooldown";
    protected FpsCharacter character;
    protected CooldownSystem cooldownSystem;

    public bool aiEnabled = true;
    public bool aiIgnoreEnemy = false;
    public bool aiEnableWander = true;

    protected IAstarAI ai;
    protected Seeker seeker;
    protected AIDestinationSetter aiDest;
    protected float targetUpdateInterval = 5.0f;

    public float moveSpeed = 4f;
    public float speedRecoverDuration = 0.5f;
    public float speedRecoverElapsed = 0f;


    protected virtual void Start()
    {
        character = GetComponent<FpsCharacter>();
        if (character.isServer)
        {
            ai = GetComponent<IAstarAI>();
            seeker = GetComponent<Seeker>();
            aiDest = GetComponent<AIDestinationSetter>();
            cooldownSystem = GetComponent<CooldownSystem>();

            character.onTakeDamageEvent.AddListener(OnCharacterTakeDamage);
            character.onKilledEvent.AddListener(OnCharacterKilled);
            character.onSpawnEvent.AddListener(OnCharacterSpawn);

            ai.canMove = false;
            ai.maxSpeed = moveSpeed;

        }
        else
        {
            AIBase aiBase = GetComponent<AIBase>();
            aiDest.enabled = false;
            aiBase.enabled = false;
            seeker.enabled = false;
            this.enabled = false;
        }
    }

    protected virtual void Update()
    {
        if (!character.isServer || character.IsDead() || !aiEnabled) return;

        RecoverSpeed();
    }

    protected virtual void SeekTarget()
    {

    }

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

    public void SetDestination(Transform targetTransform)
    {
        SetDestination(targetTransform.position);
    }

    public void SetDestination(Vector3 position)
    {
        ai.destination = position;
    }

    public void StopMoving()
    {
        SetDestination(transform.position);
    }

    protected void TogglePathingFindingAI(bool isEnable)
    {
        ai.canMove = isEnable;
        ai.canSearch = isEnable;
        seeker.enabled = isEnable;
        aiDest.enabled = isEnable;
    }

    private void OnCharacterTakeDamage(DamageInfo damageInfo)
    {
        ai.maxSpeed = 0f;
        speedRecoverElapsed = 0f;
    }

    private void OnCharacterKilled(GameObject dummy)
    {
        TogglePathingFindingAI(false);
    }

    private void OnCharacterSpawn()
    {
        TogglePathingFindingAI(true);
    }
}

