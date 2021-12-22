using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Animancer;

// A FpsCharacter should 
// -  have a Humanoid model , with weaponRoot component defined in hand for weapon world model
// -  be able to hold FpsWeapon
// -  perform weapon action with FpsWeapon

public abstract partial class FpsCharacter : FpsEntity
{

    protected FpsModel fpsModel;
    protected GameObject modelObject;
    protected GameObject modelObjectParent;
    [SerializeField] protected Transform lookAtTransform; 
    [SyncVar] public string characterName;
    
    [SyncVar] public CharacterStateEnum currState;
    [SerializeField] protected CharacterResources charRes;
    
    [SerializeField] protected List<Behaviour> disableBehaviorOnDeathList = new List<Behaviour>();
    [SerializeField] protected List<GameObject> disableGameObjectOnDeathList = new List<GameObject>();
    
    
    protected MovementDirection currMoveDir = MovementDirection.None;
        
    [SyncVar] protected Vector3 currentVelocity = Vector3.zero;
    [SyncVar] public TeamEnum team = TeamEnum.Blue;
    

    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        weaponRootTransform = GetComponentInChildren<CharacterWeaponRoot>().transform;
    }
    
    protected override void Start()
    {
        base.Start();
        AttachModel();
        SetRagdollState(false);
        
        SharedContext.Instance.RegisterCharacter(this);
        
        Start_Weapon();
        Start_Animation();
    }
    
    protected virtual void AttachModel()
    {
        fpsModel = GetComponentInChildren<FpsModel>(true);
        modelObject = fpsModel.gameObject;
        modelObjectParent = modelObject.transform.parent.gameObject;
        
        modelAnimancer = modelObject.GetComponent<AnimancerComponent>();
        fpsModel.SetLookAtTransform(lookAtTransform);
    }
        
    [Server]
    public virtual void Respawn()
    {
        currState = CharacterStateEnum.None;
        SetHealth(maxHealth);
        SetControllableState(true);
        RpcRespawn();
        RpcSwitchWeapon(activeWeaponSlot);
        ResetAllWeapons();
    }
    
    [ClientRpc]
    public void RpcRespawn()
    {
        SetControllableState(true);
        SetupComponentsByNetworkSetting();
        ResetAllWeapons();
        Respawn_Animation();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(IsDead())    return;

        Update_Weapon();
        Update_Animation();
    }

       
    [ClientRpc]
    protected void RpcPlayAnimation(ClipTransition clip)
    {
        currentPlayingClip = clip;
        modelAnimancer.Play(clip , 0.1f , FadeMode.FromStart);
    }
    
    [ClientRpc]
    protected override void RpcTakeDamage(DamageInfo damageInfo)
    {
        LocalSpawnManager.Instance.SpawnBloodFx(damageInfo.hitPoint);
    }
    
    [Server]
    protected override void Killed(DamageInfo damageInfo)
    {
        base.Killed(damageInfo);
        PlayerManager.Instance.QueueRespawn(this);
        ServerContext.Instance.UpdateCharacterKilledEvent(this , damageInfo);
    }
    
    [ClientRpc]
    protected override void RpcKilled(DamageInfo damageInfo)
    {
        base.RpcKilled(damageInfo);
        
        SetControllableState(false);
        FpsUiManager.Instance.AddNewKillListing(damageInfo.damageSource , characterName);
    }
    
    protected void SetControllableState(bool controllable)
    {
        SetRagdollState(!controllable);
        SetComponentsControllable(controllable);
    }
    
    protected void SetComponentsControllable(bool controllable)
    {
        foreach(Behaviour b in disableBehaviorOnDeathList)
            b.enabled = controllable;
        
        foreach(GameObject obj in disableGameObjectOnDeathList)
            obj.SetActive(controllable);
    }
        
    protected virtual void SetRagdollState(bool isRagdollState)
    {
        if(modelAnimancer != null)
        {
            modelAnimancer.enabled = isRagdollState ? false : true;
            modelObject.GetComponent<Animator>().enabled = isRagdollState ? false : true;
            
            if(!isRagdollState)
            {
                modelObject.transform.localPosition = Vector3.zero;
                modelObject.transform.localEulerAngles = Vector3.zero;
            }
        }
        
        if(fpsModel != null)
        {
            fpsModel.ToggleLookAt(!isRagdollState);
        }
            
        // Don't turn the bounding capsule collider in model-root to ragdoll , only children
        Rigidbody[] rbJoints = modelObject.GetComponentsInChildren<Rigidbody>(true);
        foreach(Rigidbody rb in rbJoints)
        {
            rb.isKinematic = isRagdollState ? false : true;
            rb.useGravity = isRagdollState ? true : false;         
        }
        
        Collider[] jointColliders = modelObject.GetComponentsInChildren<Collider>(true);
        foreach(Collider c in jointColliders)
        {
            c.isTrigger = isRagdollState ? false : true;
        }
    }
    
    protected MovementDirection GetMovementDirection()
    {
        Vector3 vecNormalized = GetMovementVelocity().normalized;

        // ----Pure WASD direction checking---- //
        
        float dotProductFront = Vector3.Dot(vecNormalized , modelObject.transform.forward);
        if(dotProductFront == 1)
            return MovementDirection.Front;
        else if (dotProductFront == -1)
            return MovementDirection.Back;
        else
        {
            float dotProuctRight = Mathf.Round(Vector3.Dot(vecNormalized , modelObject.transform.right));
            if(dotProuctRight == 1)
                return MovementDirection.Right;
            else if (dotProuctRight == -1)
                return MovementDirection.Left;
        }
        // --------------------------------------- //
        if(vecNormalized.x * modelObject.transform.forward.x < 0 
            && vecNormalized.z * modelObject.transform.forward.z < 0)
            return MovementDirection.Back;
        else
            return MovementDirection.Front;
    }
    
    public abstract Vector3 GetMovementVelocity();
        
    [Command]
    protected void CmdSetVelocity(Vector3 velocity)
    {
        currentVelocity = velocity;
    }
        
    protected void OnDestroy()
    {
        SharedContext.Instance.RemoveCharacter(this);
    }
}
