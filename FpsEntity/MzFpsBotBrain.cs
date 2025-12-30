using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Micosmo.SensorToolkit;

public class MzFpsBotBrain : MzBotBrainBase
{
    protected LOSSensor losSensor;
    protected UserSignals userSignals;
    protected List<FpsModel> detectedModels = new List<FpsModel>();
    protected List<Transform> detectedHitboxes = new List<Transform>();
    private Dictionary<int , Signal> dictCharIdToSignal = new Dictionary<int , Signal>();

    private FpsHumanoidCharacter fpsCharacter;
    private EasyCharacterMovement.Character aiCharacterController;

    public BotStateEnum botState = BotStateEnum.Wandering;
    private readonly Dictionary<BotStateEnum, AbstractBotStateProcessor> dictStateToProcessor = new Dictionary<BotStateEnum, AbstractBotStateProcessor>();
    [SerializeField] private readonly BotFsmDto botFsmDto = new BotFsmDto();
    private AbstractBotStateProcessor activeProcessor;

    // How long bot stays "Aiming" enemy before shooting
    public float reactionTime;
    // The chance bot will chase enemy ( go to last seen position )
    public float chaseChance = 1.0f;

    protected override void Start()
    {
        base.Start();

        if (character.isServer)
        {
            fpsCharacter = character as FpsHumanoidCharacter;
            aiCharacterController = GetComponent<EasyCharacterMovement.Character>();
            losSensor = GetComponentInChildren<LOSSensor>();
            userSignals = GetComponentInChildren<UserSignals>();

            dictStateToProcessor.Add(BotStateEnum.Wandering, new BotWanderStateProcessor(this, fpsCharacter, botFsmDto));
            dictStateToProcessor.Add(BotStateEnum.Engage, new BotEngageStateProcessor(this, fpsCharacter, botFsmDto));
            dictStateToProcessor.Add(BotStateEnum.Chasing, new BotChaseStateProcessor(this, fpsCharacter, botFsmDto));
            dictStateToProcessor.Add(BotStateEnum.ReactToUnknownDamage, new BotReactToUnknownDamageStateProcessor(this, fpsCharacter, botFsmDto));
            TransitToState(BotStateEnum.Wandering);

            // Parent no need rotation , it is handled in FpsModel with weaponAimAt 
            aiCharacterController.SetRotationMode(EasyCharacterMovement.RotationMode.None);
            fpsCharacter.IsLookAtWeaponAim = true;


            SharedContext.Instance.characterJoinEvent.AddListener(character => SyncSignalsWithContext());
            SharedContext.Instance.characterRemoveEvent.AddListener(character => SyncSignalsWithContext());

            SyncSignalsWithContext();

            fpsCharacter.onKilledEvent.AddListener(OnKilled);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!character.isServer || character.IsDead() || !aiEnabled) return;

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
        if (level <= 0)
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
        /*
        if (GetActiveWeapon().currentClip <= 0)
        {
            ReloadActiveWeapon();
            RpcReloadActiveWeapon();
        }
        */
    }

    public void OnKilled(GameObject gameObject)
    {
        BotBrainHive.Instance.RegisterNodeKilled(character.team, character.transform.position);
    }

    public void OnTeammateKilled(Vector3 deathPos, DamageInfo damageInfo)
    {
        activeProcessor.OnTeammateKilled(deathPos, damageInfo);
    }

    public bool IsReachedDesination()
    {
        // Update the destination of the AI if
        // the AI is not already calculating a path and
        // the ai has reached the end of the path or it has no path at all
        return (!ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath));
    }

    public FpsModel ScanForShootTarget()
    {
        if (losSensor == null || aiIgnoreEnemy)
            return null;

        detectedModels.Clear();

        // Detect "FpsModel" attached in the ModelRoot , because it contains the LOS Target.
        losSensor.Pulse();
        losSensor.GetDetectedComponents<FpsModel>(detectedModels);

        if (detectedModels == null || detectedModels.Count == 0)
            return null;


        foreach (FpsModel detectedModel in detectedModels)
        {
            if (!(detectedModel.fpsCharacter is FpsCharacter))
                continue;

            FpsCharacter detectedCharacter = (FpsCharacter)detectedModel.fpsCharacter;
            if (!detectedModel.fpsCharacter.IsDead() && !(detectedCharacter.team == character.team))
            {
                return detectedModel;
            }
        }
        
        return null;
    }

    public Transform GetVisibleHitBoxFromAimTarget(GameObject targetObject)
    {
        // Debug.Log(targetObject);
        detectedHitboxes.Clear();
        // Let's just assume all aim target is FpsCharacter first.
        losSensor.Pulse();
        ILOSResult result = losSensor.GetResult(targetObject);
        
        if (result == null || !result.IsVisible) return null;

        foreach (LOSRayResult rayResult in result.Rays)
        {
            if (!rayResult.IsObstructed)
            {
                detectedHitboxes.Add(rayResult.TargetTransform);
            }
        }

        if (detectedHitboxes != null && detectedHitboxes.Count > 0)
        {
            return Utils.GetRandomElement<Transform>(detectedHitboxes);
        }
        
        return null;
    }

    protected override void OnCharacterSpawn()
    {
        base.OnCharacterSpawn();

        // Disable to prevent spawning at death position
        TogglePathingFindingAI(false);
        TogglePathingFindingAI(true);
    }

    public void OnTakeHit(DamageInfo damageInfo)
    {
        if (damageInfo.damageSourcePosition == Vector3.zero) return;
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

    private void AddCharacterToSignal(FpsCharacter fpsCharacter)
    {
        Signal signal = AsSignal(fpsCharacter);
        userSignals.InputSignals.Add(signal);
        dictCharIdToSignal.Add(fpsCharacter.GetInstanceID(), signal);
    }

    public void SyncSignalsWithContext()
    {
        List<int> remainingKeys = new List<int>(dictCharIdToSignal.Keys);
        foreach (FpsCharacter fpsCharacter in SharedContext.Instance.characterList)
        {
            remainingKeys.Remove(fpsCharacter.GetInstanceID());
            if (dictCharIdToSignal.ContainsKey(fpsCharacter.GetInstanceID()) 
                || fpsCharacter.GetInstanceID() == this.fpsCharacter.GetInstanceID())
                continue;

            AddCharacterToSignal(fpsCharacter);
        }

        foreach (int remaining in remainingKeys)
        {
            if (dictCharIdToSignal.ContainsKey(remaining))
            {
                Signal signal = dictCharIdToSignal[remaining];
                userSignals.InputSignals.Remove(signal);
            }
            dictCharIdToSignal.Remove(remaining);
        }
    }

    private static Signal AsSignal(FpsCharacter fpsCharacter)
    {
        return new Signal()
        {
            Object = fpsCharacter.FpsModel.gameObject,
            Strength = 1.0f,
            Shape = new Bounds()
            {
                extents = new Vector3(0.2f, 0.75f, 0.2f),
                center = new Vector3(0, 0.75f, 0)
            }
        };
    }
}
