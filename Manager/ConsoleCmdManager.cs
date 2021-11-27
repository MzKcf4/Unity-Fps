using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ConsoleCmdManager : NetworkBehaviour
{
    [QFSW.QC.Command("bot_add_a")]
    public void AddBot_TeamA()
    {
        if(!isServer)   return;
        
        if(PlayerManager.Instance != null)
            PlayerManager.Instance.AddBot(TeamEnum.TeamA);
    }
    
    [QFSW.QC.Command("bot_add_b")]
    public void AddBot_TeamB()
    {
        if(!isServer)   return;
        
        if(PlayerManager.Instance != null)
            PlayerManager.Instance.AddBot(TeamEnum.TeamB);
    }
    
    [QFSW.QC.Command("bot_kick")]
    public void KickAllBot()
    {
        if(!isServer)   return;
        
        if(PlayerManager.Instance != null)
            PlayerManager.Instance.KickAllBot();
    }
}
