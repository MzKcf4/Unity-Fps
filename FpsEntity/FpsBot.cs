using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Kit.Physic;
using SensorToolkit;
using Pathfinding;
using Animancer;
using RootMotion.FinalIK;

public partial class FpsBot : FpsCharacter
{
    public bool aiEnabled = true;
    public bool aiIgnoreEnemy = false;
    public bool aiEnableWander = true;
    
    protected TriggerSensor visionSensor;
    
    private IAstarAI ai;
    private Seeker seeker;
    private AIDestinationSetter aiDest;
    public Vector3 moveDest;
    // --------- Pain Shock ------------ //
    public float moveSpeed = 4f;
    public float speedRecoverDuration = 0.5f;
    public float speedRecoverElapsed = 0f;
    // --------------------------------- //
    
    ActionCooldown weaponShotCooldown = new ActionCooldown();
    // private FpsBotFsm botFsm = new FpsBotFsm();
    
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
            
            ServerGetWeapon("csgo_ak47" , 0);
            RpcGetWeapon("csgo_ak47" , 0);
            weaponShotCooldown.interval = 0.1f;
        }
        if(objAttachToModel != null)
        {
            objAttachToModel.transform.parent = fpsModel.transform;
        }
    }
    
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(!isServer || IsDead() || !aiEnabled)   return;
        RecoverSpeed();
        
        Update_Fsm();
        
        if (botState == BotStateEnum.Shooting)
        {
            
            if(weaponShotCooldown.CanExecuteAfterDeltaTime(true))
            {
                ScanVisibleLosFromShootTarget();
                if(aimAtHitboxTransform != null)
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
        UpdateLookAt_Fsm();
    }
    
    private void UpdateMovementDestination()
    {
        if(!aiEnableWander)
            return;
            
        if(botState == BotStateEnum.Default || botState == BotStateEnum.Alert)
        {
            if(moveDest == null || moveDest == Vector3.zero || ai.reachedDestination)
            {
                Transform newDest = Utils.GetRandomElement<Transform>(WaypointManager.Instance.mapGoalList);
                moveDest = newDest.position;
                aiDest.SetByPosition(moveDest);
            }
            else
            {
                aiDest.SetByPosition(moveDest);
            }
        }
        else if (botState == BotStateEnum.Aiming || botState == BotStateEnum.Shooting)
        {
            // Stop moving , by now
            aiDest.SetByTransform(transform);
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
        
        OnTakeHit(damageInfo);
    }
    
    [Server]
    protected override void Killed(DamageInfo damageInfo)
    {
        base.Killed(damageInfo);
        BoardcastKilledWarning(damageInfo);
    }
    
    [Server]
    private void BoardcastKilledWarning(DamageInfo damageInfo)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 12f);
        foreach (Collider hitCollider in hitColliders)
        {
            FpsBot fpsBot = hitCollider.GetComponent<FpsBot>();
            if(fpsBot == null || fpsBot == this || fpsBot.IsDead() || fpsBot.team != this.team)
                continue;
            
            fpsBot.OnTeammateKilledNearby_Fsm(transform.position , damageInfo);
        }
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
        ProcessWeaponEventUpdate_Fsm(evt);
        if(evt == WeaponEvent.Shoot)
        {
            
            float spreadMultiplier = GetActiveWeapon().spread;
            // ---------ToDo: Shotgun pallet ??--------------- //
            if(GetActiveWeapon().weaponCategory != WeaponCategory.Shotgun)
            {
                spreadMultiplier *= 5f;
            }
            Vector3 shootDirection = Utils.GetDirection( fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform.position, targetLookAtPosition);
            CoreGameManager.Instance.DoWeaponRaycast(this , GetActiveWeapon() , fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform.position, shootDirection);
            // ----------------------------------------------- //
            RpcFireWeapon();
        }
    }
}
