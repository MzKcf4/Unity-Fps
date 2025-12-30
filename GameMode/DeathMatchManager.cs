using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class DeathMatchManager : NetworkBehaviour
{
    public static DeathMatchManager Instance;
    [SyncVar(hook = (nameof(OnBlueTeamKillsUpdate)))]
    public int currentScoreBlue;

    [SyncVar(hook = (nameof(OnRedTeamKillsUpdate)))]
    public int currentScoreRed;

    [SyncVar] public int targetScore;
    [SyncVar] bool isMatchActive = true;

    private List<E_weapon_info> nonMeleeWeapons;

    [SerializeField] private AudioClip roundEndClip;
    [SerializeField] private GameObject uiPrefab;
    [SerializeField] private GameObject weaponSelectionUiPrefab;

    void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerContext.Instance.characterKilledEventServer.AddListener(OnCharacterKilled);
        SharedContext.Instance.characterSpawnEvent.AddListener(OnCharacterSpawn);

        nonMeleeWeapons = E_weapon_info.FindEntities(e => e.f_active && e.f_category != WeaponCategory.Melee);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (DeathMatchUiManager.Instance == null)
            Instantiate(uiPrefab, FpsUiManager.Instance.GetInfoPanel());

        Instantiate(weaponSelectionUiPrefab, FpsUiManager.Instance.GetInfoPanel());

    }

    private void OnCharacterSpawn(FpsCharacter fpsCharacter)
    {
        if (!isServer || !isMatchActive || !(fpsCharacter is FpsHumanoidCharacter)) 
            return;

        if(fpsCharacter.IsBot())
        {
            AssignRandomWeapon((FpsHumanoidCharacter)fpsCharacter);
        }

        /*
        if (weapon.f_category == WeaponCategory.Pistol)
            ((FpsHumanoidCharacter)fpsCharacter).ServerCmdGetWeapon(weapon.f_name, 1);
        else
            ((FpsHumanoidCharacter)fpsCharacter).ServerCmdGetWeapon(weapon.f_name, 0);
        */
    }

    private void AssignWeaponsToPlayer()
    {
        List<FpsCharacter> fpsCharacters = SharedContext.Instance.characterList;

        foreach (FpsCharacter fpsCharacter in fpsCharacters)
        {
            if (fpsCharacter.IsBot())
            {
                AssignRandomWeapon((FpsHumanoidCharacter)fpsCharacter);
            }
        }
    }

    private void AssignRandomWeapon(FpsHumanoidCharacter humanoidCharacter)
    {
        int idx = Random.Range(0, nonMeleeWeapons.Count);
        var weapon = nonMeleeWeapons[idx];
        if (weapon.f_category == WeaponCategory.Pistol)
            humanoidCharacter.ServerCmdGetWeapon(weapon.f_name, 1);
        else
            humanoidCharacter.ServerCmdGetWeapon(weapon.f_name, 0);
    }

    [Server]
    public void RoundStart()
    {
        currentScoreBlue = 0;
        currentScoreRed = 0;
        targetScore = 1000;
        isMatchActive = true;
        RpcUpdateScore();

        RestorePlayerHealth();
        TeleportPlayersToSpawn();
        AssignWeaponsToPlayer();
    }


            
    [Server]
    public void OnCharacterKilled(FpsCharacter victim, DamageInfo dmgInfo)
    {
        PlayerManager.Instance.QueueRespawn(victim);

        if (!isServer || !isMatchActive || string.IsNullOrEmpty(dmgInfo.damageWeaponName))  
            return;
            
        int killScore = GetWeaponKillScore(dmgInfo.damageWeaponName);
        
        if(victim.team == TeamEnum.Blue)
            currentScoreRed += killScore;
        else if (victim.team == TeamEnum.Red)
            currentScoreBlue += killScore;
        
        CheckWinCondition();
    }
    
    private void CheckWinCondition(){
        if (currentScoreBlue >= targetScore || currentScoreRed >= targetScore)
        {
            isMatchActive = false;
            RpcRoundEnd(currentScoreBlue >= targetScore);
        }   
    }

    [ClientRpc]
    private void RpcRoundEnd(bool isBlueWin)
    {
        if(!isLocalPlayer || roundEndClip == null)
            return;

        if (LocalPlayerContext.Instance.localPlayerAudioSource)
            LocalPlayerContext.Instance.localPlayerAudioSource.PlayOneShot(roundEndClip);

        DeathMatchUiManager.Instance.SetWin(isBlueWin ? TeamEnum.Blue : TeamEnum.Red);        
    }
    

    private int GetWeaponKillScore(string weaponName)
    {
        E_weapon_info dbWeaponInfo = E_weapon_info.GetEntity(weaponName);
        return dbWeaponInfo.f_dm_kill_score;
    }
        
    private void OnBlueTeamKillsUpdate(int oldValue, int newValue)
    {
        DeathMatchUiManager.Instance.UpdateScores(newValue, currentScoreRed);
    }

    private void OnRedTeamKillsUpdate(int oldValue, int newValue)
    {
        DeathMatchUiManager.Instance.UpdateScores(currentScoreBlue, newValue);
    }

    [ClientRpc]
    private void RpcUpdateScore()
    {
        DeathMatchUiManager.Instance.UpdateScores(currentScoreBlue, currentScoreRed);
        DeathMatchUiManager.Instance.UpdateScores(currentScoreBlue, currentScoreRed);
        DeathMatchUiManager.Instance.SetTargetScore(targetScore);
    }

    [Server]
    public void SetTargetScore(int score)
    {
        targetScore = score;
        RpcUpdateScore();
    }

    private void TeleportPlayersToSpawn()
    {
        List<FpsCharacter> fpsCharacters = SharedContext.Instance.characterList;
        foreach (FpsCharacter fpsCharacter in fpsCharacters)
        {
            if (!fpsCharacter || !fpsCharacter.isServer) continue;

            fpsCharacter.onSpawnEvent.Invoke();
            PlayerManager.Instance.TeleportCharacterToSpawnPoint(fpsCharacter);
        }
    }

    private void RestorePlayerHealth()
    {
        List<FpsCharacter> fpsCharacters = SharedContext.Instance.characterList;
        foreach (FpsCharacter fpsCharacter in fpsCharacters)
        {
            if (!fpsCharacter || !fpsCharacter.isServer) continue;
            fpsCharacter.health = fpsCharacter.maxHealth;
        }
    }
}
