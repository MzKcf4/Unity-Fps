using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Animancer;
using UnityEngine.Events;
using EasyCharacterMovement;
using Pathfinding;
using Pathfinding.RVO;

// A FpsCharacter manges 
// -  character models
// -  character locomotion / animations

public class FpsCharacter : FpsEntity
{
    public Dictionary<string, string> AdditionalInfos { get { return dictAdditionalInfo; } }
    public Dictionary<string, object> AdditionalInfoObjects { get { return dictAdditionalInfoObject; } }
    
    public FpsModel FpsModel { get { return fpsModel; } }
    public bool IsLookAtWeaponAim { get { return isLookAtWeaponAim; }
                                    set { isLookAtWeaponAim = value; }}
    public float MaxSpeed { get { return maxSpeed; }}

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
    
    [SerializeField] protected List<Behaviour> disableBehaviorOnDeathList = new List<Behaviour>();
    [SerializeField] protected List<GameObject> disableGameObjectOnDeathList = new List<GameObject>();
    private RVOController rvoController;
    
    [SyncVar] public string characterName;
    [SyncVar] protected Vector3 currentVelocity = Vector3.zero;
    [SyncVar] public TeamEnum team = TeamEnum.Blue;

    [SyncVar(hook = (nameof(OnMaxSpeedChanged)))]
    protected float maxSpeed;
    protected float currentMaxSpeed;

    [SerializeField] protected CharacterAnimationResource charAnimationRes;
    protected Character ecmCharacter;
    protected CharacterMovement characterMovement;
    protected MzCharacterAnimator characterAnimator;
    [SerializeField] protected bool useDeathAnimation = false;
    
    protected AudioSource audioSourceWeapon;
    protected AudioSource audioSourceCharacter;

    [HideInInspector] public UnityEvent onSpawnEvent = new UnityEvent();

    protected Dictionary<string, string> dictAdditionalInfo = new Dictionary<string, string>();
    protected Dictionary<string, System.Object> dictAdditionalInfoObject = new Dictionary<string, object>();

