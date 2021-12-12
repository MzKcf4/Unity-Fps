using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FpsBot.Fsm
public partial class FpsBot
{
    public BotStateEnum botState = BotStateEnum.Default;
    
    private ActionCooldown alertStateCooldown = new ActionCooldown { interval = 2f};
    private ActionCooldown reactionCooldown = new ActionCooldown { interval = 2f};
    private ActionCooldown scanCooldown = new ActionCooldown { interval = 0.2f};
    
    // How long bot stays in alert state ( look at alert direction )
    public float maxAlertTime;
    // How long bot stays "Aiming" enemy before shooting
    public float reactionTime;
    
    public Vector3 targetLookAtPosition;
    
    public FpsModel aimAtFpsModel;
    public Transform aimAtHitboxTransform;
    
    // Should be placed in MonoBehaviour's Update() method , to replicate update() behavior
    public void Update_Fsm()
    {
        if(aiIgnoreEnemy)  return;
        
        CheckWeaponAmmo();
        if(botState == BotStateEnum.Reloading)  return;
        
        if(botState == BotStateEnum.Alert)
        {
            if(alertStateCooldown.CanExecuteAfterDeltaTime())
                botState = BotStateEnum.Default;
        } 
        else if(botState == BotStateEnum.Aiming)
        {
            if(reactionCooldown.CanExecuteAfterDeltaTime())
            {
                botState = BotStateEnum.Shooting;
            }
        }
        
        
        if (botState == BotStateEnum.Alert || botState == BotStateEnum.Default)
        {
            if(scanCooldown.CanExecuteAfterDeltaTime(true))
            {
                FpsModel foundFpsModel = ScanForShootTarget();
                if(foundFpsModel != null)
                {
                    aimAtFpsModel = foundFpsModel;
                    botState = BotStateEnum.Aiming;
                    reactionCooldown.StartCooldown();
                }
            }
        }
        
        UpdateLookAt();
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
        
        reactionCooldown.interval = reactionTime;
    }
    
    private void CheckWeaponAmmo()
    {
        if(GetActiveWeapon().currentClip <= 0)
        {
            ReloadActiveWeapon();
            RpcReloadActiveWeapon();
            botState = BotStateEnum.Reloading;
        }
    }
        
    private void UpdateLookAt_Fsm()
    {
        if(botState == BotStateEnum.Aiming)
            targetLookAtPosition = aimAtFpsModel.transform.position + new Vector3(0,1f,0);
        else if (botState == BotStateEnum.Shooting)
        {
            if(aimAtHitboxTransform == null)
                targetLookAtPosition = aimAtFpsModel.transform.position + new Vector3(0,1f,0);
            else
                targetLookAtPosition = aimAtHitboxTransform.position;
        } else if (botState != BotStateEnum.Alert)
            targetLookAtPosition = Vector3.zero;
    }
    
    
    private FpsModel ScanForShootTarget()
    {
        if(visionSensor == null)   
            return null;
            
        // Should detect "FpsModel" attached in the ModelRoot , because it contains the LOS Target.
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
    
    // Called from FpsBot , when it's about to fire the weapon.
    public void ScanVisibleLosFromShootTarget()
    {
        if(visionSensor == null)
            return;
        
        // The target has been killed ... reset to normal state
        if(aimAtFpsModel.controllerEntity.IsDead())
        {
            OnShootTargetLostSight();
            return;
        }
        
        // Let's just assume all aim target is FpsCharacter first.
        visionSensor.Pulse();
        List<Transform> tList = visionSensor.GetVisibleTransforms(aimAtFpsModel.gameObject);
        if(tList != null && tList.Count > 0)
        {
            aimAtHitboxTransform = Utils.GetRandomElement<Transform>(tList);
            return;
        }
        
        // Completely lost sight from ShootTarget
        OnShootTargetLostSight();
    }
    
    
    public void OnTakeHit(DamageInfo damageInfo)
    {
        if(damageInfo.damageSourcePosition == Vector3.zero) return;
        
        if(botState != BotStateEnum.Aiming && botState != BotStateEnum.Shooting)
        {
            botState = BotStateEnum.Alert;
            alertStateCooldown.StartCooldown();
            
            targetLookAtPosition = damageInfo.damageSourcePosition;
        }
    }
    
    public void OnShootTargetLostSight()
    {
        botState = BotStateEnum.Default;
        aimAtFpsModel = null;
        aimAtHitboxTransform = null;
    }
    
    public void ProcessWeaponEventUpdate_Fsm(WeaponEvent evt)
    {
        if(evt == WeaponEvent.Reload)
            botState = BotStateEnum.Reloading;
        else if(evt == WeaponEvent.Reload_End)
            botState = BotStateEnum.Default;
    }
}
