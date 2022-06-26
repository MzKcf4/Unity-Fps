using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class MonsterModeManager : NetworkBehaviour
{
    public static MonsterModeManager Instance;
    [SerializeField] private GameObject uiPrefab;

    private List<Transform> monsterSpawnPoints = new List<Transform>();
	private List<GameObject> spawnedEnemy = new List<GameObject>();
    [SerializeField] private List<MonsterStageConfig> stageConfigList = new List<MonsterStageConfig>();
    private MonsterStageConfig activeStageConfig;

    public bool canSpawn = false;
    
    [SerializeField] public ActionCooldown spawnInterval;
    
    [SyncVar(hook = (nameof(OnKillRemainUpdate)))] public int stageKillsRemain = 0;
    // Stage must start at "1" ! 
    [SyncVar(hook = (nameof(OnStageUpdate)))] 
    public int currentStage = 0;

    public int restTime = 10;
    [SyncVar(hook = (nameof(OnRestRemainUpdate)))] public int restRemain = 10;
    public int maxMonsters = 30;
    public Stack<GameObject> stageSpawnStack;

    private List<GameObject> killedMonsterList = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindSpawnsOnMap();
        ServerContext.Instance.characterKilledEventServer.AddListener(OnCharacterKilled);
        // SharedContext.Instance.characterSpawnEvent.AddListener(OnCharacterSpawn);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (MonsterModeUiManager.Instance == null)
            Instantiate(uiPrefab, FpsUiManager.Instance.GetInfoPanel());
    }

    public void StartGame()
    {
        currentStage = 0;
        RestStart();
    }

    public void StopGame()
    {
        canSpawn = false;
    }

    public void RestStart()
    {
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
        DestroyKilledMonsters();
        killedMonsterList.Clear();

        activeStageConfig = stageConfigList[stage];
        InitializeSpawnStack(stageConfigList[stage]);
        stageKillsRemain = activeStageConfig.targetKillCount;
        canSpawn = true;
    }

    public void EndStage()
    {
        canSpawn = false;
        RestStart();
    }

    public void Update()
    {
        if (!isServer) return;
        TrySpawnEnemy();
    }

    private void TrySpawnEnemy()
    {
        if (!canSpawn || spawnedEnemy.Count > maxMonsters) return;

        if (spawnInterval.CanExecuteAfterDeltaTime(true) && stageSpawnStack.Count > 0) 
        {
            GameObject prefab = stageSpawnStack.Pop();
            // GameObject prefab = Utils.GetRandomElement<GameObject>(normalMonsterPrefabList);
            Vector3 posToSapwn = Utils.GetRandomElement<Transform>(monsterSpawnPoints).position;

            GameObject obj = Instantiate(prefab, posToSapwn, Quaternion.identity);
            spawnedEnemy.Add(obj);

            FpsNpc fpsNpc = obj.GetComponent<FpsNpc>();
            fpsNpc.onKilledEvent.AddListener(OnMonsterKilled);

            /*
            if (progressionState == ProgressionState.Enraged)
                fpsEnemy.MultiplySpeed(enragedSpeedMultiply);
            */
            NetworkServer.Spawn(obj);
        }
    }

    [Server]
    public void OnCharacterKilled(FpsCharacter victim, DamageInfo dmgInfo)
    {
        if(victim is FpsPlayer)
            PlayerManager.Instance.QueueRespawn(victim);
    }

    private void OnMonsterKilled(GameObject enemyObj)
    {
        killedMonsterList.Add(enemyObj);

        stageKillsRemain--;

        if (stageKillsRemain <= 0)
        {
            EndStage();
        }


        /*
        // When ending stage , we manually kill all enemies , but they will invoke this too , 
        //		so add this checking to prevent concurrent modification
        if (progressionState != ProgressionState.Rest)
        {
            spawnedEnemy.Remove(enemyObj);
            stageCurrentKills++;
        }


        if (progressionState == ProgressionState.Enraged && CanEndStage())
        {
            EndStage();
        }

        RpcNotifyProgressUpdate();
        */
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
}