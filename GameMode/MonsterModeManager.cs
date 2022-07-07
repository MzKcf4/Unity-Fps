using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using BansheeGz.BGDatabase;

public class MonsterModeManager : NetworkBehaviour
{
    public static MonsterModeManager Instance;
    [SerializeField] private GameObject uiPrefab;

    [SyncVar] private GameState currentGameState = GameState.NotStarted;

    private List<Transform> monsterSpawnPoints = new List<Transform>();

    private MonsterStageConfig activeStageConfig;
    
    [SerializeField] public ActionCooldown spawnInterval;
    
    [SyncVar(hook = (nameof(OnKillRemainUpdate)))] 
    public int stageKillsRemain = 0;

    // Stage must start at "1" ! 
    [SyncVar(hook = (nameof(OnStageUpdate)))] 
    public int currentStage = 0;

    public int restTime = 10;
    [SyncVar(hook = (nameof(OnRestRemainUpdate)))] public int restRemain = 10;

    public int maxMonsters = 7;
    private int currentMonsterCount = 0;

    public Stack<GameObject> stageSpawnStack;

    private List<GameObject> killedMonsterList = new List<GameObject>();


    private List<E_monster_info> stageSpawnableList = new List<E_monster_info>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StreamingAssetManager.Instance.InitializeMonsterDict();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindSpawnsOnMap();
        maxMonsters = 7;
        MzCharacterManager.Instance.OnCharacterKilled.AddListener(OnCharacterKilled);
        // ServerContext.Instance.characterKilledEventServer.AddListener(OnCharacterKilled);
        // SharedContext.Instance.characterSpawnEvent.AddListener(OnCharacterSpawn);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (MonsterModeUiManager.Instance == null)
            Instantiate(uiPrefab, FpsUiManager.Instance.GetInfoPanel());
    }
    
    [Server]
    public void StartGame()
    {
        restTime = 15;
        currentStage = 0;
        GivePistolOnStart();
        RestStart();
    }

    private void GivePistolOnStart()
    {
        string pistolName = E_weapon_monster_info.FindEntity(e => e.f_level == 1 && e.f_weapon_info.f_category == WeaponCategory.Pistol)
                                                 .f_weapon_info.f_name;
        SharedContext.Instance.GetPlayers().ForEach(p => p.ServerCmdGetWeapon(pistolName, 1));
    }

    public void StopGame()
    {
        currentGameState = GameState.NotStarted;
    }

    public void RestStart()
    {
        currentGameState = GameState.Rest;
        MzCharacterManager.Instance.KillAllCharacterInTeam(TeamEnum.Monster);
        currentStage++;
        restRemain = restTime;
        StartCoroutine(DoRestTimeCountdown());
        RpcRestStart(currentStage);
    }

    [ClientRpc]
    private void RpcRestStart(int stage)
    {
        MonsterModeUiManager.Instance.UpdateStage(stage);
    }

    private IEnumerator DoRestTimeCountdown()
    {
        while (restRemain > 0)
        {
            yield return new WaitForSeconds(1);
            restRemain--;
        }
        EndRest();
    }

    public void EndRest()
    {
        StartStage(currentStage);
    }

    public void StartStage(int stage)
    {
        currentGameState = GameState.Battle;
        currentMonsterCount = 0;
        stageKillsRemain = 20;
        stageSpawnableList = BuildStageSpawnableList(stage);
    }

    public void EndStage()
    {
        RestStart();
    }

    public void Update()
    {
        if (!isServer) return;
        TrySpawnEnemy();
    }

    private void TrySpawnEnemy()
    {
        if ( (currentGameState != GameState.Battle && currentGameState != GameState.Midnight) 
             || currentMonsterCount >= maxMonsters) return;

        if (spawnInterval.CanExecuteAfterDeltaTime(true)) 
        {
            currentMonsterCount++;
            E_monster_info monsterInfo = Utils.GetRandomElement<E_monster_info>(stageSpawnableList);

            GameObject prefab = StreamingAssetManager.Instance.GetMonsterPrefab(monsterInfo.f_name);


            Vector3 posToSapwn = Utils.GetRandomElement<Transform>(monsterSpawnPoints).position;
            GameObject obj = Instantiate(prefab, posToSapwn, Quaternion.identity);

            MzRpgCharacter mzRpgCharacter = obj.GetComponent<MzRpgCharacter>();
            mzRpgCharacter.SetMaxSpeed(monsterInfo.f_move_speed);

            NetworkServer.Spawn(obj);
            MzCharacterManager.Instance.AddNewCharacter(mzRpgCharacter);

            float healthMultiplier = Math.Max(2 * (currentStage - 1), 1);
            mzRpgCharacter.SetHealth((int)(monsterInfo.f_base_health * healthMultiplier));
        }
    }

    private List<E_monster_info> BuildStageSpawnableList(int stage) 
    {
        return E_monster_info.FindEntities(e => stage >= e.f_start_stage && stage <= e.f_end_stage);
    }

    private void OnCharacterKilled(FpsCharacter fpsCharacter)
    {
        if (fpsCharacter.team == TeamEnum.Monster)
            OnMonsterKilled(fpsCharacter);
        else if (fpsCharacter is FpsPlayer)
            PlayerManager.Instance.QueueRespawn(fpsCharacter);
    }

    private void OnMonsterKilled(FpsCharacter fpsCharacter)
    {
        MzCharacterManager.Instance.QueueRemove(fpsCharacter, 5);

        if (currentGameState == GameState.Battle || currentGameState == GameState.Midnight)
        {
            currentMonsterCount--;
            stageKillsRemain--;

            if (stageKillsRemain <= 0)
            {
                EndStage();
            }
        }
    }

    private void DestroyKilledMonsters()
    {
        foreach (GameObject go in killedMonsterList)
            NetworkServer.Destroy(go);
    }

    private void InitializeSpawnStack(MonsterStageConfig stageConfig)
    {
        stageSpawnStack = new Stack<GameObject>();
        for (int i = 0; i < stageConfig.targetKillCount; i++)
        {
            stageSpawnStack.Push(stageConfig.spawnPrefabList[0]);
        }
    }

    private void FindSpawnsOnMap()
    {
        monsterSpawnPoints = new List<Transform>();
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag(Constants.TAG_MONSTER_SPAWN);

        foreach (GameObject obj in spawnObjects)
            monsterSpawnPoints.Add(obj.transform);
    }

    private void OnStageUpdate(int oldVal , int newVal)
    {
        MonsterModeUiManager.Instance.UpdateRestCountdown(newVal);
    }

    private void OnKillRemainUpdate(int oldVal , int newVal)
    {
        MonsterModeUiManager.Instance.UpdateKillText(newVal);
    }

    private void OnRestRemainUpdate(int oldVal , int newVal)
    {
        MonsterModeUiManager.Instance.UpdateRestCountdown(newVal);
    }

    private enum GameState 
    { 
        NotStarted,
        Rest,
        Battle,
        Midnight
    }
}