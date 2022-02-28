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
    [SerializeField] private AudioClip roundEndClip;
    
    void Awake()
    {
        Instance = this;
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        targetScore = 1500;
        ServerContext.Instance.characterKilledEventServer.AddListener(OnCharacterKilled);
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateUiScore(currentScoreBlue , currentScoreRed , targetScore);
        SharedContext.Instance.characterSpawnEvent.AddListener(OnCharacterSpawn);
    }

    [Server]
    public void UpdateTargetScore(int newScore)
    {
        targetScore = newScore;
        RpcUpdateScore(currentScoreBlue, currentScoreRed, targetScore);
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
        CheckWinCondition();
    }
    
    private void CheckWinCondition(){
        if (currentScoreBlue >= targetScore || currentScoreRed >= targetScore)
        {
            isMatchActive = false;
            RpcRoundEnd(currentScoreBlue >= targetScore);
            PlayerManager.Instance.KickAllBot();
        }   
    }

    [ClientRpc]
    private void RpcRoundEnd(bool isBlueWin)
    {
        if (isLocalPlayer && roundEndClip != null)
        {
            if (LocalPlayerContext.Instance.localPlayerAudioSource)
            {
                LocalPlayerContext.Instance.localPlayerAudioSource.PlayOneShot(roundEndClip);
            }

            if (isBlueWin)
            {
                uiTextScoreBlue.text = "Blue team wins";
                uiTextScoreRed.text = "";
                uiTextScoreTarget.text = "";
            }
            else
            {
                uiTextScoreBlue.text = "";
                uiTextScoreRed.text = "Blue team wins";
                uiTextScoreTarget.text = "";
            }
        }
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
        if (currentScoreBlue >= targetScore)
        {
            uiTextScoreBlue.text = "Blue team wins";
            uiTextScoreRed.text = "";
            uiTextScoreTarget.text = "";
        }
        else if (currentScoreRed >= targetScore)
        {
            uiTextScoreBlue.text = "";
            uiTextScoreRed.text = "Blue team wins";
            uiTextScoreTarget.text = "";
        }
        else
        {
            uiTextScoreTarget.text = newTarget.ToString();
            uiTextScoreBlue.text = newBlue.ToString();
            uiTextScoreRed.text = newRed.ToString();
        }
    }
    
    private void OnCharacterSpawn(FpsCharacter fpsCharacter)
    {

        if(fpsCharacter.isLocalPlayer && fpsCharacter is FpsHumanoidCharacter)
        {
            FpsHumanoidCharacter humanoidCharacter = (FpsHumanoidCharacter)fpsCharacter;
            string weaponNamePrimary = LocalPlayerContext.Instance.GetAdditionalValue(Constants.ADDITIONAL_KEY_DM_SELECTED_WEAPON , "csgo_ak47");
            string weaponNameSecondary = LocalPlayerContext.Instance.GetAdditionalValue(Constants.ADDITIONAL_KEY_DM_SELECTED_WEAPON_SECONDARY, "csgo_deagle");

            if (!humanoidCharacter.HasWeapon(weaponNamePrimary))
                humanoidCharacter.CmdGetWeapon(weaponNamePrimary, 0);

            if (!humanoidCharacter.HasWeapon(weaponNameSecondary))
                humanoidCharacter.CmdGetWeapon(weaponNameSecondary, 1);

        }
    }
    
}
