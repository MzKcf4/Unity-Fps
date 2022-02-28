using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

public class MonsterModeManager : NetworkBehaviour
{
    public static MonsterModeManager Instance;

	private List<Transform> monsterSpawnPoints = new List<Transform>();
	private List<GameObject> spawnedEnemy = new List<GameObject>();

    public bool canSpawn = false;
    private int maxMonsters = 15;

    [SerializeField] public ActionCooldown spawnInterval;

    [SyncVar] public int stageCurrentKills = 0;
    [SyncVar] public int stageTargetKills = 10;

    public List<GameObject> normalMonsterPrefabList;
    public List<GameObject> specialMonsterPrefabList;
    public List<GameObject> bossMonsterPrefabList;

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
        // ServerContext.Instance.characterKilledEventServer.AddListener(OnCharacterKilled);
        // SharedContext.Instance.characterSpawnEvent.AddListener(OnCharacterSpawn);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        /*
        if (GunGameUiManager.Instance == null)
            Instantiate(uiPrefab, FpsUiManager.Instance.GetInfoPanel());
        */
    }

    public void StartGame()
    {
        canSpawn = true;
    }

    public void StopGame()
    {
        canSpawn = false;
    }

    public void Update()
    {
        TrySpawnEnemy();
    }

    private void TrySpawnEnemy()
    {
        if (!canSpawn || spawnedEnemy.Count > maxMonsters) return;

        if (spawnInterval.CanExecuteAfterDeltaTime(true)) 
        {
            GameObject prefab = Utils.GetRandomElement<GameObject>(normalMonsterPrefabList);
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

    private void OnMonsterKilled(GameObject enemyObj)
    {
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

    private void FindSpawnsOnMap()
    {
        monsterSpawnPoints = new List<Transform>();
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag(Constants.TAG_MONSTER_SPAWN);

        foreach (GameObject obj in spawnObjects)
            monsterSpawnPoints.Add(obj.transform);
    }
}