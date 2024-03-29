﻿using System;
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

    private static readonly string ADDRESS_KEY_AUDIO_HUMAN_WIN = "annoce_win_human";
    private static readonly string ADDRESS_KEY_AUDIO_ZOMBIE_WIN = "annoce_win_zombie";

    public int StageTargetKillCount
    {
        get { return config.stageTargetKillCount; }
        set { config.stageTargetKillCount = value; }
    }

    public float HealthMultiplierPerPlayer
    {
        get { return config.healthMultiplierPerPlayer; }
        set { config.healthMultiplierPerPlayer = value; }
    }

    public float HealthMultiplierPerStage
    {
        get { return config.healthMultiplierPerStage; }
        set { config.healthMultiplierPerStage = value; }
    }

    public int StageRestTime
    {
        get { return config.restTime; }
        set { config.restTime = value; }
    }

    public int MaxStage
    {
        get { return config.maxStage; }
        set { config.maxStage = value; }
    }

    public int MaxMonsters
    {
        get { return config.maxMonsters; }
        set { config.maxMonsters = value; }
    }

    private MonsterModeConfig config = new MonsterModeConfig();

    [SyncVar] private GameState currentGameState = GameState.NotStarted;

    private List<Transform> monsterSpawnPoints = new List<Transform>();
    
    [SerializeField] public ActionCooldown spawnInterval;
    
    [SyncVar(hook = (nameof(OnKillUpdate)))] 
    public int stageKillCount = 0;

    // Stage must start at "1" ! 
    [SyncVar(hook = (nameof(OnStageUpdate)))] 
    public int currentStage = 0;

    [SyncVar(hook = (nameof(OnRestRemainUpdate)))] public int restRemain = 10;

    private int activePlayers = 1;

    private int currentMonsterCount = 0;

    public Stack<GameObject> stageSpawnStack;

    private List<E_monster_info> stageSpawnableList = new List<E_monster_info>();
    private Dictionary<string , int> dictMonsterSpawnCount = new Dictionary<string , int>();
    

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StreamingAssetManager.Instance.InitializeMonsterDict();
        StreamingAssetManager.Instance.InitializeEffectDict();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        FindSpawnsOnMap();

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
        List<FpsPlayer> fpsPlayers = SharedContext.Instance.GetPlayers();
        fpsPlayers.ForEach(player => { 
            player.TargetUpdateAmmoPack(player.connectionToClient, 5);
            player.Respawn();
        });
        
        activePlayers = fpsPlayers.Count;

        // restTime = 15;
        currentStage = 0;
        dictMonsterSpawnCount.Clear();
        GivePistolOnStart();
        RestStart();
        RpcStartGame();
    }

    [ClientRpc]
    private void RpcStartGame()
    {
        MonsterModeInfoDto monsterModeInfoDto = new MonsterModeInfoDto();
        LocalPlayerContext.Instance.player.AdditionalInfoObjects[Constants.ADDITIONAL_KEY_MM_CHAR_INFO] = monsterModeInfoDto;
    }

    private void GivePistolOnStart()
    {
        List<E_weapon_info> pistolList = E_weapon_info.FindEntities(e => e.f_monster_level == 0 && e.f_category == WeaponCategory.Pistol);
        pistolList.Shuffle();
        SharedContext.Instance.GetPlayers().ForEach(p => p.ServerCmdGetWeapon(pistolList[0].f_name, 1));
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
        restRemain = config.restTime;
        StartCoroutine(DoRestTimeCountdown());
        RpcRestStart(currentStage);
    }

    [ClientRpc]
    private void RpcRestStart(int stage)
    {
        MonsterModeUiManager.Instance.UpdateStage(stage , config.maxStage);
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
        stageKillCount = 0;
        stageSpawnableList = BuildStageSpawnableList(stage);
    }

    public void EndStage()
    {
        if (currentStage == config.maxStage)
            EndGame(true);
        else
            RestStart();
    }

    private void EndGame(bool isHumanWin) 
    {
        currentGameState = GameState.NotStarted;
        MzCharacterManager.Instance.KillAllCharacterInTeam(TeamEnum.Monster);
        RpcEndGame(isHumanWin);
    }

    [ClientRpc]
    private void RpcEndGame(bool isHumanWin) 
    {
        LocalPlayerContext.Instance.PlayAnnouncementAddressable(isHumanWin ? ADDRESS_KEY_AUDIO_HUMAN_WIN : ADDRESS_KEY_AUDIO_ZOMBIE_WIN);
    }

    public void Update()
    {
        if (!isServer) return;
        TrySpawnEnemy();
    }

    private void TrySpawnEnemy()
    {
        if ( (currentGameState != GameState.Battle && currentGameState != GameState.Midnight) 
             || currentMonsterCount >= config.maxMonsters) return;

        if (spawnInterval.CanExecuteAfterDeltaTime(true)) 
        {
            currentMonsterCount++;
            // ToDo: replace with shuffle
            E_monster_info monsterInfo = Utils.GetRandomElement<E_monster_info>(stageSpawnableList);
            while(!CanSpawnEnemy(monsterInfo.f_name , monsterInfo.f_max_count))
                monsterInfo = Utils.GetRandomElement<E_monster_info>(stageSpawnableList);

            GameObject prefab = StreamingAssetManager.Instance.GetMonsterPrefab(monsterInfo.f_name);


            Vector3 posToSapwn = Utils.GetRandomElement<Transform>(monsterSpawnPoints).position;
            GameObject obj = Instantiate(prefab, posToSapwn, Quaternion.identity);

            MzRpgCharacter mzRpgCharacter = obj.GetComponent<MzRpgCharacter>();
            mzRpgCharacter.SetMaxSpeed(monsterInfo.f_move_speed);
            mzRpgCharacter.MeleeDamage = monsterInfo.f_melee_damage;

            NetworkServer.Spawn(obj);
            MzCharacterManager.Instance.AddNewCharacter(mzRpgCharacter);

            float healthMultiplier = GetEnemyHealthMultiplier();
            mzRpgCharacter.SetHealth((int)(monsterInfo.f_base_health * healthMultiplier));
            mzRpgCharacter.AdditionalInfos.Add(Constants.ADDITIONAL_INFO_MONSTER_ID, monsterInfo.f_name);

            if (dictMonsterSpawnCount.ContainsKey(monsterInfo.f_name))
                dictMonsterSpawnCount[monsterInfo.f_name]++;
            else
                dictMonsterSpawnCount.Add(monsterInfo.f_name, 1);

            AssignAbility(mzRpgCharacter, monsterInfo.f_ability_key);
        }
    }

    private float GetEnemyHealthMultiplier()
    {
        float stageHealthMultiplier = Math.Max(config.healthMultiplierPerStage * (currentStage - 1), 1);

        float playerHealthMultiplier = Mathf.Pow(config.healthMultiplierPerPlayer, activePlayers);

        return stageHealthMultiplier * playerHealthMultiplier;
    }

    private bool CanSpawnEnemy(string monsterId , int maxCount)
    {
        dictMonsterSpawnCount.TryGetValue(monsterId, out int currentCount);
        return currentCount < maxCount;
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
        {
            bool isAllDead = true;
            foreach (FpsPlayer player in ServerContext.Instance.playerList)
            {
                if (!player.IsDead())
                {
                    isAllDead = false;
                    break;
                }
            }

            if(isAllDead)
                EndGame(false);

            PlayerManager.Instance.QueueRespawn(fpsCharacter, 10);
        }
            
    }

    private void OnMonsterKilled(FpsCharacter fpsCharacter)
    {
        if (fpsCharacter.AdditionalInfos.ContainsKey(Constants.ADDITIONAL_INFO_MONSTER_ID))
        {
            string monsterId = fpsCharacter.AdditionalInfos[Constants.ADDITIONAL_INFO_MONSTER_ID];
            dictMonsterSpawnCount[monsterId]--;
        }
        
        MzCharacterManager.Instance.QueueRemove(fpsCharacter, 5);

        if (currentGameState == GameState.Battle || currentGameState == GameState.Midnight)
        {
            RollAmmoPack();
            currentMonsterCount--;
            stageKillCount++;

            if (stageKillCount >= config.stageTargetKillCount)
            {
                EndStage();
            }
        }
    }

    private void RollAmmoPack()
    {
        if (UnityEngine.Random.Range(0, 100) < 30)
        {
            SharedContext.Instance.GetPlayers().ForEach(player => player.TargetUpdateAmmoPack(player.connectionToClient , 1));
        }
    }

    private void FindSpawnsOnMap()
    {
        monsterSpawnPoints = new List<Transform>();
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag(Constants.TAG_MONSTER_SPAWN);

        foreach (GameObject obj in spawnObjects)
            monsterSpawnPoints.Add(obj.transform);
    }

    public void AssignAbility(FpsCharacter fpsCharacter, string abilityKey) 
    {
        if (string.IsNullOrEmpty(abilityKey))
            return;

        Ability ability = null;
        if (string.Equals(AbilityBerserk.ID, abilityKey, StringComparison.OrdinalIgnoreCase))
            ability = new AbilityBerserk(fpsCharacter);
        else if (string.Equals(AbilityDeathExplosion.ID, abilityKey, StringComparison.OrdinalIgnoreCase))
            ability = new AbilityDeathExplosion(fpsCharacter);
        else if (string.Equals(AbilityStalk.ID, abilityKey, StringComparison.OrdinalIgnoreCase))
            ability = new AbilityStalk(fpsCharacter);
        else if (string.Equals(AbilityRadiation.ID, abilityKey, StringComparison.OrdinalIgnoreCase))
            ability = new AbilityRadiation(fpsCharacter);
        else if (string.Equals(AbilityMetatronicSkill.ID, abilityKey, StringComparison.OrdinalIgnoreCase))
            ability = new AbilityMetatronicSkill(fpsCharacter);

        if (ability != null) { 
            MzAbilitySystem abilitySystem = fpsCharacter.GetComponent<MzAbilitySystem>();
            abilitySystem.AddAbility(ability);
        }
    }

    

    private void OnStageUpdate(int oldVal , int newVal)
    {
        // MonsterModeUiManager.Instance.UpdateStage(newVal);
    }

    private void OnKillUpdate(int oldVal , int newVal)
    {
        MonsterModeUiManager.Instance.UpdateKillText(newVal , config.stageTargetKillCount);
    }

    private void OnRestRemainUpdate(int oldVal , int newVal)
    {
        MonsterModeUiManager.Instance.UpdateRestCountdown(newVal);
    }

    public int GetBackAmmoForWeaponType(WeaponCategory category , bool isSemiAuto)
    {
        switch (category) 
        {
            case WeaponCategory.Rifle:
            case WeaponCategory.Pistol:
                return 30;
            case WeaponCategory.Smg:
            case WeaponCategory.Mg:
                return 50;
            case WeaponCategory.Shotgun:
                return 10;
            case WeaponCategory.Sniper:
                if (isSemiAuto)
                    return 5;
                else
                    return 20;
        }
        return 0;
    }

    private enum GameState 
    { 
        NotStarted,
        Rest,
        Battle,
        Midnight
    }
}