using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class ProgressionManager : NetworkBehaviour
{
	public static ProgressionManager Instance;
	public UnityEvent onProgressUpdateEvent = new UnityEvent();
	
	public List<StageConfig> stageConfigs = new List<StageConfig>();
	private StageConfig activeStageConfig;
	
	public List<Transform> spawnPoints = new List<Transform>();
	private List<GameObject> spawnedEnemy = new List<GameObject>();
	
	[SerializeField]
	private ProgressionWeaponConfig progressionWeaponConfig;
	
	[SerializeField]
	public ActionCooldown spawnInterval;
	[SyncVar]
	public int currentStage = 0;
	public int maxEnemyCount = 10;
	
	
	public bool canSpawn = false;
	[SyncVar] public int stageCurrentKills = 0;
	[SyncVar] public int stageTargetKills = 10;
	
	[SerializeField]
	public ActionCooldown stageTime;
	[SerializeField]
	public ActionCooldown restTime;
	
	[SyncVar]
	public int stageTimeDisplay;
	[SyncVar]
	public int restTimeDisplay;
	
	public float enragedSpeedMultiply = 2f;
	

	public ActionCooldown regularInvokeInterval = new ActionCooldown(){
		interval = 1f
		};
		
	[SyncVar]
	public ProgressionState progressionState = ProgressionState.NotStarted;
	
	void Awake()
	{
		Instance = this;
	}
	
    void Start()
    {
	    progressionWeaponConfig.InitializeWeaponList();
    }

    void Update()
	{
		if(!isServer)	return;
		UpdateProgression();

	}
	
	private void UpdateProgression()
	{
		if(progressionState == ProgressionState.NotStarted)	return;
		
		regularInvokeInterval.ReduceCooldown(Time.deltaTime);
		if(!regularInvokeInterval.IsOnCooldown())
		{
			stageTimeDisplay = GetRemainingStageTime();
			restTimeDisplay = GetRemainingRestTime();
			RpcNotifyProgressUpdate();
			regularInvokeInterval.StartCooldown();
		}
		
		if(progressionState == ProgressionState.Rest)
		{
			restTime.ReduceCooldown(Time.deltaTime);
			if(!restTime.IsOnCooldown())
			{
				StartStage(currentStage + 1);
			}
		}
		else if (progressionState == ProgressionState.Started)
		{
			stageTime.ReduceCooldown(Time.deltaTime);
			if(!stageTime.IsOnCooldown())
			{
				if(CanEndStage())
					EndStage();
				else
					StartEnrage();
			}
		}
		
		if(progressionState == ProgressionState.Started || progressionState == ProgressionState.Enraged)
		{
			if(spawnedEnemy.Count < maxEnemyCount)
			{
				spawnInterval.ReduceCooldown(Time.deltaTime);
				if(!spawnInterval.IsOnCooldown())
				{
					SpawnEnemy();
					spawnInterval.StartCooldown();
				}
			}
		}
	}
    
	public void Restart()
	{
		
	}
	
	public void Stop()
	{
		
	}
    
	public void StartStage(int i)
	{
		spawnedEnemy.Clear();
		stageCurrentKills = 0;
		activeStageConfig = stageConfigs[i];
		currentStage = i;
		maxEnemyCount = activeStageConfig.maxSpawnCount;
		canSpawn = true;
		progressionState = ProgressionState.Started;
		stageTime.StartCooldown();
		
		RpcNotifyProgressUpdate();
		
		
		List<GameObject> weaponPrefabList = progressionWeaponConfig.dictTierToWeaponPrefab[currentStage];
		GameObject weaponPrefab = Utils.GetRandomElement<GameObject>(weaponPrefabList);
		// FpsWeapon fpsWeapon = weaponPrefab.GetComponent<FpsWeapon>();		
		foreach(FpsPlayer player in ServerContext.Instance.playerList)
		{
			// player.RpcGetWeapon(fpsWeapon.weaponName,  0);
		}
	}
	
	public void StartEnrage()
	{
		progressionState = ProgressionState.Enraged;
		foreach(GameObject enemyObj in spawnedEnemy)
		{
			FpsEnemy fpsEnemy = enemyObj.GetComponent<FpsEnemy>();
			if(fpsEnemy != null)
			{
				fpsEnemy.MultiplySpeed(enragedSpeedMultiply);
			}
		}
	}
	
	public void EndStage()
	{
		if(currentStage + 1 == stageConfigs.Count)
		{
			progressionState = ProgressionState.Ended;
			KillAllSpawnedEnemy();
			RpcNotifyProgressUpdate();
			return;
		}
		else
		{	
			progressionState = ProgressionState.Rest;
			canSpawn = false;
			restTime.StartCooldown();
			KillAllSpawnedEnemy();
			RpcNotifyProgressUpdate();
		}

	}
	
	private void SpawnEnemy()
	{
		GameObject prefab = Utils.GetRandomElement<GameObject>(activeStageConfig.spawnPrefabList);
		Vector3 posToSapwn = Utils.GetRandomElement<Transform>(spawnPoints).position;
		
		GameObject obj = Instantiate(prefab, posToSapwn , Quaternion.identity);
		spawnedEnemy.Add(obj);
		
		FpsEnemy fpsEnemy = obj.GetComponent<FpsEnemy>();
		fpsEnemy.onKilledEvent.AddListener(OnEnemyKilled);
		
		if(progressionState == ProgressionState.Enraged)
			fpsEnemy.MultiplySpeed(enragedSpeedMultiply);
			
		NetworkServer.Spawn(obj);
	}
	
	private void OnEnemyKilled(GameObject enemyObj)
	{
		// When ending stage , we manually kill all enemies , but they will invoke this too , 
		//		so add this checking to prevent concurrent modification
		if(progressionState != ProgressionState.Rest)
		{
			spawnedEnemy.Remove(enemyObj);
			stageCurrentKills++;
		}
			

		if(progressionState == ProgressionState.Enraged && CanEndStage())
		{
			EndStage();
		}
		
		RpcNotifyProgressUpdate();
	}
	
	private bool CanEndStage()
	{
		return stageCurrentKills >= stageTargetKills;
	}
	
	private bool CanSpawnEnemy()
	{
		return spawnedEnemy.Count < maxEnemyCount;
	}
	
	[ClientRpc]
	private void RpcNotifyProgressUpdate()
	{
		onProgressUpdateEvent.Invoke();
	}
	public int GetRemainingStageTime()
	{
		return Mathf.FloorToInt(stageTime.nextUse);
	}
	
	public int GetRemainingRestTime()
	{
		return Mathf.FloorToInt(restTime.nextUse);
	}
	
	private void KillAllSpawnedEnemy()
	{
		foreach(GameObject enemyObj in spawnedEnemy)
		{
			FpsEnemy fpsEnemy = enemyObj.GetComponent<FpsEnemy>();
			if(fpsEnemy != null)
			{
				fpsEnemy.Kill();
			}
		}
	}
	
}
