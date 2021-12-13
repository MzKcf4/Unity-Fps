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
    

}
