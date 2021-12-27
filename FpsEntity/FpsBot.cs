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
            
            Start_Fsm();
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
    }

    public override void Respawn()
    {
        base.Respawn();
        // Reset the dto data for FSM
        botFsmDto.Clear();
        TransitToState(BotStateEnum.Wandering);
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

    public void SetDestination(Transform targetTransform)
    {
        aiDest.SetByTransform(targetTransform);
    }

    public void SetDestination(Vector3 position)
    {
        aiDest.SetByPosition(position);
    }

    public void StopMoving()
    {
        aiDest.SetByTransform(transform);
    }
    
    [Server]
    public void ShootAtTarget()
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
        List<FpsCharacter> fpsCharacterList = SharedContext.Instance.characterList;
        foreach (FpsCharacter fpsCharacter in fpsCharacterList)
        {
            if (!(fpsCharacter is FpsBot)) continue;

            FpsBot otherBot = (FpsBot)fpsCharacter;
            if (otherBot.team != team || otherBot.IsDead() || otherBot == this) continue;

            otherBot.OnTeammateKilled(transform.position, damageInfo);
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
        // ProcessWeaponEventUpdate_Fsm(evt);
        if(evt == WeaponEvent.Shoot)
        {
            
            float spreadMultiplier = GetActiveWeapon().spread;
            // ---------ToDo: Shotgun pallet ??--------------- //
            if(GetActiveWeapon().weaponCategory != WeaponCategory.Shotgun)
            {
                spreadMultiplier *= 5f;
            }
            Vector3 shootDirection = Utils.GetDirection( fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform.position, lookAtTransform.position);
            CoreGameManager.Instance.DoWeaponRaycast(this , GetActiveWeapon() , fpsWeaponWorldSlot[activeWeaponSlot].muzzleTransform.position, shootDirection);
            // ----------------------------------------------- //
            RpcFireWeapon();
        }
    }
}
