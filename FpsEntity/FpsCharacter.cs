using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Animancer;
using UnityEngine.Events;
using EasyCharacterMovement;

// A FpsCharacter manges 
// -  character models
// -  character locomotion / animations

public class FpsCharacter : FpsEntity
{
    public FpsModel FpsModel { get { return fpsModel; } }
    public bool IsLookAtWeaponAim { get { return isLookAtWeaponAim; }
                                    set { isLookAtWeaponAim = value; }}

    protected CharacterCommonResources characterCommonResources;
    protected FpsModel fpsModel;
    [SerializeField] private GameObject fpsModelPrefab;

    protected GameObject modelObject;
    protected GameObject modelObjectParent;
    protected bool canRagdoll = true;

    protected bool isLookAtWeaponAim = false;
    // Need to be synchorinzed for up / down rotation
    [SerializeField] protected Transform weaponAimAt;

    // Objects that you want to rotate with model itself
    [SerializeField] protected Transform attachToModel;
    
    // [SerializeField] protected CharacterResources charRes;
    
    [SerializeField] protected List<Behaviour> disableBehaviorOnDeathList = new List<Behaviour>();
    [SerializeField] protected List<GameObject> disableGameObjectOnDeathList = new List<GameObject>();
    
    [SyncVar] public string characterName;
    [SyncVar] protected Vector3 currentVelocity = Vector3.zero;
    [SyncVar] public TeamEnum team = TeamEnum.Blue;

    [SerializeField] protected CharacterAnimationResource charAnimationRes;
    protected Character ecmCharacter;
    protected MzCharacterAnimator characterAnimator;
    [SerializeField] protected bool useDeathAnimation = false;
    
    protected AudioSource audioSourceWeapon;
    protected AudioSource audioSourceCharacter;


