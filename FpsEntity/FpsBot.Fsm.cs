using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FpsBot.Fsm
public partial class FpsBot
{
    public BotStateEnum botState = BotStateEnum.Wandering;
    private readonly Dictionary<BotStateEnum, AbstractBotStateProcessor> dictStateToProcessor = new Dictionary<BotStateEnum, AbstractBotStateProcessor>();
    [SerializeField] private readonly BotFsmDto botFsmDto = new BotFsmDto();
    private AbstractBotStateProcessor activeProcessor;

    // How long bot stays "Aiming" enemy before shooting
    public float reactionTime;
    // The chance bot will chase enemy ( go to last seen position )
    public float chaseChance = 1.0f;

    public void Start_Fsm()
    {
        dictStateToProcessor.Add(BotStateEnum.Wandering, new BotWanderStateProcessor(this, botFsmDto));
        dictStateToProcessor.Add(BotStateEnum.Engage, new BotEngageStateProcessor(this, botFsmDto));
        dictStateToProcessor.Add(BotStateEnum.Chasing, new BotChaseStateProcessor(this, botFsmDto));
        dictStateToProcessor.Add(BotStateEnum.ReactToUnknownDamage, new BotReactToUnknownDamageStateProcessor(this, botFsmDto));
        TransitToState(BotStateEnum.Wandering);
    }

    // Should be placed in MonoBehaviour's Update() method , to replicate update() behavior
    public void Update_Fsm()
    {
        if (activeProcessor == null) return;
        activeProcessor.ProcessState();
    }

    public void TransitToState(BotStateEnum newState)
    {
        botState = newState;
        activeProcessor = dictStateToProcessor[newState];
        activeProcessor.EnterState();
    }


    public void SetSkillLevel(int level)
    {
        if(level <= 0)
            reactionTime = 2f;
        else if (level == 1)
            reactionTime = 1f;
        else if (level == 2)
            reactionTime = 0.5f;
        else 
            reactionTime = 0.3f;
    }
    
    private void CheckWeaponAmmo()
    {
        if(GetActiveWeapon().currentClip <= 0)
        {
            ReloadActiveWeapon();
            RpcReloadActiveWeapon();
        }
    }
    
    public void OnTeammateKilled(Vector3 deathPos, DamageInfo damageInfo)
    {
        activeProcessor.OnTeammateKilled(deathPos, damageInfo);
    }

    public bool IsReachedDesination()
    {
        return ai.reachedDestination || !ai.hasPath;
    }

    public void AlignLookAtWithMovementDirection()
    {
        Vector3 moveVec = GetMovementVelocity().normalized * 2f;
        if (moveVec != Vector3.zero)
            lookAtTransform.localPosition = new Vector3(moveVec.x, 1.3f + moveVec.y, moveVec.z);
    }

    public void SetLookAtToPosition(Vector3 pos)
    {
        lookAtTransform.position = pos;
    }
        
    public FpsModel ScanForShootTarget()
    {
        if(visionSensor == null || aiIgnoreEnemy)
            return null;

        // Should detect "FpsModel" attached in the ModelRoot , because it contains the LOS Target.
        visionSensor.Pulse();
        List<FpsModel> detectedModels = visionSensor.GetDetectedByComponent<FpsModel>();
        
        if(detectedModels == null || detectedModels.Count == 0)     
            return null;
        
        
        foreach(FpsModel detectedModel in detectedModels)
        {
            if (!(detectedModel.controllerEntity is FpsCharacter))
                continue;
                
            FpsCharacter detectedCharacter = (FpsCharacter)detectedModel.controllerEntity;
            if(!detectedModel.controllerEntity.IsDead() && !(detectedCharacter.team == this.team))
            {
                return detectedModel;
            }
        }
        
        return null;
    }

    public Transform GetVisibleHitBoxFromAimTarget(GameObject targetObject)
    {
        if (visionSensor == null)   return null;

        // Let's just assume all aim target is FpsCharacter first.
        visionSensor.Pulse();
        List<Transform> tList = visionSensor.GetVisibleTransforms(targetObject);
        if (tList != null && tList.Count > 0)
        {
            return Utils.GetRandomElement<Transform>(tList);
        }
        return null;
    }
    
    public void OnTakeHit(DamageInfo damageInfo)
    {
        if(damageInfo.damageSourcePosition == Vector3.zero) return;
        activeProcessor.OnTakeHit(damageInfo);
    }

    public void ProcessWeaponEventUpdate_Fsm(WeaponEvent evt)
    {
        /*
        if(evt == WeaponEvent.Reload)
            botState = BotStateEnum.Reloading;
        else if(evt == WeaponEvent.Reload_End)
            botState = BotStateEnum.Default;
        */
    }
}