    protected MzAbilitySystem abilitySystem;
    protected Dictionary<string, VfxMarker> dictVfxMarkers = new Dictionary<string, VfxMarker>();


    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isServer)
        {
            DisableClientSideComponents();
        }

        characterCommonResources = WeaponAssetManager.Instance.GetCharacterCommonResources();
        audioSourceWeapon = gameObject.AddComponent<AudioSource>();
        audioSourceCharacter = gameObject.AddComponent<AudioSource>();
        InitializeAudioSource(audioSourceWeapon);
        InitializeAudioSource(audioSourceCharacter);
        
    }

    protected virtual void DisableClientSideComponents()
    {
        AIBase ai = GetComponent<AIBase>();
        if (ai != null)
            ai.enabled = false;

        Seeker seeker = GetComponent<Seeker>();
        if(seeker != null)
            seeker.enabled = false;

        AIDestinationSetter aiDest = GetComponent<AIDestinationSetter>();
        if(aiDest != null)
            aiDest.enabled = false;

        MzBotBrainBase botBrain = GetComponent<MzBotBrainBase>();
        if(botBrain != null)
            botBrain.enabled = false;

    }
    
    protected override void Awake()
    {
        base.Awake();
        ecmCharacter = GetComponent<Character>();
        characterMovement = GetComponent<CharacterMovement>();
        characterAnimator = GetComponent<MzCharacterAnimator>();
        rvoController = GetComponent<RVOController>();
    }
    
    protected override void Start()
    {
        base.Start();
        abilitySystem = GetComponent<MzAbilitySystem>();

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

        VfxMarker[] vfxMarkers = GetComponentsInChildren<VfxMarker>();
        for (int i = 0; i < vfxMarkers.Length; i++) {
            dictVfxMarkers.Add(vfxMarkers[i].MarkerName, vfxMarkers[i]);
        }
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
    public void ServerPlayAnimationByKey(string key, bool isFullBody, bool isForceExecute, float freezeTime = 0)
    {
        RpcPlayAnimationByKey(key, isFullBody , isForceExecute);
        if (freezeTime > 0) {
            SetCanMove(false);
            StartCoroutine(SetCanMoveAfter(true, freezeTime));
        }
    }

    [ClientRpc]
    protected void RpcPlayAnimationByKey(string key, bool isFullBody, bool isForceExecute)
    {
        PlayAnimationByKey(key, isFullBody , isForceExecute);
    }

    public void PlayAnimationByKey(string key, bool isFullBody, bool isForceExecute)
    {
        if (!charAnimationRes.actionClips.ContainsKey(key)) {
            Debug.LogWarning("Action clip key not found : " + key);
            return;
        }

        characterAnimator.PlayActionAnimation(charAnimationRes.actionClips[key].actionClip , isFullBody, isForceExecute);
    }

    [Server]
    public void PlayVfxInMarker(string markerName , string vfxKey , float vfxLifeTime)
    {
        RpcPlayVfxInMarker(markerName, vfxKey, vfxLifeTime);
    }

    [ClientRpc]
    protected void RpcPlayVfxInMarker(string markerName, string vfxKey, float vfxLifeTime)
    {
        if (!dictVfxMarkers.ContainsKey(markerName))
        {
            Debug.LogWarning("Marker not found : " + markerName);
            return;
        }

        VfxMarker marker = dictVfxMarkers[markerName];

        EffectManager.Instance.PlayEffect(vfxKey, marker.transform.position, vfxLifeTime , marker.transform);
    }

    [Server]
    public override void TakeDamage(DamageInfo damageInfo)
    {
        if (damageInfo.victim == null)
            damageInfo.victim = this;

        base.TakeDamage(damageInfo);
        HandlePainShock();

        // Only send to the damage dealer
        if (damageInfo.attacker is FpsPlayer)
            TargetSpawnDamageText(damageInfo.attackerNetIdentity.connectionToClient, damageInfo.damage, damageInfo.hitPoint,damageInfo.bodyPart == BodyPart.Head);
    }

    [TargetRpc]
    public void TargetSpawnDamageText(NetworkConnection target, int damage, Vector3 position, bool isHeadshot)
    {
        LocalSpawnManager.Instance.SpawnDamageText(damage, position + Vector3.up, isHeadshot);
    }

    // Called before this character deals damage to others
    protected virtual void PreDealDamage(DamageInfo damageInfo) 
    {
        if (abilitySystem != null)
            abilitySystem.OnOwnerPreDealDamage(damageInfo);
    }

    protected override void PreTakeDamage(DamageInfo damageInfo)
    {
        base.PreTakeDamage(damageInfo);

        if(damageInfo.attacker != null)
            damageInfo.attacker.PreDealDamage(damageInfo);

        if(abilitySystem != null)
            abilitySystem.OnOwnerPreTakeDamage(damageInfo);
        // if (abilitySystem != null)
            
    }

    protected virtual void PostDealDamage(DamageInfo damageInfo) 
    {
        if (abilitySystem != null)
            abilitySystem.OnOwnerPostDealDamage(damageInfo);
    }

    protected override void PostTakeDamage(DamageInfo damageInfo)
    {
        base.PostTakeDamage(damageInfo);
        damageInfo.attacker?.PostDealDamage(damageInfo);
        if (abilitySystem != null)
            abilitySystem.OnOwnerPostTakeDamage(damageInfo);
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

    protected virtual void HandlePainShock() 
    {
        SetVelocity(Vector3.zero);
    }
    
    [Server]
    protected override void Killed(DamageInfo damageInfo)
    {
        base.Killed(damageInfo);
        ServerContext.Instance.UpdateCharacterKilledEvent(this , damageInfo);
        MzCharacterManager.Instance.OnCharacterKilled.Invoke(this);
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

        ecmCharacter.enabled = !isRagdollState;

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

        if(rvoController != null)
            rvoController.enabled = !isRagdollState;
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
        // LocalPlayer sync its velocity to server.
        if (isLocalPlayer)
        {
            currentVelocity = GetMovementVelocity();
            CmdSyncMovementVelocity(GetMovementVelocity());
        }
        // Server syncs back velocity from other clients or bots to clients
        else if (isServer)
            currentVelocity = GetMovementVelocity();
        // RpcSyncMovementVelocity(GetMovementVelocity());
        // Then , for all clients , order the mover to move according to received velocity
        // else if (isClient)
        //    characterMovement.velocity = currentVelocity;

        /*
        if (isServer)
            currentVelocity = GetMovementVelocity();

        if (!isLocalPlayer)
            characterMovement.velocity = currentVelocity;
        */
    }

    [Command]
    protected void CmdSyncMovementVelocity(Vector3 velocity)
    {
        currentVelocity = velocity;
    }

    [ClientRpc]
    protected void RpcSyncMovementVelocity(Vector3 velocity)
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

    public virtual void SetMaxSpeed(float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        ecmCharacter.maxWalkSpeed = maxSpeed;
    }

    public virtual void SetCanMove(bool canMove)
    {
        ecmCharacter.SetMovementMode(canMove ? MovementMode.Walking : MovementMode.None);
        // this.maxSpeed = maxSpeed;
        // ecmCharacter.maxWalkSpeed = maxSpeed;
    }

    private void OnMaxSpeedChanged(float oldMaxSpeed, float newMaxSpeed)
    {
        SetMaxSpeed(newMaxSpeed);
    }

    protected void SetVelocity(Vector3 velocity)
    {
        if (characterMovement != null)
            characterMovement.velocity = velocity;
    }

    private IEnumerator SetCanMoveAfter(bool canMove, float durationInSecond)
    {
        yield return new WaitForSeconds(durationInSecond);
        SetCanMove(canMove);
    }
}