    [HideInInspector] public UnityEvent onSpawnEvent = new UnityEvent();

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        characterCommonResources = WeaponAssetManager.Instance.GetCharacterCommonResources();
        audioSourceWeapon = gameObject.AddComponent<AudioSource>();
        audioSourceCharacter = gameObject.AddComponent<AudioSource>();
        InitializeAudioSource(audioSourceWeapon);
        InitializeAudioSource(audioSourceCharacter);
    }
    
    protected override void Awake()
    {
        base.Awake();
        
    }
    
    protected override void Start()
    {
        base.Start();
        ecmCharacter = GetComponent<Character>();
        characterAnimator = GetComponent<MzCharacterAnimator>();

        AttachModel();
        SetRagdollState(false);
        
        SharedContext.Instance.RegisterCharacter(this);
    }
    
    protected virtual void AttachModel()
    {
        if (fpsModelPrefab != null)
        {
            GameObject fpsModelObj = GameObject.Instantiate(fpsModelPrefab , transform);
            // fpsModelObj.transform.parent = transform;

            fpsModel = fpsModelObj.GetComponent<FpsModel>();
        }
        else
        {
            fpsModel = GetComponentInChildren<FpsModel>(true);
        }

        
        if (fpsModel == null && charAnimationRes != null && charAnimationRes.characterModelPrefab != null)
        {
            modelObject = Instantiate(charAnimationRes.characterModelPrefab, transform);
            fpsModel = modelObject.GetComponent<FpsModel>();
        }

        modelObject = fpsModel.gameObject;
        modelObjectParent = modelObject.transform.parent.gameObject;

        fpsModel.SetLookAtTransform(weaponAimAt);
        if (attachToModel != null)
        {
            attachToModel.SetParent(modelObject.transform);
        }

        characterAnimator.SetAttachedModel(fpsModel);
        // Apply highlight effect to newly added model's mesh
        highlightEffect.Refresh();
    }
        
    [Server]
    public virtual void Respawn()
    {
        // currState = CharacterStateEnum.None;
        SetHealth(maxHealth);
        SetControllableState(true);
        SharedContext.Instance.characterSpawnEvent.Invoke(this);
        RpcRespawn();

        onSpawnEvent.Invoke();
    }
    
    [ClientRpc]
    public virtual void RpcRespawn()
    {
        SetControllableState(true);
        SetupComponentsByNetworkSetting();
        // Respawn_Animation();

        onSpawnEvent.Invoke();
        SharedContext.Instance.characterSpawnEvent.Invoke(this);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(IsDead())    return;

        SyncMovementVelocity();
    }

    [Server]
    public void ServerPlayAnimationByKey(string key)
    {
        RpcPlayAnimationByKey(key);
    }

    [ClientRpc]
    protected void RpcPlayAnimationByKey(string key)
    {
        PlayAnimationByKey(key);
    }

    public void PlayAnimationByKey(string key) 
    {
        if (!charAnimationRes.actionClips.ContainsKey(key)) {
            Debug.LogWarning("Action clip key not found : " + key);
            return;
        }

        characterAnimator.PlayActionAnimation(charAnimationRes.actionClips[key].actionClip);

    }
           
    [ClientRpc]
    protected override void RpcTakeDamage(DamageInfo damageInfo)
    {
        LocalSpawnManager.Instance.SpawnBloodFx(damageInfo.hitPoint);

        AudioClip hurtSoundClip =
            damageInfo.bodyPart == BodyPart.Head ? Utils.GetRandomElement<AudioClip>(characterCommonResources.hurtHeadShotSoundList)
                                                 : Utils.GetRandomElement<AudioClip>(characterCommonResources.hurtSoundList);

        audioSourceCharacter.PlayOneShot(hurtSoundClip);
    }
    
    [Server]
    protected override void Killed(DamageInfo damageInfo)
    {
        base.Killed(damageInfo);
        ServerContext.Instance.UpdateCharacterKilledEvent(this , damageInfo);
    }
    
    [ClientRpc]
    protected override void RpcKilled(DamageInfo damageInfo)
    {
        base.RpcKilled(damageInfo);
        
        SetControllableState(false);
        FpsUiManager.Instance.AddNewKillListing(damageInfo , characterName);

        if (useDeathAnimation && charAnimationRes.deathClips.Count > 0)
        {
            characterAnimator.PlayDeathAnimation(Utils.GetRandomElement<ClipTransition>(charAnimationRes.deathClips));
        }
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
        
        if (canRagdoll)
        {
            if (modelObject != null)
            {
                if(!useDeathAnimation)
                    characterAnimator.enabled = isRagdollState ? false : true;

                if (!isRagdollState)
                {
                    modelObject.transform.localPosition = Vector3.zero;
                    modelObject.transform.localEulerAngles = Vector3.zero;
                }
            }

            if (fpsModel != null)
            {
                fpsModel.ToggleLookAt(!isRagdollState);
            }


            // Don't turn the bounding capsule collider in model-root to ragdoll , only children
            Rigidbody[] rbJoints = modelObject.GetComponentsInChildren<Rigidbody>(true);
            foreach (Rigidbody rb in rbJoints)
            {
                rb.isKinematic = isRagdollState ? false : true;
                rb.useGravity = isRagdollState ? true : false;
            }
        }

        // The capsule collider of character
        Collider boundCollider = GetComponent<Collider>();
        boundCollider.isTrigger = isRagdollState ? true : false;

        FpsHitbox[] hitboxes = modelObject.GetComponentsInChildren<FpsHitbox>(true);
        foreach (FpsHitbox hitbox in hitboxes)
        {
            Collider c = hitbox.GetComponent<Collider>();
            c.isTrigger = isRagdollState ? false : true;
            c.gameObject.layer = isRagdollState ? LayerMask.NameToLayer(Constants.LAYER_IGNORE_RAYCAST)
                                                : LayerMask.NameToLayer(GetHitboxLayerName());
        }
            
        modelObject.gameObject.layer = isRagdollState ? LayerMask.NameToLayer(Constants.LAYER_IGNORE_RAYCAST)
                                                      : LayerMask.NameToLayer(GetModelLayerName());
    }

  
    protected virtual string GetModelLayerName()
    {
        return Constants.LAYER_CHARACTER_MODEL;
    }

    protected virtual string GetHitboxLayerName()
    {
        return Constants.LAYER_HITBOX;
    }

    protected virtual void SyncMovementVelocity()
    {
        if (isServer)
            currentVelocity = GetMovementVelocity();
    }

    [Command]
    protected void CmdSyncMovementVelocity(Vector3 velocity)
    {
        currentVelocity = velocity;
    }

    public virtual Vector3 GetMovementVelocity() 
    { 
        return ecmCharacter.GetVelocity();
    }
        
    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }
        
    protected virtual void OnDestroy()
    {
        SharedContext.Instance.RemoveCharacter(this);
    }

    protected void InitializeAudioSource(AudioSource audioSource)
    {
        audioSource.outputAudioMixerGroup = LocalPlayerContext.Instance.audioMixerGroup;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f;
        audioSource.spread = 1f;
    }

    public void AimAtMovementDirection()
    {
        Vector3 moveVec = GetMovementVelocity().normalized * 2f;
        
        if (moveVec != Vector3.zero)
            weaponAimAt.localPosition = new Vector3(moveVec.x, 1.3f + moveVec.y, moveVec.z);
    }

    public void AimAtPosition(Vector3 pos)
    {
        weaponAimAt.position = pos;
    }
}
