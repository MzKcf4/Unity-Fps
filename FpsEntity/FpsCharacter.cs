using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Animancer;

// A FpsCharacter should have a Humanoid model , with weaponRoot component defined in hand for weapon world model

public abstract class FpsCharacter : FpsEntity
{
    protected AnimancerComponent modelAnimancer;
    
    protected FpsModel fpsModel;
    protected GameObject modelObject;
    protected GameObject modelObjectParent;
    [SerializeField] protected Transform lookAtTransform; 
    public string characterName;
    
    [SyncVar] protected CharacterStateEnum currState;
    [SerializeField] protected CharacterResources charRes;
    
    [SerializeField] protected List<Behaviour> disableBehaviorOnDeathList = new List<Behaviour>();
    [SerializeField] protected List<GameObject> disableGameObjectOnDeathList = new List<GameObject>();
    
    protected Transform weaponRootTransform;
    private Vector3 lastFrameVelocity = Vector3.zero;
    private MovementDirection prevMoveDir = MovementDirection.None;
    
    public TeamEnum team = TeamEnum.None;
    
    protected override void Start()
    {
        base.Start();
        AttachModel();
        SetRagdollState(false);
        weaponRootTransform = GetComponentInChildren<CharacterWeaponRoot>().transform;
        
        SharedContext.Instance.RegisterCharacter(this);
    }
    
    protected virtual void AttachModel()
    {
        fpsModel = GetComponentInChildren<FpsModel>(true);
        modelObject = fpsModel.gameObject;
        modelObjectParent = modelObject.transform.parent.gameObject;
        
        modelAnimancer = modelObject.GetComponent<AnimancerComponent>();
    }
    
    public virtual void Respawn()
    {
        currState = CharacterStateEnum.None;
        SetHealth(maxHealth);
        SetControllableState(true);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        HandleMovementAnimation();
    }
    
    private void HandleMovementAnimation()
    {
        
        if(charRes == null) return;
        
        if(CanPlayIdleAnimation())
        {
            currState = CharacterStateEnum.Idle;
            prevMoveDir = MovementDirection.None;
            PlayAnimation(charRes.idleClip);
        } 
        else if (CanPlayRunAnimation())
        {
            currState = CharacterStateEnum.Run;
            MovementDirection dir = GetMovementDirection();
            if(prevMoveDir != dir)
            {
                prevMoveDir = dir;
                if(dir == MovementDirection.Front)
                    PlayAnimation(charRes.runClip);
                else if(dir == MovementDirection.Back)
                    PlayAnimation(charRes.runClipBack);
                else if(dir == MovementDirection.Left)
                    PlayAnimation(charRes.runClipLeft);
                else if(dir == MovementDirection.Right)
                    PlayAnimation(charRes.runClipRight);
            }
        }
    }
    
    private bool CanPlayIdleAnimation()
    {
        return !IsDead() && 
        (currState != CharacterStateEnum.Idle && 
            currState != CharacterStateEnum.Dead && 
            GetMovementVelocity().magnitude == 0f);
    }
    
    private bool CanPlayRunAnimation()
    {
        return !IsDead() && 
        (currState != CharacterStateEnum.Dead &&
            GetMovementVelocity().magnitude > 0f);
    }
    
    protected void PlayAnimation(ClipTransition clip)
    {
        modelAnimancer.Play(clip , 0.1f , FadeMode.FromStart);
    }
    
    [ClientRpc]
    protected override void RpcTakeDamage(DamageInfo damageInfo)
    {
        LocalSpawnManager.Instance.SpawnBloodFx(damageInfo.hitPoint);
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
}
