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
    
    // Assume bot can only hold 1 weapon at the moment
    [HideInInspector] public FpsWeapon fpsWeapon;
    
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
        }
        if(objAttachToModel != null)
        {
            objAttachToModel.transform.parent = fpsModel.transform;
        }
        fpsModel.SetLookAtTransform(lookAtTransform);
        GetWeapon(WeaponAssetManager.Instance.ak47WeaponPrefab , 0);
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
        if(fpsWeapon == null)
            return;
        
        float spreadMultiplier = fpsWeapon.spread;
        if(fpsWeapon.weaponCategory != WeaponCategory.Shotgun)
        {
            spreadMultiplier *= 5f;
        }
        
        for(int i = 0 ; i < fpsWeapon.palletPerShot ; i++)
        {
            RayHitInfo rayHitInfo = Utils.CastRayAndGetHitInfo(visionSensor.transform , LayerMask.GetMask(Constants.LAYER_HITBOX , Constants.LAYER_GROUND, Constants.LAYER_LOCAL_PLAYER_HITBOX), fpsWeapon.spread);
            if(rayHitInfo == null)
                continue;
            
            fpsWeapon.FireWeapon(rayHitInfo.hitPoint);
            GameObject objOnHit = rayHitInfo.hitObject;
            
            // Hits wall
            if( (1 << objOnHit.layer) == LayerMask.GetMask(Constants.LAYER_GROUND))
            {
                LocalSpawnManager.Instance.SpawnBulletDecalFx(rayHitInfo.hitPoint , rayHitInfo.normal);
                continue;
            }
            
            // Else should expect hitting hitbox
            FpsHitbox enemyHitBox = objOnHit.GetComponent<FpsHitbox>();
            FpsEntity hitEntity = enemyHitBox.fpsEntity;
        
            if(hitEntity is FpsCharacter)
            {
                TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                if(hitTeam == this.team)   continue;
            }
        
            if(hitEntity != null)
            {
                DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon , enemyHitBox , enemyHitBox.transform.position);
                hitEntity.TakeDamage(dmgInfo);
                continue;
            }
        }        
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
                    
    [Server]
    protected override void Killed(DamageInfo damageInfo)
    {
        base.Killed(damageInfo);
        PlayerManager.Instance.QueueRespawn(this);
    }
    
    public void GetWeapon(GameObject weaponModelPrefab , int slot)
    {
        GameObject weaponModelObj = Instantiate(weaponModelPrefab , weaponRootTransform);
        FpsWeapon fpsWeapon = weaponModelObj.GetComponent<FpsWeapon>();
        fpsWeapon.owner = this;
        
        this.fpsWeapon = fpsWeapon;
        this.weaponShotCooldown.interval = fpsWeapon.shootInterval;
    }
    
    public override Vector3 GetMovementVelocity()
    {
        if(ai == null)
            return Vector3.zero;
        return ai.velocity;
    }

    /*
    private void RotateModelTowardsDirection(Vector3 dir)
    {
        if(dir == Vector3.zero)   return;
        
        
        Quaternion lookAtTargetRotation = Quaternion.LookRotation(dir);
        lookAtRotationTransform.rotation = Quaternion.RotateTowards(lookAtRotationTransform.rotation , lookAtTargetRotation , 200f * Time.deltaTime);
        // Offset y to 0 so model doesn't rotate up/down
        Quaternion targetDir = Quaternion.LookRotation(new Vector3(dir.x , 0f , dir.z));
        
        modelObjectParent.transform.rotation = Quaternion.RotateTowards(modelObjectParent.transform.rotation , targetDir , 200f * Time.deltaTime);
    }
    */
}
