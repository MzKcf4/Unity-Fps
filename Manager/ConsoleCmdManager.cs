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
            PlayerManager.Instance.AddBot(TeamEnum.TeamA, skillLevel);
    }
    
    [QFSW.QC.Command("bot_add_b")]
    public void AddBot_TeamB(int skillLevel)
    {
        if(!isServer)   return;
        
        if(PlayerManager.Instance != null)
            PlayerManager.Instance.AddBot(TeamEnum.TeamB, skillLevel);
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
}
