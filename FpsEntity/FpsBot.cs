using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Kit.Physic;
using SensorToolkit;
using Pathfinding;
using Animancer;
using RootMotion.FinalIK;

public class FpsBot : FpsCharacter
{
    public bool aiEnabled = true;
    public bool aiIgnoreEnemy = false;
    
    protected TriggerSensor visionSensor;
    
    private IAstarAI ai;
    private Seeker seeker;
    private AIDestinationSetter aiDest;
    public Transform moveDest;
    // --------- Pain Shock ------------ //
    public float moveSpeed = 4f;
    public float speedRecoverDuration = 0.5f;
    public float speedRecoverElapsed = 0f;
    // --------------------------------- //
    
    ActionCooldown weaponShotCooldown = new ActionCooldown();
    private FpsBotFsm botFsm = new FpsBotFsm();
    
    public GameObject objAttachToModel;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        if(isServer)
        {
            visionSensor = GetComponentInChildren<TriggerSensor>();
            ai = GetComponent<IAstarAI>();
            seeker = GetComponent<Seeker>();
            aiDest = GetComponent<AIDestinationSetter>();
            SetupFsm();
            
            ServerGetWeapon("csgo_ak47" , 0);
            RpcGetWeapon("csgo_ak47" , 0);
            weaponShotCooldown.interval = 0.1f;
        }
        if(objAttachToModel != null)
        {
            objAttachToModel.transform.parent = fpsModel.transform;
        }
    }
    
    private void SetupFsm()
    {
        botFsm.Setup(this , visionSensor);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(!isServer || IsDead() || !aiEnabled)   return;
        RecoverSpeed();
        
        botFsm.ManualUpdate();
        
        if (botFsm.botState == BotStateEnum.Shooting)
        {
            
            if(weaponShotCooldown.CanExecuteAfterDeltaTime(true))
            {
                // Debug.Log(weaponShotCooldown.nextUse);
                botFsm.ScanVisibleLosFromShootTarget();
                if(botFsm.aimAtHitboxTransform != null)
                    ShootAtTarget();
                
            }
        }
        
        UpdateMovementDestination();
        UpdateLookAt();
    }
        
    private void RecoverSpeed()
    {
        if(ai == null || ai.maxSpeed >= moveSpeed)  return;
        
        if(speedRecoverElapsed < speedRecoverDuration)
        {
            ai.maxSpeed = Mathf.Lerp(0 , moveSpeed , speedRecoverElapsed / speedRecoverDuration);
            speedRecoverElapsed += Time.deltaTime;
        }
        else
        {
            ai.maxSpeed = moveSpeed;
        }
    }
    
    private void UpdateLookAt()
    {
        if(botFsm.targetLookAtPosition == Vector3.zero)
        {
            Vector3 moveVec = GetMovementVelocity().normalized * 2f;
            if(moveVec != Vector3.zero)
                lookAtTransform.localPosition = new Vector3(moveVec.x , 1.3f + moveVec.y , moveVec.z);            
        }
        else
            lookAtTransform.position = botFsm.targetLookAtPosition;
    }
    
    private void UpdateMovementDestination()
    {
        if(botFsm.botState == BotStateEnum.Default || botFsm.botState == BotStateEnum.Alert)
        {
            if(moveDest == null || ai.reachedDestination)
            {
                Transform newDest = Utils.GetRandomElement<Transform>(WaypointManager.Instance.mapGoalList);
                moveDest = newDest;
                aiDest.target = newDest;
            }
            else
            {
                aiDest.target = moveDest;
            }
        }
        else if (botFsm.botState == BotStateEnum.Aiming || botFsm.botState == BotStateEnum.Shooting)
        {
            // Stop moving , by now
            aiDest.target = transform;
        }
    }
    
    [Server]
    private void ShootAtTarget()
    {
        if(GetActiveWeapon() == null)
            return;
        
        GetActiveWeapon().DoWeaponFire();
    }
                
    [Server]
    public override void TakeDamage(DamageInfo damageInfo)
    {
        base.TakeDamage(damageInfo);
        // Pain shock
        if(ai != null)
        {
            ai.maxSpeed = 0f;   
            speedRecoverElapsed = 0f;
        }
        
        botFsm.OnTakeHit(damageInfo);
    }

    public override Vector3 GetMovementVelocity()
    {
        if(isServer)
        {
            if(ai == null)
                currentVelocity = Vector3.zero;
            else
                currentVelocity = ai.velocity;
        }
        return currentVelocity;
    }
    
    public override void ProcessWeaponEventUpdate(WeaponEvent evt)
    {
        botFsm.ProcessWeaponEventUpdate(evt);
        if(evt == WeaponEvent.Shoot)
        {
            
            float spreadMultiplier = GetActiveWeapon().spread;
            // ---------ToDo: Shotgun pallet ??--------------- //
            if(GetActiveWeapon().weaponCategory != WeaponCategory.Shotgun)
            {
                spreadMultiplier *= 5f;
            }
            Vector3 shootDirection = Utils.GetDirection( fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform.position, botFsm.targetLookAtPosition);
            CoreGameManager.Instance.DoWeaponRaycast(this , GetActiveWeapon() , fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform.position, shootDirection);
            // ----------------------------------------------- //
            RpcFireWeapon();
        }
    }
}
