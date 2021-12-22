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
        UpdateUiScore();
    }
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    [Server]
    public void RestartMatch()
    {
        currentScoreBlue = 0;
        currentScoreRed = 0;
        isMatchActive = true;
        UpdateUiScore();
    }
    
    [Server]
    public void OnCharacterKilled(FpsCharacter victim, DamageInfo dmgInfo){
        if(!isMatchActive || string.IsNullOrEmpty(dmgInfo.damageWeaponName))  
            return;
            
        int killScore = GetWeaponKillScore(dmgInfo.damageWeaponName);
        
        if(victim.team == TeamEnum.Blue)
            currentScoreRed += killScore;
        else if (victim.team == TeamEnum.Red)
            currentScoreBlue += killScore;
        
        RpcUpdateScore();
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
    public void RpcUpdateScore()
    {
        UpdateUiScore();
    }
    
    private void UpdateUiScore()
    {
        uiTextScoreTarget.text = targetScore.ToString();
        uiTextScoreBlue.text = currentScoreBlue.ToString();
        uiTextScoreRed.text = currentScoreRed.ToString();
    }
    
}
