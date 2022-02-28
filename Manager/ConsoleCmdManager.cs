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

    [QFSW.QC.Command("changelevel_sample")]
    public void ChangeLevel_sample()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("fy_sample");
    }

    [QFSW.QC.Command("changelevel_fy_snow")]
    public void ChangeLevel()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("fy_snow");
    }

    [QFSW.QC.Command("changelevel_ar_egypt")]
    public void ChangeLevel_ar()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("ar_egypt");
    }


    [QFSW.QC.Command("changelevel_yet")]
    public void ChangeLevel_yet()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("aim_yetanotheraimmap");
    }

    [QFSW.QC.Command("changelevel_cybergrap")]
    public void ChangeLevel_cyber()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("cybergrape");
    }

    [QFSW.QC.Command("changelevel_aim_space")]
    public void ChangeLevel_space()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("aim_space");
    }

    [QFSW.QC.Command("changelevel_aim_2077")]
    public void ChangeLevel_2()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("aim_allpistols");
    }

    [QFSW.QC.Command("changelevel_aim_2019")]
    public void ChangeLevel_3()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("aim_allpistols_2019");
    }

    [QFSW.QC.Command("changelevel_de_dust2_classic")]
    public void ChangeLevel_4()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("de_dust2_classic");
    }

    [QFSW.QC.Command("changelevel_cs_bloodstrike")]
    public void ChangeLevel_5()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("cs_bloodstrike");
    }

    [QFSW.QC.Command("changelevel_aim_rush")]
    public void ChangeLevel_6()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("aim_rush");
    }

    [QFSW.QC.Command("changelevel_aim_vertigo")]
    public void ChangeLevel_7()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("aim_vertigo");
    }

    [QFSW.QC.Command("db_reload")]
    public void ReloadDB()
    {
        if (!isServer) return;
        CoreGameManager.Instance.ReloadDatabase();
    }
}
