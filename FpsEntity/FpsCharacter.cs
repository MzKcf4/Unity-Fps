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
    protected static float WALKING_SPEED = 2.5f;
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

    [SyncVar]
    protected bool isWalking = false;


    // ToDo: Remove this ! Use MzCharacterAnimator instead
    [SerializeField] protected CharacterAnimationResource charAnimationRes;


    [SerializeField] protected CharacterResources characterResources;
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

    protected bool isBot = false;

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
        audioSourceWeapon.minDistance = 8;
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
        isBot = GetComponent<MzBotBrainBase>() != null;

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

    [Command]
    public void CmdPlayAnimationByKey(string key, bool isFullBody, bool isForceExecute)
    {
        RpcPlayAnimationByKey(key, isFullBody, isForceExecute);
    }

    [ClientRpc]
    protected void RpcPlayAnimationByKey(string key, bool isFullBody, bool isForceExecute)
    {
        characterAnimator.PlayAnimationByKey(key, isFullBody, isForceExecute);

        // PlayAnimationByKey(key, isFullBody , isForceExecute);
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
        if (damageInfo.attacker is FpsPlayer && damageInfo.wallsPenetrated <= 0)
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

        if (characterResources != null && characterResources.hurtVoiceList != null && characterResources.hurtVoiceList.Count > 0)
        {
            AudioClip clip = Utils.GetRandomElement<AudioClip>(characterResources.hurtVoiceList);
            audioSourceCharacter.PlayOneShot(clip);
        }

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
        ApplyForceOnRagdoll(damageInfo);
        FpsUiManager.Instance.AddNewKillListing(damageInfo , characterName);

        if (useDeathAnimation && charAnimationRes.deathClips.Count > 0)
        {
            characterAnimator.PlayDeathAnimation(Utils.GetRandomElement<ClipTransition>(charAnimationRes.deathClips));
        }

        if (characterResources != null && characterResources.deathVoiceList != null && characterResources.deathVoiceList.Count > 0)
        {
            AudioClip clip = Utils.GetRandomElement<AudioClip>(characterResources.deathVoiceList);
            audioSourceCharacter.PlayOneShot(clip);
        }
        else
        {
            AudioClip clip = Utils.GetRandomElement<AudioClip>(characterCommonResources.deathSoundList);
            audioSourceCharacter.PlayOneShot(clip);
        }
    }
    
    protected void SetControllableState(bool controllable)
    {
        SetRagdollState(!controllable);
        SetComponentsControllable(controllable);
    }

    protected void ApplyForceOnRagdoll(DamageInfo damageInfo) 
    {
        var direction = Utils.GetDirection(damageInfo.damageSourcePosition, damageInfo.hitPoint);
        var force = direction * damageInfo.damage * 10f;
        Rigidbody[] rbJoints = modelObject.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in rbJoints)
        {
            rb.AddForce(force);
        }
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
        // Currently used by BOT only
        if (isServer)
            currentVelocity = GetMovementVelocity();
    }


    [Server]
    public void ServerSetVelocity(Vector3 velocity)
    {
        currentVelocity = velocity;
    }

    public virtual Vector3 GetMovementVelocity() 
    {
        // return currentVelocity;
        return characterMovement.velocity;
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
        audioSource.spatialBlend = 1.0f;
        audioSource.spread = 1f;
        audioSource.minDistance = 2;
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

    public bool IsBot()
    {
        return isBot;
    }
}
