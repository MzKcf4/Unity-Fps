using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ConsoleCmdManager : NetworkBehaviour
{
    [QFSW.QC.Command("bot_add_a")]
    public void AddBot_TeamA(int skillLevel)
    {
        if(!isServer)   return;
        
        Debug.Log(skillLevel);
        if(PlayerManager.Instance != null)
            PlayerManager.Instance.AddBot(TeamEnum.Blue, skillLevel);
    }
    
    [QFSW.QC.Command("bot_add_b")]
    public void AddBot_TeamB(int skillLevel)
    {
        if(!isServer)   return;
        
        if(PlayerManager.Instance != null)
            PlayerManager.Instance.AddBot(TeamEnum.Red, skillLevel);
    }
    
    [QFSW.QC.Command("bot_kick")]
    public void KickAllBot()
    {
        if(!isServer)   return;
        
        if(PlayerManager.Instance != null)
            PlayerManager.Instance.KickAllBot();
    }
    
    [QFSW.QC.Command("bot_ignore_enemy")]
    public void SetBotIgnoreEnemy(int ignore)
    {
        if(!isServer)   return;
        bool isIgnore = ignore == 0 ? false : true;
        
        foreach(FpsCharacter character in SharedContext.Instance.characterList)
        {
            if(!(character is FpsBot))
                continue;
            FpsBot fpsBot = (FpsBot) character;
            fpsBot.aiIgnoreEnemy = isIgnore;
        }
    }
    
    [QFSW.QC.Command("bot_enable_wander")]
    public void SetBotWander(int enable)
    {
        if(!isServer)   return;
        bool isEnable = enable == 0 ? false : true;
        
        foreach(FpsCharacter character in SharedContext.Instance.characterList)
        {
            if(!(character is FpsBot))
                continue;
            FpsBot fpsBot = (FpsBot) character;
            fpsBot.aiEnableWander = isEnable;
        }
    }
    
    [QFSW.QC.Command("bot_enable_godmode")]
    public void SetBotGodmode(int enable)
    {
        if(!isServer)   return;
        bool isEnable = enable == 0 ? false : true;
        
        foreach(FpsCharacter character in SharedContext.Instance.characterList)
        {
            if(!(character is FpsBot))
                continue;
            FpsBot fpsBot = (FpsBot) character;
            fpsBot.isGodMode = isEnable;
        }
    }

    [QFSW.QC.Command("sv_gamemode")]
    public void SetGameMode(GameModeEnum gameModeEnum)
    {
        if (!isServer) return;
        FpsNetworkRoomManager.Instance.SetGamemode(gameModeEnum);
        Debug.Log("Game mode will change to " + gameModeEnum.ToString());
    }
    
    [QFSW.QC.Command("dm_restart")]
    public void RestartDeathMatch()
    {
        if(!isServer)   return;
        DeathMatchManager.Instance.RestartMatch();
    }

    [QFSW.QC.Command("dm_score")]
    public void SetDeathMatchScore(int newScore)
    {
        if (!isServer) return;
        DeathMatchManager.Instance.UpdateTargetScore(newScore);
    }

    [QFSW.QC.Command("gg_restart")]
    public void RestartGunGame()
    {
        if (!isServer) return;
        GunGameManager.Instance.RoundStart();
    }

    [QFSW.QC.Command("gg_killsPerLevel_blue")]
    public void SetGunGameKillsPerLevel_Blue(int killsPerLevel)
    {
        if (!isServer) return;
        GunGameManager.Instance.killsPerLevelBlueTeam = killsPerLevel;
    }

    [QFSW.QC.Command("gg_killsPerLevel_red")]
    public void SetGunGameKillsPerLevel_Red(int killsPerLevel)
    {
        if (!isServer) return;
        GunGameManager.Instance.killsPerLevelRedTeam = killsPerLevel;
    }

    [QFSW.QC.Command("gg_weapon_count")]
    public void SetGunGameWeaponCount(int weaponCount)
    {
        if (!isServer) return;
        GunGameManager.Instance.weaponCount = weaponCount;
    }


    [QFSW.QC.Command("kill")]
    public void KillSelf()
    {
        if(LocalPlayerContext.Instance.player.isLocalPlayer)
            LocalPlayerContext.Instance.player.CmdKill();
    }

    [QFSW.QC.Command("mm_start")]
    public void Start_MonsterMode()
    {
        if (!isServer) return;
        MonsterModeManager.Instance.StartGame();
    }

    [QFSW.QC.Command("mm_stage_restTime")]
    public void MonsterMode_SetStageRestTime(int restTime)
    {
        if (!isServer) return;
        MonsterModeManager.Instance.StageRestTime = restTime;
    }

    [QFSW.QC.Command("mm_killsPerStage")]
    public void MonsterMode_SetKillsPerStage(int killCount)
    {
        if (!isServer) return;
        MonsterModeManager.Instance.StageTargetKillCount = killCount;
    }

    [QFSW.QC.Command("mm_health_multiplier_player")]
    public void MonsterMode_SetHealthMultiplierPerPlayer(float multiplier)
    {
        if (!isServer) return;
        MonsterModeManager.Instance.HealthMultiplierPerPlayer = multiplier;
    }

    [QFSW.QC.Command("mm_health_multiplier_stage")]
    public void MonsterMode_SetHealthMultiplierPerStage(float multiplier)
    {
        if (!isServer) return;
        MonsterModeManager.Instance.HealthMultiplierPerStage = multiplier;
    }

    [QFSW.QC.Command("db_reload")]
    public void ReloadDB()
    {
        if (!isServer) return;
        CoreGameManager.Instance.ReloadDatabase();
    }
}
