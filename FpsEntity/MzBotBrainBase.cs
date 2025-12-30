using Kit.Physic;
using NUnit.Framework.Interfaces;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
    protected AIDestinationSetter aiDest;
    protected float targetUpdateInterval = 5.0f;

    public float moveSpeed = 4f;
    public float speedRecoverDuration = 0.5f;
    public float speedRecoverElapsed = 0f;

    private GameObject destinationObj;


    protected virtual void Start()
    {
        character = GetComponent<FpsCharacter>();
        if (character.isServer)
        {
            destinationObj = new GameObject($"{gameObject.name}-AIDestination");
            ai = GetComponentInChildren<IAstarAI>();
            aiDest = GetComponentInChildren<AIDestinationSetter>();
            cooldownSystem = GetComponent<CooldownSystem>();

            character.onTakeDamageEvent.AddListener(OnCharacterTakeDamage);
            character.onKilledEvent.AddListener(OnCharacterKilled);
            character.onSpawnEvent.AddListener(OnCharacterSpawn);

            ai.maxSpeed = moveSpeed;

        }
        else
        {
            this.enabled = false;
        }
    }

    

    protected virtual void Update()
    {

    }

    protected void FixedUpdate()
    {
        if (!character.isServer || character.IsDead() || !aiEnabled) return;

        character.ServerSetVelocity(ai.velocity);
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
        destinationObj.transform.position = position;
        aiDest.target = destinationObj.transform;
    }

    public void StopMoving()
    {
        SetDestination(transform.position);
    }

    protected void TogglePathingFindingAI(bool isEnable)
    {
        ai.canSearch = isEnable;
        aiDest.enabled = isEnable;
        if (isEnable)
        {
            StartCoroutine(DelayEnablePathFinding(1f));
        }
        
        if(!isEnable)
        {
            // ai.simulateMovement = isEnable;
            aiDest.target = null;
        }
    }

    IEnumerator DelayEnablePathFinding(float delayTime)
    {
        //Wait for the specified delay time before continuing.
        yield return new WaitForSeconds(delayTime);

        // ai.simulateMovement = true;
        ai.canSearch = true;
        aiDest.enabled = true;
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

    protected virtual void OnCharacterSpawn()
    {
        TogglePathingFindingAI(true);
    }
}

