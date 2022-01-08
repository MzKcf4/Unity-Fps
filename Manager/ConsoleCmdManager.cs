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
    
    [QFSW.QC.Command("dm_restart")]
    public void RestartDeathMatch()
    {
        if(!isServer)   return;
        DeathMatchManager.Instance.RestartMatch();
    }

    [QFSW.QC.Command("kill")]
    public void KillSelf()
    {
        if(LocalPlayerContext.Instance.player.isLocalPlayer)
            LocalPlayerContext.Instance.player.CmdKill();
    }

    [QFSW.QC.Command("changelevel")]
    public void ChangeLevel()
    {
        if (!isServer) return;
        NetworkManager.singleton.ServerChangeScene("fy_iceworld");
        // NetworkSceneManager.Instance.ChangeLevel();
    }

}
