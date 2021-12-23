using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

// Holds the logic of the "Match" , such as : 
//    How the match starts 
//    How to determine which team wins
public class DeathMatchManager : NetworkBehaviour
{
    public static DeathMatchManager Instance;
    [SyncVar] public int currentScoreBlue;
    [SyncVar] public int currentScoreRed;
    [SyncVar] public int targetScore;
    [SyncVar] bool isMatchActive = true;
    [SerializeField] private TextMeshProUGUI uiTextScoreBlue;
    [SerializeField] private TextMeshProUGUI uiTextScoreRed;
    [SerializeField] private TextMeshProUGUI uiTextScoreTarget;
    
    void Awake()
    {
        Instance = this;
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        targetScore = 500;
        ServerContext.Instance.characterKilledEventServer.AddListener(OnCharacterKilled);
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateUiScore(currentScoreBlue , currentScoreRed , targetScore);
        SharedContext.Instance.characterSpawnEvent.AddListener(OnCharacterSpawn);
    }
        
    [Server]
    public void RestartMatch()
    {
        currentScoreBlue = 0;
        currentScoreRed = 0;
        isMatchActive = true;
        RpcUpdateScore(currentScoreBlue , currentScoreRed , targetScore);
    }
    
    [Server]
    public void OnCharacterKilled(FpsCharacter victim, DamageInfo dmgInfo)
    {    
        if(!isServer || !isMatchActive || string.IsNullOrEmpty(dmgInfo.damageWeaponName))  
            return;
            
        int killScore = GetWeaponKillScore(dmgInfo.damageWeaponName);
        
        if(victim.team == TeamEnum.Blue)
            currentScoreRed += killScore;
        else if (victim.team == TeamEnum.Red)
            currentScoreBlue += killScore;
        
        RpcUpdateScore(currentScoreBlue , currentScoreRed , targetScore);
    }
    
    private void CheckWinCondition(){
        if(currentScoreBlue >= targetScore || currentScoreRed >= targetScore)
            isMatchActive = false;
            
    }
    
    private int GetWeaponKillScore(string weaponName)
    {
        E_weapon_info dbWeaponInfo = E_weapon_info.GetEntity(weaponName);
        return dbWeaponInfo.f_dm_kill_score;
    }
    
    [ClientRpc]
    public void RpcUpdateScore(int newBlue , int newRed , int newTarget)
    {
        UpdateUiScore(newBlue , newRed , newTarget);
    }
        
    
    private void UpdateUiScore(int newBlue , int newRed , int newTarget)
    {
        uiTextScoreTarget.text = newTarget.ToString();
        uiTextScoreBlue.text = newBlue.ToString();
        uiTextScoreRed.text = newRed.ToString();
    }
    
    private void OnCharacterSpawn(FpsCharacter fpsCharacter)
    {
        if(fpsCharacter.isLocalPlayer)
        {
            string weaponName = LocalPlayerContext.Instance.GetAdditionalValue(Constants.ADDITIONAL_KEY_DM_SELECTED_WEAPON , "csgo_ak47");
            
            if(fpsCharacter.HasWeapon(weaponName))
                return;
            
            fpsCharacter.CmdGetWeapon(weaponName , 0);
        }
    }
    
}
