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
    // private BotStateEnum currBotState = BotStateEnum.Default;
    
    // When distance = hitChanceDistanceBase , the multiplier is 1
    private float hitChanceDistanceBase = 15f;
    
    public bool aiEnabled = true;
    [SerializeField]
    protected TriggerSensor visionSensor;
    public FpsModel aimAtFpsModel;
    public Transform shootAtTransform;
    public ActionCooldown reactionCooldown = new ActionCooldown() {interval = 1f};
    
    private IAstarAI ai;
    private Seeker seeker;
    private AIDestinationSetter aiDest;
    public Transform moveDest;
    // --------- Pain Shock ------------ //
    public float moveSpeed = 4f;
    public float speedRecoverDuration = 0.5f;
    public float speedRecoverElapsed = 0f;
    // --------------------------------- //
    
    ActionCooldown scanCooldown = new ActionCooldown() { interval = 1f};
    ActionCooldown weaponShotCooldown = new ActionCooldown();
    ActionCooldown alertStateCooldown = new ActionCooldown { interval = 2f};
    private FpsBotFsm botFsm = new FpsBotFsm();
    
    // Assume bot can only hold 1 weapon at the moment
    public FpsWeapon fpsWeapon;
    
    private Vector3 lookAtTransformOffset;
    [SerializeField] private Transform lookAtRotationTransform;
    
    private Vector3 lookAtDirection = Vector3.zero;
    private Quaternion alertLookRotation = Quaternion.identity;
    private Vector3 alertLookDirection = Vector3.zero;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        lookAtTransformOffset = lookAtTransform.localPosition;
        
        if(isServer)
        {
            visionSensor = GetComponentInChildren<TriggerSensor>();
            ai = GetComponent<IAstarAI>();
            seeker = GetComponent<Seeker>();
            aiDest = GetComponent<AIDestinationSetter>();
            SetupFsm();
        }
        fpsModel.SetLookAtTransform(lookAtTransform);
        GetWeapon(WeaponAssetManager.Instance.sawoffWeaponPrefab , 0);
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
        
        if(botFsm.botState == BotStateEnum.Aiming)
        {
            lookAtDirection = Utils.GetDirection(modelObjectParent.transform.position, botFsm.aimAtFpsModel.transform.position);
        }
        else if (botFsm.botState == BotStateEnum.Shooting)
        {
            if(weaponShotCooldown.CanExecuteAfterDeltaTime(true))
            {
                botFsm.ScanVisibleLosFromShootTarget();
                if(botFsm.aimAtHitboxTransform != null)
                    ShootAtTarget(botFsm.aimAtFpsModel , botFsm.aimAtHitboxTransform);
                
            }
        }
        
        
        UpdateMovementDestination();
        RotateTowardsTargetDirection();
        // RotateTowardsMoveDirection();
        
        
        /*
        if(aimAtFpsModel != null)
        {
            LookAtTarget();
            if(!reactionCooldown.IsOnCooldown(true))
            {
                if(weaponShotCooldown.CanExecuteAfterDeltaTime())
                {
                    Transform aimTransform = GetVisibleTransformFromTarget(aimAtFpsModel);
                    if(aimTransform != null)
                        ShootAtTarget();
                    else
                        SetAimTarget(null);         // Lost sight during shooting.
                }
            }
        }
        else
        {
            if (currBotState == BotStateEnum.Alert)
            {
                if(alertStateCooldown.IsOnCooldown())
                {
                    alertStateCooldown.ReduceCooldown(Time.deltaTime);
                    RotateTowardsAlertRotation();
                }
                else
                {
                    currBotState = BotStateEnum.Default;
                }
            }
        }
        
        if(scanCooldown.IsOnCooldown())
            scanCooldown.ReduceCooldown(Time.deltaTime);
        else
        {
            FindTargetInSight();
            HandleMovementDestination();
            scanCooldown.StartCooldown();
        }
        
        if(currBotState != BotStateEnum.Alert)
        RotateTowardsMoveDirection();
        */
    }
    
    private void RotateTowardsTargetDirection()
    {
        RotateModelTowardsDirection(botFsm.targetLookAtDirection);
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
        if(botFsm.botState == BotStateEnum.Default)
            lookAtDirection = GetMovementVelocity().normalized;
        else if (botFsm.botState == BotStateEnum.Alert)
            lookAtDirection = botFsm.targetLookAtDirection;
        
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
    
    private void RotateTowardsMoveDirection()
    {
        if(aimAtFpsModel != null)   return;
        
        Vector3 dir = GetMovementVelocity().normalized;
        RotateModelTowardsDirection(dir);
    }
    
    private void RotateTowardsAlertRotation()
    {
        RotateModelTowardsDirection(alertLookDirection);
    }
    
    private void RotateModelTowardsDirection(Vector3 dir)
    {
        if(dir == Vector3.zero)   return;
        
        
        Quaternion lookAtTargetRotation = Quaternion.LookRotation(dir);
        lookAtRotationTransform.rotation = Quaternion.RotateTowards(lookAtRotationTransform.rotation , lookAtTargetRotation , 200f * Time.deltaTime);
        // Offset y to 0 so model doesn't rotate up/down
        Quaternion targetDir = Quaternion.LookRotation(new Vector3(dir.x , 0f , dir.z));
        
        modelObjectParent.transform.rotation = Quaternion.RotateTowards(modelObjectParent.transform.rotation , targetDir , 200f * Time.deltaTime);
        
    }

    /*
    private void FindTargetInSight()
    {
        if(!isServer || visionSensor == null)    return;
        
        FpsModel target = GetTargetAimModelFromSensor();
        if(target != null)
        {            
            SetAimTarget(target);
            Transform t = GetVisibleTransformFromTarget(target);
            if(t != null)
                SetShootTransform(t);
        }
        else
        {
            SetAimTarget(null);
            SetShootTransform(null);
        }
    }
    */
    
    /*
    private FpsModel GetTargetAimModelFromSensor()
    {
        // Should detect "FpsModel" attached in the ModelRoot , because it contains the LOS Target.
        List<FpsModel> detectedModels = visionSensor.GetDetectedByComponent<FpsModel>();
        if(detectedModels != null && detectedModels.Count > 0)
        {
            foreach(FpsModel detectedModel in detectedModels)
            {
                if (!(detectedModel.controllerEntity is FpsCharacter))
                    continue;
                    
                FpsCharacter detectedCharacter = (FpsCharacter)detectedModel.controllerEntity;
                if(!detectedModel.controllerEntity.IsDead() && !(detectedCharacter.team == team))
                {
                    return detectedModel;
                }
            }
        }
        return null;
    }
    */
    
    /*
    private Transform GetVisibleTransformFromTarget(FpsModel targetFpsModel)
    {
        if(targetFpsModel.controllerEntity.IsDead())    return null;
        
        // Let's just assume all aim target is FpsCharacter first.
        visionSensor.Pulse();
        List<Transform> tList = visionSensor.GetVisibleTransforms(targetFpsModel.gameObject);
        if(tList != null && tList.Count > 0)
        {
            Transform t = Utils.GetRandomElement<Transform>(tList);
            return t;
        }
        return null;
    }
    */
    
    private void LookAtTarget()
    {
        RotateModelTowardsDirection(Utils.GetDirection(modelObjectParent.transform.position, aimAtFpsModel.transform.position));
    }
    
    [Server]
    private void ShootAtTarget(FpsModel targetModel , Transform targetTransform)
    {
        if(fpsWeapon != null && targetTransform != null)
        {
            if(fpsWeapon.weaponCategory == WeaponCategory.Shotgun)
            {
                for(int i = 0 ; i < fpsWeapon.palletPerShot ; i++)
                {
                    RayHitInfo rayHitInfo = Utils.CastRayAndGetHitInfo(lookAtRotationTransform , LayerMask.GetMask(Constants.LAYER_HITBOX , Constants.LAYER_GROUND, Constants.LAYER_LOCAL_PLAYER_HITBOX), fpsWeapon.spread);
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
            else
            {
                float hitChanceByVisibility = visionSensor.GetVisibility(targetModel.gameObject);
                float hitChanceByDifficulty = 0.4f;
                float hitChanceByDistance = GetHitChanceByDistance(targetModel.transform.position);
                float finalHitChance = hitChanceByVisibility * hitChanceByDifficulty * hitChanceByDistance;
                // Debug.Log("Final hit chance = " + finalHitChance);
                bool isHit = Utils.WithinChance(finalHitChance);
                if(isHit)
                {
                    // shootAtTransform should be a FpsHitbox
                    FpsHitbox hitbox = shootAtTransform.GetComponent<FpsHitbox>();
                    FpsEntity fpsEntity = hitbox.fpsEntity;
                
                    DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon , hitbox , hitbox.transform.position);
                
                    fpsEntity.TakeDamage(dmgInfo);
                    fpsWeapon.FireWeapon(shootAtTransform.position);
                }
                else
                {
                    Vector3 positionOffset = shootAtTransform.position + Random.insideUnitSphere * 0.5f;
                    fpsWeapon.FireWeapon(positionOffset);
                }
            }
        }
    }
    
    private float GetHitChanceByDistance(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(transform.position , targetPosition);
        float multiplier = hitChanceDistanceBase/distance;
        return multiplier;
    }
        
    private void SetShootTransform(Transform t)
    {
        shootAtTransform = t;
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
        
        /*
        if(damageInfo.damageSourcePosition == Vector3.zero) return;
        
        if(currBotState != BotStateEnum.Aiming)
        {
            currBotState = BotStateEnum.Alert;
            alertStateCooldown.StartCooldown();
            
            alertLookDirection = (damageInfo.damageSourcePosition - transform.position).normalized;
        }
        */
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
}
